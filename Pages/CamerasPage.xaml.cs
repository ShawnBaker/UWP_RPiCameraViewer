// Copyright © 2019 Shawn Baker using the MIT License.
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
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
		}

		/// <summary>
		/// Initializes the controls.
		/// </summary>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

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
		}

		/// <summary>
		/// Shows the details for the selected camera.
		/// </summary>
		private void HandleDetailsButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			ImageButton button = (ImageButton)sender;
			Camera camera = button.Tag as Camera;
			Frame.Navigate(typeof(CameraPage), camera, new DrillInNavigationTransitionInfo());
		}

		/// <summary>
		/// Prompts the user to delete the selected camera.
		/// </summary>
		private void HandleDeleteButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			ImageButton button = (ImageButton)sender;
			camera = button.Tag as Camera;
			Utils.YesNo(Res.Str.OkToDeleteCamera, DeleteYesHandler, null);
		}

		/// <summary>
		/// Deletes the selected camera.
		/// </summary>
		/// <param name="command">Command button.</param>
		private void DeleteYesHandler(IUICommand command)
		{
			settings.Cameras.Remove(camera);
			settings.Save();
		}

		/// <summary>
		/// Adds a new camera.
		/// </summary>
		private void HandleAddButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			Frame.Navigate(typeof(CameraPage), null, new DrillInNavigationTransitionInfo());
		}

		/// <summary>
		/// Prompts the user to delete all cameras.
		/// </summary>
		private void HandleDeleteAllButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			Utils.YesNo(Res.Str.OkToDeleteAllCameras, DeleteAllYesHandler, null);
		}

		/// <summary>
		/// Deletes all the cameras.
		/// </summary>
		/// <param name="command">Command button.</param>
		private void DeleteAllYesHandler(IUICommand command)
		{
			settings.Cameras.Clear();
			settings.Save();
		}

		/// <summary>
		/// Scans the local network for cameras.
		/// </summary>
		private void HandleScanButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
		}

		/// <summary>
		/// Displays the about page.
		/// </summary>
		private void HandleAboutButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			Frame.Navigate(typeof(AboutPage), null, new DrillInNavigationTransitionInfo());
		}

		/// <summary>
		/// Displays the help page.
		/// </summary>
		private void HandleHelpButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			Frame.Navigate(typeof(HelpPage), null, new DrillInNavigationTransitionInfo());
		}

		/// <summary>
		/// Displays the settings page.
		/// </summary>
		private void HandleSettingsButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			Frame.Navigate(typeof(SettingsPage), null, new DrillInNavigationTransitionInfo());
		}
	}
}
