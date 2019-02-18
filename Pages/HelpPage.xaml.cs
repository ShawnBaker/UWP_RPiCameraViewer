// Copyright © 2019 Shawn Baker using the MIT License.
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

			// set the help text
			var links = new Dictionary<string, string>
			{
				{ Res.Str.StreamingArticleText, Res.Str.StreamingArticleLink }
			};
			Utils.CreateInlines(helpTextBlock, Res.Str.HelpMessage, links);
		}

		/// <summary>
		/// Return to the previous page.
		/// </summary>
		private void HandleBackButtonClick(object sender, RoutedEventArgs e)
		{
			Frame.GoBack();
		}
	}
}
