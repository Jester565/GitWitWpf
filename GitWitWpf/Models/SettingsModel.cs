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

        private class Settings
        {
            public ScreenPosition WindowPosition = DEFAULT_WINDOW_POSITION;
            public int NumWeeks = DEFAULT_NUM_WEEKS;
            public string Username = String.Empty;
            public string AccessToken = String.Empty;
        }

        private Settings? _settings = null;
        private static readonly string SETTINGS_PATH = @".\settings.json";
        private static readonly ScreenPosition DEFAULT_WINDOW_POSITION = ScreenPosition.TopRight;
        private static readonly int DEFAULT_NUM_WEEKS = 4;
        private static int MAX_WEEKS = 10;
        private static int MIN_WEEKS = 1;
        public SettingsModel()
        {
        }

        public async Task Init()
        {
            if (File.Exists(@".\settings.json"))
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

        public string Username
        {
            get
            {
                return (_settings != null) ? _settings.Username : null;
            }
            set
            {
                if (_settings != null && value != null && value != _settings.Username)
                {
                    _settings.Username = value;
                    OnPropertyChanged("Username");
                    _ = WriteSettings();
                }
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
    }
}
