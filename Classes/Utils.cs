// Copyright © 2019 Shawn Baker using the MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace RPiCameraViewer
{
    public static class Utils
    {
		// public properties
		public static Color PrimaryColor = Color.FromArgb(255, 195, 30, 30);
		public static Color TextColor = Colors.White;
		public static Color GoodColor = Color.FromArgb(255, 0, 192, 0);
		public static Color BadColor = Colors.Red;
		public static string IpAddressRegexString = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
		public static string HostnameRegexString = "^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\\-]*[A-Za-z0-9])$";
		public static Regex IpAddressRegex = new Regex(IpAddressRegexString);
		public static Regex HostnameRegex = new Regex(HostnameRegexString);

		// static variables
		private static MediaElement soundPlayer = new MediaElement();

		/// <summary>
		/// Gets the name of the network that this computer is on.
		/// </summary>
		/// <returns></returns>
		public static string GetNetworkName()
		{
			ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
			return (profile != null) ? profile.ProfileName : "";
		}

		/// <summary>
		/// Determines whether or not this computer is on a network.
		/// </summary>
		/// <returns>True if this computer is on a network, false if not.</returns>
		public static bool ConnectedToNetwork()
		{
			return !string.IsNullOrEmpty(GetNetworkName());
		}

		/// <summary>
		/// Gets the IP address of this computer on the local network.
		/// </summary>
		/// <returns>The IP address of this computer.</returns>
		public static string GetLocalIpAddress()
		{
			ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
			if (profile != null && profile.NetworkAdapter != null)
			{
				HostName hostname = NetworkInformation.GetHostNames().FirstOrDefault(hn =>
										hn.Type == HostNameType.Ipv4 &&
										hn.IPInformation?.NetworkAdapter != null &&
										hn.IPInformation.NetworkAdapter.NetworkAdapterId == profile.NetworkAdapter.NetworkAdapterId);
				if (hostname != null)
				{
					return hostname.CanonicalName;
				}
			}

			// indicate failure
			return "";
		}

		/// <summary>
		/// Gets the base IP address of the network that this computer is on.
		/// </summary>
		/// <returns>The base IP address of the network that this computer is on.</returns>
		public static string GetBaseIpAddress()
		{
			string ipAddress = GetLocalIpAddress();
			if (!string.IsNullOrEmpty(ipAddress))
			{
				int i = ipAddress.LastIndexOf('.');
				if (i != -1)
				{
					return ipAddress.Substring(0, i + 1);
				}
			}
			return "";
		}

		/// <summary>
		/// Determines if an address is a valid IPV4 address.
		/// </summary>
		/// <param name="address">Address to be checked.</param>
		/// <returns>True if the address is a valid IPV4 address, false if not.</returns>
		public static bool IsIpAddress(string address)
		{
			return IpAddressRegex.IsMatch(address);
		}

		/// <summary>
		/// Determines if an address is a valid host name.
		/// </summary>
		/// <param name="address">Address to be checked.</param>
		/// <returns>True if the address is a valid host name, false if not.</returns>
		public static bool IsHostname(string address)
		{
			return HostnameRegex.IsMatch(address);
		}

		/// <summary>
		/// Get the highest number from the existing camera names.
		/// </summary>
		/// <param name="cameras">List of cameras to get the highest number from.</param>
		/// <returns>Highest camera number.</returns>
		public static int GetHighestCameraNumber(Cameras cameras)
		{
			int highest = 0;
			Settings settings = new Settings();
			String defaultName = settings.CameraName + " ";
			foreach (Camera camera in cameras)
			{
				if (camera.Name.StartsWith(defaultName))
				{
					int num = -1;
					int.TryParse(camera.Name.Substring(defaultName.Length), out num);
					if (num > highest)
					{
						highest = num;
					}
				}
			}
			return highest;
		}

		/// <summary>
		/// Get the next camera name for a list of cameras.
		/// </summary>
		/// <param name="cameras">List of cameras to get the next camera name for.</param>
		/// <returns>Next camera name.</returns>
		public static String GetNextCameraName(Cameras cameras)
		{
			Settings settings = new Settings();
			return settings.CameraName + " " + (GetHighestCameraNumber(cameras) + 1);
		}

		/// <summary>
		/// Gets the list of cameras for a network.
		/// </summary>
		/// <param name="network">Network to get cameras for.</param>
		/// <param name="includeHostnames">True to include hostanem cameras, false not to.</param>
		/// <returns>List of network cameras.</returns>
		public static Cameras GetNetworkCameras(string network, bool includeHostnames)
		{
			Settings settings = new Settings();

			// find the cameras for the network
			Cameras networkCameras = new Cameras();
			foreach (Camera camera in settings.Cameras)
			{
				bool isIp = Utils.IsIpAddress(camera.Address);
				if ((isIp && (camera.Network == network)) || (!isIp && includeHostnames))
				{
					networkCameras.Add(camera);
				}
			}

			return networkCameras;
		}

		/// <summary>
		/// Displays a popup error message asynchronously.
		/// </summary>
		/// <param name="message">Error message to be displayed.</param>
		public static async void ErrorAsync(string message)
		{
			Log.Error("ErrorAsync: " + message);
			ContentDialog cd = new ContentDialog()
			{
				Title = Res.Str.Error,
				Content = message,
				CloseButtonText = Res.Str.OK
			};
			await cd.ShowAsync();
		}

		/// <summary>
		/// Displays a popup yes/no prompt asynchronously.
		/// </summary>
		/// <param name="message">Prompt to be displayed.</param>
		/// <param name="yesHandler">Method to be called when Yes is pressed.</param>
		/// <param name="noHandler">Method to be called when No is pressed.</param>
		public static async void YesNoAsync(string message, UICommandInvokedHandler yesHandler, UICommandInvokedHandler noHandler = null)
		{
			Log.Error("YesNoAsync: " + message);
			ContentDialog cd = new ContentDialog()
			{
				Title = Res.Str.AppName,
				Content = message,
				IsPrimaryButtonEnabled = true,
				PrimaryButtonText = Res.Str.Yes,
				CloseButtonText = Res.Str.No
			};
			ContentDialogResult result = await cd.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				yesHandler?.Invoke(new UICommand(Res.Str.Yes));
			}
			else
			{
				noHandler?.Invoke(new UICommand(Res.Str.No));
			}
		}

		/// <summary>
		/// Plays a sound from the Assets\sounds folder.
		/// </summary>
		/// <param name="name">Name of the sound to be played.</param>
		public static async Task PlaySoundAsync(string name)
		{
			try
			{
				// open the file
				StorageFolder sounds = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets\\sounds");
				StorageFile file = await sounds.GetFileAsync(name + ".mp3");
				var stream = await file.OpenAsync(FileAccessMode.Read);

				// play the sound
				soundPlayer.SetSource(stream, file.ContentType);
				soundPlayer.Play();
			}
			catch (Exception ex)
			{
				Log.Error("PlaySoundAsync EXCEPTION: {0}", ex.Message);
			}
		}

		/// <summary>
		/// Creates the Inlines to represent some text embedded with links.
		/// </summary>
		/// <param name="textBlock">TextBlock control to create the lines within.</param>
		/// <param name="text">Text with the embedded links.</param>
		/// <param name="links">Link texts and URLs.</param>
		public static void CreateInlines(TextBlock textBlock, string text, Dictionary<string, string> links)
		{
			// get the indexes of the link texts
			var indexes = new SortedDictionary<int, InlineLink>();
			int index = 0;
			if (links != null)
			{
				foreach (KeyValuePair<string, string> kvp in links)
				{
					string placeholder = "{" + index + "}";
					indexes.Add(text.IndexOf(placeholder), new InlineLink(placeholder, kvp.Key, kvp.Value));
					index++;
				}
			}

			// create the runs, line breaks and hyperlinks
			index = 0;
			textBlock.Inlines.Clear();
			foreach (KeyValuePair<int, InlineLink> kvp in indexes)
			{
				if (kvp.Key != -1)
				{
					if (kvp.Key != 0)
					{
						int len = kvp.Key - index;
						CreateLines(textBlock, text.Substring(index, len));
						index += len;
					}
					Hyperlink link = new Hyperlink() { NavigateUri = new Uri(kvp.Value.Link) };
					link.Inlines.Add(new Run() { Text = kvp.Value.Text });
					textBlock.Inlines.Add(link);
					index += kvp.Value.Placeholder.Length;
				}
			}
			if (index < text.Length)
			{
				CreateLines(textBlock, text.Substring(index));
			}
		}

		/// <summary>
		/// Creates a set of runs and line breaks for a piece of text.
		/// </summary>
		/// <param name="textBlock">TextBlock control to create the lines within.</param>
		/// <param name="text">Text to create the lines from.</param>
		private static void CreateLines(TextBlock textBlock, string text)
		{
			bool first = true;
			string[] lines = text.Split('\n');
			foreach (string line in lines)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					textBlock.Inlines.Add(new LineBreak());
				}
				if (line.Length > 0)
				{
					//textBlock.Inlines.Add(new Run() { Text = line });
					string str = line;
					int i = str.IndexOf("<b>");
					while (i != -1)
					{
						if (i > 0)
						{
							textBlock.Inlines.Add(new Run() { Text = str.Substring(0, i) });
						}
						int j = str.IndexOf("</b>", i + 3);
						textBlock.Inlines.Add(new Run() { Text = str.Substring(i + 3, j - i - 3), FontWeight = FontWeights.Bold });
						str = str.Substring(j + 4);
						i = str.IndexOf("<b>");
					}
					if (str.Length > 0)
					{
						textBlock.Inlines.Add(new Run() { Text = str });
					}
				}
			}
		}

		/// <summary>
		/// An inline link.
		/// </summary>
		private class InlineLink
		{
			public string Placeholder;
			public string Text;
			public string Link;

			public InlineLink(string placeholder, string text, string link)
			{
				Placeholder = placeholder;
				Text = text;
				Link = link;
			}
		}
	}
}
