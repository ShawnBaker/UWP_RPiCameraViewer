// Copyright © 2019 Shawn Baker using the MIT License.
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace RPiCameraViewer
{
    /// <summary>
    /// Help page.
    /// </summary>
    public sealed partial class HelpPage : Page
    {
		/// <summary>
		/// Constructor - Initializes the controls.
		/// </summary>
		public HelpPage()
        {
            InitializeComponent();

			helpTextBlock.Inlines.Clear();
			string str = Res.Str.HelpMessage;
			int i = str.IndexOf("<b>");
			while (i != -1)
			{
				if (i > 0)
				{
					helpTextBlock.Inlines.Add(new Run() { Text = str.Substring(0, i) });
				}
				int j = str.IndexOf("</b>", i + 3);
				helpTextBlock.Inlines.Add(new Run() { Text = str.Substring(i + 3, j - i - 3), FontWeight = FontWeights.Bold });
				str = str.Substring(j + 4);
				i = str.IndexOf("<b>");
			}
			if (str.Length > 0)
			{
				helpTextBlock.Inlines.Add(new Run() { Text = str });
			}
		}

		/// <summary>
		/// Return to the previous page.
		/// </summary>
		private void HandleBackButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			Frame.GoBack();
		}
	}
}
