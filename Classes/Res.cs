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
			public static string BadAddress => res.GetString("ErrorBadAddress");
			public static string BadPort => res.GetString("ErrorBadPort");
			public static string BadScanTimeout => res.GetString("ErrorBadScanTimeout");
			public static string CouldntConnect => res.GetString("ErrorCouldntConnect");
			public static string CouldntCreateZipFile => res.GetString("CouldntCreateZipFile");
			public static string LostConnection => res.GetString("ErrorLostConnection");
			public static string NameAlreadyExists => res.GetString("ErrorNameAlreadyExists");
			public static string NoAddress => res.GetString("ErrorNoAddress");
			public static string NoCameraName => res.GetString("ErrorNoCameraName");
		}

		// localized strings
		public static class Str
		{
			public static string AllNetworks => res.GetString("AllNetworks");
			public static string AppName => res.GetString("AppName");
			public static string AttachmentWarning => res.GetString("AttachmentWarning");
			public static string AutisticLucarioLink => res.GetString("AutisticLucarioLink");
			public static string AutisticLucarioText => res.GetString("AutisticLucarioText");
			public static string Cancel => res.GetString("Cancel");
			public static string ClosingVideo => res.GetString("ClosingVideo");
			public static string Copyright => res.GetString("Copyright");
			public static string Done => res.GetString("Done");
			public static string EditCamera => res.GetString("EditCamera");
			public static string Error => res.GetString("Error");
			public static string Camera => res.GetString("Camera");
			public static string GithubLink => res.GetString("GithubLink");
			public static string GithubText => res.GetString("GithubText");
			public static string HelpMessage => res.GetString("HelpMessage");
			public static string InitializingVideo => res.GetString("InitializingVideo");
			public static string LogFiles => res.GetString("LogFilesSubject");
			public static string MartinBerubeLink => res.GetString("MartinBerubeLink");
			public static string MartinBerubeText => res.GetString("MartinBerubeText");
			public static string MITLink => res.GetString("MITLink");
			public static string MITText => res.GetString("MITText");
			public static string NetworkName => res.GetString("NetworkName");
			public static string NewCamera => res.GetString("NewCamera");
			public static string NewCamerasFound => res.GetString("NewCamerasFound");
			public static string No => res.GetString("No");
			public static string NoNetwork => res.GetString("NoNetwork");
			public static string OK => res.GetString("OK");
			public static string OkToClearLogs => res.GetString("OkToClearLogs");
			public static string OkToDeleteAllCameras => res.GetString("OkToDeleteAllCameras");
			public static string OkToDeleteCamera => res.GetString("OkToDeleteCamera");
			public static string OxygenTeamLink => res.GetString("OxygenTeamLink");
			public static string OxygenTeamText => res.GetString("OxygenTeamText");
			public static string ScanningForCameras => res.GetString("ScanningForCameras");
			public static string ScanningOnPort => res.GetString("ScanningOnPort");
			public static string ShaderrowLink => res.GetString("ShaderrowLink");
			public static string ShaderrowText => res.GetString("ShaderrowText");
			public static string StreamingArticleLink => res.GetString("StreamingArticleLink");
			public static string StreamingArticleText => res.GetString("StreamingArticleText");
			public static string Version => res.GetString("Version");
			public static string Yes => res.GetString("Yes");
		}
	}
}
