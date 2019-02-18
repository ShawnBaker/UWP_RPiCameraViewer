// Copyright © 2019 Shawn Baker using the MIT License.
using Windows.ApplicationModel.Resources;

namespace RPiCameraViewer
{
	public static class Res
	{
		// resource loader
		private static ResourceLoader res = ResourceLoader.GetForViewIndependentUse();

		// localized strings
		public static class Error
		{
			public static string BadAddress { get { return res.GetString("ErrorBadAddress"); } }
			public static string BadPort { get { return res.GetString("ErrorBadPort"); } }
			public static string BadScanTimeout { get { return res.GetString("ErrorBadScanTimeout"); } }
			public static string NameAlreadyExists { get { return res.GetString("ErrorNameAlreadyExists"); } }
			public static string NoAddress { get { return res.GetString("ErrorNoAddress"); } }
			public static string NoCameraName { get { return res.GetString("ErrorNoCameraName"); } }
		}

		// localized strings
		public static class Str
		{
			public static string AllNetworks { get { return res.GetString("AllNetworks"); } }
			public static string AppName { get { return res.GetString("AppName"); } }
			public static string Cancel { get { return res.GetString("Cancel"); } }
			public static string Copyright { get { return res.GetString("Copyright"); } }
			public static string Done { get { return res.GetString("Done"); } }
			public static string EditCamera { get { return res.GetString("EditCamera"); } }
			public static string Error { get { return res.GetString("Error"); } }
			public static string Camera { get { return res.GetString("Camera"); } }
			public static string GithubLink { get { return res.GetString("GithubLink"); } }
			public static string GithubText { get { return res.GetString("GithubText"); } }
			public static string HelpMessage { get { return res.GetString("HelpMessage"); } }
			public static string MartinBerubeLink { get { return res.GetString("MartinBerubeLink"); } }
			public static string MartinBerubeText { get { return res.GetString("MartinBerubeText"); } }
			public static string MITLink { get { return res.GetString("MITLink"); } }
			public static string MITText { get { return res.GetString("MITText"); } }
			public static string NetworkName { get { return res.GetString("NetworkName"); } }
			public static string NewCamera { get { return res.GetString("NewCamera"); } }
			public static string NewCamerasFound { get { return res.GetString("NewCamerasFound"); } }
			public static string No { get { return res.GetString("No"); } }
			public static string NoNetwork { get { return res.GetString("NoNetwork"); } }
			public static string OK { get { return res.GetString("OK"); } }
			public static string OkToDeleteAllCameras { get { return res.GetString("OkToDeleteAllCameras"); } }
			public static string OkToDeleteCamera { get { return res.GetString("OkToDeleteCamera"); } }
			public static string OxygenTeamLink { get { return res.GetString("OxygenTeamLink"); } }
			public static string OxygenTeamText { get { return res.GetString("OxygenTeamText"); } }
			public static string ScanningForCameras { get { return res.GetString("ScanningForCameras"); } }
			public static string ScanningOnPort { get { return res.GetString("ScanningOnPort"); } }
			public static string StreamingArticleLink { get { return res.GetString("StreamingArticleLink"); } }
			public static string StreamingArticleText { get { return res.GetString("StreamingArticleText"); } }
			public static string Version { get { return res.GetString("Version"); } }
			public static string Yes { get { return res.GetString("Yes"); } }
		}
	}
}
