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
		/// Constructor - Initializes the controls.
		/// </summary>
		public AboutPage()
        {
            InitializeComponent();

			// set the name and version
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

			// set the copyright text
			var links = new Dictionary<string, string>
			{
				{ Res.Str.MartinBerubeText, Res.Str.MartinBerubeLink },
				{ Res.Str.OxygenTeamText, Res.Str.OxygenTeamLink },
				{ Res.Str.GithubText, Res.Str.GithubLink },
				{ Res.Str.MITText, Res.Str.MITLink }
			};
			Utils.CreateInlines(copyrightTextBlock, Res.Str.Copyright, links);
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
