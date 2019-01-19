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
			public static string NoCameraName { get { return res.GetString("ErrorNoCameraName"); } }
			public static string BadAddress { get { return res.GetString("ErrorBadAddress"); } }
			public static string BadScanTimeout { get { return res.GetString("ErrorBadScanTimeout"); } }
			public static string BadPort { get { return res.GetString("ErrorBadPort"); } }
			public static string NameAlreadyExists { get { return res.GetString("ErrorNameAlreadyExists"); } }
			public static string NoAddress { get { return res.GetString("ErrorNoAddress"); } }
		}

		// localized strings
		public static class Str
		{
			public static string AllNetworks { get { return res.GetString("AllNetworks"); } }
			public static string AppName { get { return res.GetString("AppName"); } }
			public static string EditCamera { get { return res.GetString("EditCamera"); } }
			public static string Error { get { return res.GetString("Error"); } }
			public static string Camera { get { return res.GetString("Camera"); } }
			public static string GithubMIT { get { return res.GetString("GithubMIT"); } }
			public static string HelpMessage { get { return res.GetString("HelpMessage"); } }
			public static string NetworkName { get { return res.GetString("NetworkName"); } }
			public static string NewCamera { get { return res.GetString("NewCamera"); } }
			public static string No { get { return res.GetString("No"); } }
			public static string NoNetwork { get { return res.GetString("NoNetwork"); } }
			public static string OkToDeleteAllCameras { get { return res.GetString("OkToDeleteAllCameras"); } }
			public static string OkToDeleteCamera { get { return res.GetString("OkToDeleteCamera"); } }
			public static string OpenSource { get { return res.GetString("OpenSource"); } }
			public static string Version { get { return res.GetString("Version"); } }
			public static string Yes { get { return res.GetString("Yes"); } }
		}
	}
}
