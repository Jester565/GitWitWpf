using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitWitWpf.Controls
{
    /// <summary>
    /// Interaction logic for AccessTokenControl.xaml
    /// </summary>
    public partial class AccessTokenControl : UserControl
    {
        private static readonly string HELP_URL = "https://help.github.com/en/github/authenticating-to-github/creating-a-personal-access-token-for-the-command-line";
        public AccessTokenControl()
        {
            InitializeComponent();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            LaunchBrowser(HELP_URL);
        }

        private void LaunchBrowser(string url)
        {
            using (RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
            {
                if (userChoiceKey != null)
                {
                    object progIdValue = userChoiceKey.GetValue("Progid");
                    string path = progIdValue + @"\shell\open\command";
                    FileInfo browserPath;
                    using (RegistryKey pathKey = Registry.ClassesRoot.OpenSubKey(path))
                    {
                        if (pathKey == null)
                        {
                            return;
                        }

                        // Trim parameters.
                        try
                        {
                            path = pathKey.GetValue(null).ToString().ToLower().Replace("\"", "");
                            if (!path.EndsWith(".exe"))
                            {
                                path = path.Substring(0, path.LastIndexOf(".exe", StringComparison.Ordinal) + ".exe".Length);
                            }
                            browserPath = new FileInfo(path);
                            Process.Start(new ProcessStartInfo(browserPath.FullName, url));
                        }
                        catch
                        {
                            ShowNoBrowserWarning();
                        }
                    }
                } else
                {
                    ShowNoBrowserWarning();
                }
            }
        }

        private void ShowNoBrowserWarning()
        {
            MessageBoxResult result = MessageBox.Show("We couldn't find your default browser. Would you like the link copied to your clipboard?",
                                         "Confirmation",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Clipboard.SetText(HELP_URL);
            }
        }
    }
}
