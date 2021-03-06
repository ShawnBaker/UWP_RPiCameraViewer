﻿// Copyright © 2019 Shawn Baker using the MIT License.
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RPiCameraViewer
{
    /// <summary>
    /// Settings page.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
		// instance variables
		private Settings settings = new Settings();

		/// <summary>
		/// Constructor - Initializes the page.
		/// </summary>
		public SettingsPage()
        {
            InitializeComponent();
			Loaded += HandleLoaded;
		}

		/// <summary>
		/// Initializes the controls.
		/// </summary>
		private void HandleLoaded(object sender, RoutedEventArgs e)
		{
			Log.Info("+SettingsPage.HandleLoaded");
			nameTextBox.Text = settings.CameraName;
			timeoutTextBox.Text = settings.ScanTimeout.ToString();
			portTextBox.Text = settings.Port.ToString();
			showAllNetworksToggle.IsOn = settings.ShowAllNetworks;
			Log.Info("-SettingsPage.HandleLoaded");
		}

		/// <summary>
		/// Validates and saves the settings.
		/// </summary>
		private void HandleSaveButtonClick(object sender, RoutedEventArgs e)
		{
			// get the camera name
			Log.Info("+SettingsPage.HandleSaveButtonClick");
			string name = nameTextBox.Text.Trim();
			if (name.Length == 0)
			{
				Utils.ErrorAsync(Res.Error.NoCameraName);
				return;
			}

			// get the scan timeout
			int timeout = -1;
			string timeoutString = timeoutTextBox.Text.Trim();
			if (timeoutString.Length > 0)
			{
				if (!int.TryParse(timeoutString, out timeout))
				{
					timeout = -1;
				}
			}
			if (timeout < Settings.MIN_TIMEOUT || timeout > Settings.MAX_TIMEOUT)
			{
				Utils.ErrorAsync(string.Format(Res.Error.BadScanTimeout, Settings.MIN_TIMEOUT, Settings.MAX_TIMEOUT));
				return;
			}

			// get the port
			int port = -1;
			string portString = portTextBox.Text.Trim();
			if (portString.Length > 0)
			{
				if (!int.TryParse(portString, out port))
				{
					port = -1;
				}
			}
			if (port < Settings.MIN_PORT || port > Settings.MAX_PORT)
			{
				Utils.ErrorAsync(string.Format(Res.Error.BadPort, Settings.MIN_PORT, Settings.MAX_PORT));
				return;
			}

			// save the settings
			Log.Info("SettingsPage.HandleSaveButtonClick: {0} {1} {2} {3}", name, timeout, port, showAllNetworksToggle.IsOn);
			settings.CameraName = name;
			settings.ScanTimeout = timeout;
			settings.Port = port;
			settings.ShowAllNetworks = showAllNetworksToggle.IsOn;
			settings.Save();

			// return to the previous page
			Frame.GoBack();
			Log.Info("-SettingsPage.HandleSaveButtonClick");
		}

		/// <summary>
		/// Returns to the previous page.
		/// </summary>
		private void HandleCancelButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("HelpPage.HandleCancelButtonClick");
			Frame.GoBack();
		}
	}
}
