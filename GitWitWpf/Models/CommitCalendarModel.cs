using GitWitWpf.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static GitWitWpf.Services.GitService;

namespace GitWitWpf.Models
{
    public class CommitCalendarModel: ObservableObject
    {
        public class CommitDay
        {
            private List<CommitData> _commits = new List<CommitData>();
            public string Summary
            {
                get {
                    string summary = "";
                    foreach (var it in _commits.Select((x, i) => new { Value = x, Index = i }))
                    {
                        CommitData commit = it.Value;
                        summary += commit.DateTime.ToString("HH:mm");
                        summary += " - ";
                        summary += commit.Msg;
                        if (it.Index < _commits.Count - 1)
                        {
                            summary += "\n\n";
                        }
                    }
                    return summary;
                }
            }

            public string Label
            {
                get { return Date.ToString("M/d"); }
            }
            public DateTime Date { get; set; }
            public List<CommitData> Commits
            {
                get { return _commits; }
                set { _commits = value; }
            }
        }
        public class CommitWeek
        {
            public string Label
            {
                get { return StartDate.ToString("M/d"); }
            }
            public DateTime StartDate { get; set; }
            public List<CommitDay> Days { get; set; }
        }
        private static readonly string PREV_DATA_PATH = @".\weeks.json";
        private SettingsModel _settingsModel;
        private GitService _gitService;
        private List<CommitWeek> _weeks;
        private bool _loading = false;
        private bool _showRefresh = false;
        private string _errorMsg = null;
        private bool _isAccessTokenError = false;
        private CancellationTokenSource _cancelSource = null;
        //Refresh git calendar every 10 minutes
        private const int DEFAULT_REFERSH_INTERVAL = 60 * 10;
        private System.Timers.Timer _pollTimer = new System.Timers.Timer();
        private static readonly List<string> DOW_LABELS = new List<string> { "U", "M", "T", "W", "R", "F", "S" };
        public CommitCalendarModel(SettingsModel settingsModel)
        {
            _settingsModel = settingsModel;
        }

        public void Init()
        {
            _settingsModel.PropertyChanged += OnSettingsChanged;
            _gitService = new GitService();
            //TODO: Handle no access token
            if (_settingsModel.AccessToken != null)
            {
                _gitService.SetHttpClient(_settingsModel.AccessToken);
            }
            _pollTimer.AutoReset = true;
            _pollTimer.Elapsed += new ElapsedEventHandler(this.OnPoll);
            _ = LoadPreviousData();
            _ = Refresh();
            StartPolling();
        }

        public SettingsModel Settings
        {
            get { return _settingsModel; }
        }

        public List<string> DowLabels
        {
            get { return DOW_LABELS; }
        }

        public List<CommitWeek> Weeks
        {
            get
            {
                return _weeks;
            }
            set
            {
                if (value != _weeks)
                {
                    _weeks = value;
                    OnPropertyChanged("Weeks");
                }
            }
        }

        public bool Loading
        {
            get
            {
                return _loading;
            }
            set
            {
                _loading = value;
                OnPropertyChanged("Loading");
            }
        }

        public bool ShowRefresh
        {
            get { return _showRefresh; }
            set
            {
                _showRefresh = value;
                OnPropertyChanged("ShowRefresh");
            }
        }

        public bool IsAccessTokenError
        {
            get { return _isAccessTokenError; }
            set
            {
                _isAccessTokenError = value;
                OnPropertyChanged("IsAccessTokenError");
            }
        }

        public string ErrorMsg
        {
            get
            {
                return _errorMsg;
            }
            set
            {
                _errorMsg = value;
                OnPropertyChanged("ErrorMsg");
            }
        }

        private async Task LoadPreviousData()
        {
            if (File.Exists(PREV_DATA_PATH))
            {
                string weeksStr = await File.ReadAllTextAsync(PREV_DATA_PATH);
                Weeks = JsonConvert.DeserializeObject<List<CommitWeek>>(weeksStr);
            }
        }

        private async Task WriteData(List<CommitWeek> commitWeeks)
        {
            string weeksStr = JsonConvert.SerializeObject(commitWeeks);
            await File.WriteAllTextAsync(PREV_DATA_PATH, weeksStr);
        }

