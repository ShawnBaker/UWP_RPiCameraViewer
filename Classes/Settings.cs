// Copyright © 2019 Shawn Baker using the MIT License.
using Windows.Storage;

namespace RPiCameraViewer
{
	public class Settings
	{
		// public constants
		public const int MIN_TIMEOUT = 100;
		public const int MAX_TIMEOUT = 5000;
		public const int DEFAULT_TIMEOUT = 500;
		public const int MIN_PORT = 1024;
		public const int MAX_PORT = 65535;
		public const int DEFAULT_PORT = 5001;

		// instance variables
		private ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

		/// <summary>
		/// Constructor - Loads the data from ApplicationData.Current.LocalSettings.
		/// </summary>
		public Settings()
		{
			if (settings.Values["CameraName"] == null)
			{
				settings.Values["CameraName"] = Res.Str.Camera;
				settings.Values["ScanTimeout"] = DEFAULT_TIMEOUT;
				settings.Values["Port"] = DEFAULT_PORT;
				settings.Values["ShowAllNetworks"] = false;
				settings.Values["Cameras"] = "";
			}

			CameraName = (string)settings.Values["CameraName"];
			ScanTimeout = (int)settings.Values["ScanTimeout"];
			Port = (int)settings.Values["Port"];
			ShowAllNetworks = (bool)settings.Values["ShowAllNetworks"];
			string camerasString = (string)settings.Values["Cameras"];
			Cameras = new Cameras();
			if (!string.IsNullOrEmpty(camerasString))
			{
				string[] parts = camerasString.Split(',');
				for (int i = 0; i < parts.Length; i += 4)
				{
					Camera camera = new Camera();
					camera.Network = parts[i];
					camera.Name = parts[i + 1];
					camera.Address = parts[i + 2];
					camera.Port = int.Parse(parts[i + 3]);
					Cameras.Add(camera);
				}
			}
		}

		// public properties
		public string CameraName { get; set; }
		public int ScanTimeout { get; set; }
		public int Port { get; set; }
		public bool ShowAllNetworks { get; set; }
		public Cameras Cameras { get; set; }

		/// <summary>
		/// Saves the data to ApplicationData.Current.LocalSettings.
		/// </summary>
		public void Save()
		{
			settings.Values["CameraName"] = CameraName;
			settings.Values["ScanTimeout"] = ScanTimeout;
			settings.Values["Port"] = Port;
			settings.Values["ShowAllNetworks"] = ShowAllNetworks;
			string camerasString = "";
			foreach (Camera camera in Cameras)
			{
				if (camerasString.Length > 0)
				{
					camerasString += ",";
				}
				camerasString += camera.Network + "," + camera.Name + "," + camera.Address + "," + camera.Port;
			}
			settings.Values["Cameras"] = camerasString;
		}
	}
}
