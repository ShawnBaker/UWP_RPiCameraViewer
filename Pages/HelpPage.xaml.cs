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
		/// Constructor - Initializes the page.
		/// </summary>
		public HelpPage()
        {
            InitializeComponent();
			Loaded += HandleLoaded;
		}

		/// <summary>
		/// Initializes the controls.
		/// </summary>
		private void HandleLoaded(object sender, RoutedEventArgs e)
		{
			// set the help text
			Log.Info("+HelpPage.HandleLoaded");
			var links = new Dictionary<string, string>
			{
				{ Res.Str.StreamingArticleText, Res.Str.StreamingArticleLink }
			};
			Utils.CreateInlines(helpTextBlock, Res.Str.HelpMessage, links);
			Log.Info("-HelpPage.HandleLoaded");
		}

		/// <summary>
		/// Return to the previous page.
		/// </summary>
		private void HandleBackButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("HelpPage.HandleBackButtonClick");
			Frame.GoBack();
		}
	}
}