        public void StartPolling(int refreshInterval = DEFAULT_REFERSH_INTERVAL)
        {
            _pollTimer.Interval = 1000 * refreshInterval; // in miliseconds
            _pollTimer.Start();
        }

        public void StopPolling()
        {
            _pollTimer.Stop();
        }

        public async Task Refresh()
        {
            ErrorMsg = null;
            IsAccessTokenError = false;
            ShowRefresh = false;
            if (_cancelSource != null)
            {
                _cancelSource.Cancel();
            }
            _cancelSource = new CancellationTokenSource();
            //Check that user set their access token
            if (!String.IsNullOrEmpty(_settingsModel.AccessToken))
            {
                Loading = true;
                try
                {
                    Weeks = await _GetData(_cancelSource.Token);
                    _ = WriteData(Weeks);
                    Loading = false;
                    ShowRefresh = true;
                }
                catch (TaskCanceledException e)
                {
                    System.Diagnostics.Debug.WriteLine("Refresh cancelled");
                }
                catch (HttpRequestException e)
                {
                    if (e.Data.Contains("StatusCode") && (int)e.Data["StatusCode"] == 401)
                    {
                        ErrorMsg = "Invalid Access Token";
                        IsAccessTokenError = true;
                    } else
                    {
                        ErrorMsg = "Could not access GitHub";
                        ShowRefresh = true;
                    }
                    Loading = false;
                }
            } else
            {
                ErrorMsg = "Please Provide An Access Token";
                IsAccessTokenError = true;
            }
        }

        private async Task<List<CommitWeek>> _GetData(CancellationToken ct)
        {
            string username = await _gitService.GetUsername(ct);
            int numWeeks = (int)_settingsModel.NumWeeks;
            DateTime startDateTime = GetDayOfWeek(DateTime.Now.ToLocalTime().AddDays(-(numWeeks - 1) * 7), DayOfWeek.Sunday);
            List<CommitData> commits = await _gitService.GetRecentCommits(username, startDateTime, ct);
            return CommitsToWeeks(commits, numWeeks);
        }

        private List<CommitWeek> CommitsToWeeks(List<CommitData> commits, int numWeeks)
        {
            List<CommitWeek> commitWeeks = new List<CommitWeek>();
            DateTime latestWeekStart = GetDayOfWeek(DateTime.Now.ToLocalTime(), DayOfWeek.Sunday);
            for (int i = -numWeeks + 1; i <= 0; i++)
            {
                DateTime weekStart = latestWeekStart.AddDays(7 * i).Date;
                int nowToStartOfWeekDays = (int)((DateTime.Now.ToLocalTime().Date - weekStart).TotalDays);
                int numDaysInWeek = (nowToStartOfWeekDays > 6) ? 6 : nowToStartOfWeekDays;
                List<CommitDay> commitDays = new List<CommitDay>();
                for (int j = 0; j <= numDaysInWeek; j++)
                {
                    commitDays.Add(new CommitDay
                    {
                        Date = weekStart.AddDays(j).Date
                    });
                }
                commitWeeks.Add(new CommitWeek
                {
                    StartDate = weekStart,
                    Days = commitDays
                });
            }
            foreach (CommitData commit in commits)
            {
                int dayDiff = (int)(latestWeekStart.Date - commit.DateTime.Date).TotalDays + 7;
                int weeksOff = (int)Math.Ceiling(dayDiff / 7.0);
                int weekI = numWeeks - weeksOff;
                int dayI = (int)commit.DateTime.DayOfWeek;
                if (weekI >= 0)
                {
                    commitWeeks[weekI].Days[dayI].Commits.Add(commit);
                }
            }
            return commitWeeks;
        }


        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            //If WindowPosition changed, reset windowposition
            if (e.PropertyName == "NumWeeks")
            {
                _ = Refresh();
            } else if (e.PropertyName == "AccessToken")
            {
                _gitService.SetHttpClient(_settingsModel.AccessToken);
                _ = Refresh();
            }
            //TODO: Handle only username change
        }

        private void OnPoll(object sender, ElapsedEventArgs e)
        {
            _ = this.Refresh();
        }

        private DateTime GetDayOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
