using System;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;

namespace RPiCameraViewer
{
    public static class Utils
    {
		// public properties
		public static string IpAddressRegexString = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
		public static string HostnameRegexString = "^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\\-]*[A-Za-z0-9])$";
		public static Regex IpAddressRegex = new Regex(IpAddressRegexString);
		public static Regex HostnameRegex = new Regex(HostnameRegexString);

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
			int i = ipAddress.LastIndexOf('.');
			return ipAddress.Substring(0, i + 1);
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
		/// Displays a popup error message asynchronously.
		/// </summary>
		/// <param name="message">Error message to be displayed.</param>
		public static void Error(string message)
		{
			MessageDialog md = new MessageDialog(message, Res.Str.Error);
			md.ShowAsync();
		}

		/// <summary>
		/// Displays a popup yes/no prompt asynchronously.
		/// </summary>
		/// <param name="message">Prompt to be displayed.</param>
		/// <param name="yesHandler">Method to be called when Yes is pressed.</param>
		/// <param name="noHandler">Method to be called when No is pressed.</param>
		public static void YesNo(string message, UICommandInvokedHandler yesHandler, UICommandInvokedHandler noHandler = null)
		{
			MessageDialog md = new MessageDialog(message, Res.Str.AppName);
			md.Commands.Add(new UICommand(Res.Str.Yes, yesHandler ?? YesNoNullHandler));
			md.Commands.Add(new UICommand(Res.Str.No, noHandler ?? YesNoNullHandler));
			md.ShowAsync();
		}

		/// <summary>
		/// Empty button press handler for the YesNo method.
		/// </summary>
		/// <param name="command"></param>
		private static void YesNoNullHandler(IUICommand command)
		{
		}
	}
}
