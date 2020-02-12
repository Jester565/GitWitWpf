using System;
using System.Collections.Generic;
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
            System.Diagnostics.Process.Start(HELP_URL);
        }
    }
}
