using GitWitWpf.Models;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GitWitWpf.Controls
{
	public class CommitsToColorConverter : IValueConverter
	{
		private static int MIN_G_VAL = 100;
		private static int MAX_COMMIT_SHOWN = 6;
		private static int COMMIT_INCR = 1;
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			int numCommits = (int)value;
			if (numCommits == 0)
			{
				Color c = new Color();
				c.R = 100; c.G = 100; c.B = 100; c.A = 100;
				return new SolidColorBrush(c);
			} else
			{
				Color c = new Color();
				int numShownCommits = (numCommits > MAX_COMMIT_SHOWN) ? MAX_COMMIT_SHOWN : numCommits;

				c.R = 0; 
				c.G = (byte)((int)((255 - MIN_G_VAL) * Math.Ceiling(((double)numShownCommits) / COMMIT_INCR) / MAX_COMMIT_SHOWN + MIN_G_VAL));
				c.B = 0; c.A = 200;
				return new SolidColorBrush(c);
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	/// <summary>
	/// Interaction logic for CommitDayControl.xaml
	/// </summary>
	public partial class CommitDayControl : UserControl
    {

		public CommitDayControl()
		{
			InitializeComponent();
		}
    }
}
