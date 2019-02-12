// Copyright © 2019 Shawn Baker using the MIT License.
using System;
using Windows.UI.Xaml;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

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

			// set the copyright
			copyrightTextBlock.Text = "Copyright \u00A9 2019 Shawn Baker";

			// set the open source attributions
			openSourceTextBlock.Inlines.Clear();
			openSourceTextBlock.Inlines.Add(new Run() { Text = Res.Str.OpenSource });
			openSourceTextBlock.Inlines.Add(new LineBreak());
			Hyperlink github = new Hyperlink() { NavigateUri = new Uri(@"https://github.com/ShawnBaker/UWP_RPiCameraViewer") };
			github.Inlines.Add(new Run() { Text = "github" });
			Hyperlink mit = new Hyperlink() { NavigateUri = new Uri(@"https://opensource.org/licenses/MIT") };
			mit.Inlines.Add(new Run() { Text = "MIT" });
			int githubIndex = Res.Str.GithubMIT.IndexOf("github");
			int mitIndex = Res.Str.GithubMIT.IndexOf("MIT");
			bool mitFirst = mitIndex < githubIndex;
			int first = Math.Min(githubIndex, mitIndex);
			int second = Math.Max(githubIndex, mitIndex);
			int index = first;
			if (index != 0)
			{
				openSourceTextBlock.Inlines.Add(new Run() { Text = Res.Str.GithubMIT.Substring(0, index) });
				index += mitFirst ? 3 : 6;
			}
			openSourceTextBlock.Inlines.Add(mitFirst ? mit : github);
			if (index != second)
			{
				int len = second - index;
				openSourceTextBlock.Inlines.Add(new Run() { Text = Res.Str.GithubMIT.Substring(index, len) });
				index += len;
			}
			openSourceTextBlock.Inlines.Add(mitFirst ? github : mit);
			index += mitFirst ? 6 : 3;
			if (index < Res.Str.GithubMIT.Length)
			{
				openSourceTextBlock.Inlines.Add(new Run() { Text = Res.Str.GithubMIT.Substring(index) });
			}
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
