using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace GitWitWpf.Models
{
    public class SettingsModel: ObservableObject
    {
        private static SettingsModel _instance = null;
        public static SettingsModel GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SettingsModel();
            }
            return _instance;
        }

        public enum ScreenPosition : ushort
        {
            TopLeft = 0,
            TopRight = 1,
            BottomLeft = 2,
            BottomRight = 3
        }
        public static Dictionary<ScreenPosition, string> ScreenPositionNames = new Dictionary<ScreenPosition, string> {
            { ScreenPosition.TopLeft, "Top Left" },
            { ScreenPosition.TopRight, "Top Right" },
            { ScreenPosition.BottomLeft, "Bottom Left" },
            { ScreenPosition.BottomRight, "Bottom Right" }
        };

        private static Dictionary<string, int> _refreshIntervals = new Dictionary<string, int>
        {
            { "10 Minutes", 10 },
            { "30 Minutes", 30 },
            { "1 Hour", 60 },
            { "2 Hours", 120 },
            { "4 Hours", 240 },
            { "6 Hours", 360 },
            { "8 Hours", 480 },
            { "12 Hours", 720 },
            { "1 Day", 1440 },
            { "Never", 0 }
        };

        private class Settings
        {
            public ScreenPosition WindowPosition = DEFAULT_WINDOW_POSITION;
            public int NumWeeks = DEFAULT_NUM_WEEKS;
            public string Username = String.Empty;
            public string AccessToken = String.Empty;
            public int RefreshInterval = DEFAULT_REFRESH_INTERVAL;
        }

        private Settings? _settings = null;
        private static readonly string SETTINGS_PATH = @".\settings.json";
        private static readonly ScreenPosition DEFAULT_WINDOW_POSITION = ScreenPosition.TopRight;
        private static readonly int DEFAULT_NUM_WEEKS = 4;
        private static readonly int DEFAULT_REFRESH_INTERVAL = 60;
        private static int MAX_WEEKS = 10;
        private static int MIN_WEEKS = 1;
        public SettingsModel()
        {
        }

        public async Task Init()
        {
            if (File.Exists(SETTINGS_PATH))
            {
                string settingsStr = await File.ReadAllTextAsync(SETTINGS_PATH);
                _settings = JsonConvert.DeserializeObject<Settings>(settingsStr);
            } else
            {
                _settings = new Settings();
            }
        }

        public async Task WriteSettings() {
            await File.WriteAllTextAsync(SETTINGS_PATH, JsonConvert.SerializeObject(_settings));
        }

        public List<KeyValuePair<ScreenPosition, string>> AllScreenPositions
        {
            get
            {
                return ScreenPositionNames.ToList();
            }
        }

        public ScreenPosition? WindowPosition
        {
            get
            {
                return (_settings != null)? _settings.WindowPosition: (ScreenPosition?) null;
            }
            set
            {
                if (_settings != null && value != null && value != _settings.WindowPosition)
                {
                    _settings.WindowPosition = (ScreenPosition)value;
                    OnPropertyChanged("WindowPosition");
                    _ = WriteSettings();
                }
            }
        }
        public int? NumWeeks
        {
            get
            {
                return (_settings != null) ? _settings.NumWeeks : (int?)null;
            }
            set
            {
                if (_settings != null && value != null && value != _settings.NumWeeks)
                {
                    _settings.NumWeeks = (int)value;
                    OnPropertyChanged("NumWeeks");
                    _ = WriteSettings();
                }
            }
        }

        public List<KeyValuePair<string, int>> AllWeekCounts
        {
            get
            {
                List<KeyValuePair<string, int>> WeekCounts = new List<KeyValuePair<string, int>>();
                for (int i = MIN_WEEKS; i <= MAX_WEEKS; i++)
                {
                    WeekCounts.Add(new KeyValuePair<string, int>(i.ToString(), i));
                }
                return WeekCounts;
            }
        }

        public string AccessToken
        {
            get
            {
                return (_settings != null) ? _settings.AccessToken : null;
            }
            set
            {
                if (_settings != null && value != null && value != _settings.AccessToken)
                {
                    _settings.AccessToken = value;
                    OnPropertyChanged("AccessToken");
                    _ = WriteSettings();
                }
            }
        }

        public List<KeyValuePair<string, int>> AllRefreshIntervals
        {
            get
            {
                return _refreshIntervals.ToList();
            }
        }

        public int RefreshInterval
        {
            get
            {
                return (_settings != null) ? _settings.RefreshInterval : DEFAULT_REFRESH_INTERVAL;
            }
            set
            {
                if (_settings != null && value != _settings.RefreshInterval)
                {
                    _settings.RefreshInterval = value;
                    OnPropertyChanged("RefreshInterval");
                }
            }
        }
    }
}
