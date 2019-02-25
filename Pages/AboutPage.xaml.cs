// Copyright © 2019 Shawn Baker using the MIT License.
using Windows.UI.Xaml;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;

namespace RPiCameraViewer
{
    /// <summary>
    /// About page.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
		/// <summary>
		/// Constructor - Initializes the page.
		/// </summary>
		public AboutPage()
		{
			InitializeComponent();
			Loaded += HandleLoaded;
		}

		/// <summary>
		/// Initializes the controls.
		/// </summary>
		private void HandleLoaded(object sender, RoutedEventArgs e)
		{
			// set the name and version
			Log.Info("+AboutPage");
			nameTextBlock.Text = Res.Str.AppName;
			PackageVersion version = Package.Current.Id.Version;
			string ver = string.Format("{0} {1}.{2}", Res.Str.Version, version.Major, version.Minor);
			if (version.Build != 0)
			{
				ver += "." + version.Build.ToString();
			}
			if (version.Revision != 0)
			{
				ver += "." + version.Revision.ToString();
			}
			versionTextBlock.Text = ver;
			Log.Info("AboutPage: {0}", ver);

			// set the copyright text
			var links = new Dictionary<string, string>
			{
				{ Res.Str.MartinBerubeText, Res.Str.MartinBerubeLink },
				{ Res.Str.OxygenTeamText, Res.Str.OxygenTeamLink },
				{ Res.Str.ShaderrowText, Res.Str.ShaderrowLink },
				{ Res.Str.AutisticLucarioText, Res.Str.AutisticLucarioLink },
				{ Res.Str.GithubText, Res.Str.GithubLink },
				{ Res.Str.MITText, Res.Str.MITLink }
			};
			Utils.CreateInlines(copyrightTextBlock, Res.Str.Copyright, links);
			Log.Info("-AboutPage");
		}

		/// <summary>
		/// Return to the previous page.
		/// </summary>
		private void HandleBackButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("AboutPage.HandleBackButtonClick");
			Frame.GoBack();
		}
	}
}
