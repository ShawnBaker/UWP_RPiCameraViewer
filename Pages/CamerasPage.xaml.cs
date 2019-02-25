// Copyright © 2019 Shawn Baker using the MIT License.
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using WinRTXamlToolkit.Controls;

namespace RPiCameraViewer
{
	/// <summary>
	/// Cameras page.
	/// </summary>
    public sealed partial class CamerasPage : Page
    {
		// instance variables
		private Settings settings = new Settings();
		private Camera camera;

		/// <summary>
		/// Constructor - Initializes the page.
		/// </summary>
		public CamerasPage()
        {
            InitializeComponent();
			Loaded += HandleLoaded;
		}

		/// <summary>
		/// Initializes the controls.
		/// </summary>
		private void HandleLoaded(object sender, RoutedEventArgs e)
		{
			Log.Info("+CamerasPage.HandleLoaded");
			if (settings.ShowAllNetworks)
			{
				networkTextBlock.Text = Res.Str.AllNetworks;
			}
			else
			{
				string network = Utils.GetNetworkName();
				if (string.IsNullOrEmpty(network))
				{
					networkTextBlock.Text = Res.Str.NoNetwork;
				}
				else
				{
					networkTextBlock.Text = string.Format(Res.Str.NetworkName, network);
				}
			}

			camerasListView.ItemsSource = settings.Cameras;
			Log.Info("-CamerasPage.HandleLoaded");
		}

		/// <summary>
		/// Shows the details for the selected camera.
		/// </summary>
		private void HandleDetailsButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("CamerasPage.HandleLoaded");
			ImageButton button = (ImageButton)sender;
			Camera camera = button.Tag as Camera;
			Frame.Navigate(typeof(CameraPage), camera, new DrillInNavigationTransitionInfo());
		}

		/// <summary>
		/// Prompts the user to delete the selected camera.
		/// </summary>
		private void HandleDeleteButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("CamerasPage.HandleDeleteButtonClick");
			ImageButton button = (ImageButton)sender;
			camera = button.Tag as Camera;
			Utils.YesNoAsync(Res.Str.OkToDeleteCamera, DeleteYesHandler, null);
		}

		/// <summary>
		/// Deletes the selected camera.
		/// </summary>
		/// <param name="command">Command button.</param>
		private void DeleteYesHandler(IUICommand command)
		{
			Log.Info("CamerasPage.DeleteYesHandler: {0}", camera.Name);
			settings.Cameras.Remove(camera);
			settings.Save();
		}

		/// <summary>
		/// Adds a new camera.
		/// </summary>
		private void HandleAddButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("CamerasPage.HandleAddButtonClick");
			Frame.Navigate(typeof(CameraPage), null, new DrillInNavigationTransitionInfo());
		}

		/// <summary>
		/// Prompts the user to delete all cameras.
		/// </summary>
		private void HandleDeleteAllButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("CamerasPage.HandleDeleteAllButtonClick");
			Utils.YesNoAsync(Res.Str.OkToDeleteAllCameras, DeleteAllYesHandler, null);
		}

		/// <summary>
		/// Deletes all the cameras.
		/// </summary>
		/// <param name="command">Command button.</param>
		private void DeleteAllYesHandler(IUICommand command)
		{
			Log.Info("CamerasPage.DeleteAllYesHandler");
			settings.Cameras.Clear();
			settings.Save();
		}

		/// <summary>
		/// Scans the local network for cameras.
		/// </summary>
		private async void HandleScanButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("CamerasPage.HandleScanButtonClick");
			ScannerContentDialog scannerDialog = new ScannerContentDialog(settings);
			await scannerDialog.ShowAsync();
		}

		/// <summary>
		/// Displays the settings page.
		/// </summary>
		private void HandleSettingsButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("CamerasPage.HandleSettingsButtonClick");
			Frame.Navigate(typeof(SettingsPage), null, new DrillInNavigationTransitionInfo());
		}

		/// <summary>
		/// Displays the help page.
		/// </summary>
		private void HandleHelpButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("CamerasPage.HandleHelpButtonClick");
			Frame.Navigate(typeof(HelpPage), null, new DrillInNavigationTransitionInfo());
		}

		/// <summary>
		/// Displays the log files page.
		/// </summary>
		private void HandleLogFilesButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("CamerasPage.HandleLogFilesButtonClick");
			Frame.Navigate(typeof(LogFilesPage), null, new DrillInNavigationTransitionInfo());
		}

		/// <summary>
		/// Displays the about page.
		/// </summary>
		private void HandleAboutButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("CamerasPage.HandleAboutButtonClick");
			Frame.Navigate(typeof(AboutPage), null, new DrillInNavigationTransitionInfo());
		}

		/// <summary>
		/// Displays the video page.
		/// </summary>
		private void HandleItemClick(object sender, ItemClickEventArgs e)
		{
			Camera camera = e.ClickedItem as Camera;
			Log.Info("CamerasPage.HandleItemClick: {0}", camera.Name);
			Frame.Navigate(typeof(VideoPage), camera, new DrillInNavigationTransitionInfo());
		}
	}
}
