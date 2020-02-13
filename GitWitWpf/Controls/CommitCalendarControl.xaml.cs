using GitWitWpf.Models;
using GitWitWpf.Services;
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
    /// Interaction logic for CommitCalendarControl.xaml
    /// </summary>
    public partial class CommitCalendarControl : UserControl
    {
        public CommitCalendarControl()
        {
            InitializeComponent();
        }

        private void AccessToken_Click(object sender, RoutedEventArgs e)
        {
            AccessTokenWindow atWindow = new AccessTokenWindow();
            atWindow.DataContext = ((CommitCalendarModel)DataContext).Settings;
            atWindow.ShowDialog();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _ = ((CommitCalendarModel)DataContext).Refresh();
        }
    }
}
