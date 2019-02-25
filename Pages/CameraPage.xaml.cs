// Copyright © 2019 Shawn Baker using the MIT License.
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace RPiCameraViewer
{
	/// <summary>
	/// Camera page.
	/// </summary>
	public sealed partial class CameraPage : Page
	{
		// instance variables
		private Settings settings = new Settings();
		private bool newCamera;
		private Camera camera;

		/// <summary>
		/// Constructor - Initializes the page.
		/// </summary>
		public CameraPage()
		{
			InitializeComponent();
			Loaded += HandleLoaded;
		}

		/// <summary>
		/// Gets the camera parameter.
		/// </summary>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			Log.Info("+CameraPage.OnNavigatedTo");
			base.OnNavigatedTo(e);

			// get the camera
			camera = e.Parameter as Camera;
			newCamera = camera == null;
			if (newCamera)
			{
				camera = new Camera(Utils.GetNetworkName(), settings.CameraName + " ?", Utils.GetBaseIpAddress(), Settings.DEFAULT_PORT);
			}
			Log.Info("-CameraPage.OnNavigatedTo");
		}

		/// <summary>
		/// Initializes the controls.
		/// </summary>
		private void HandleLoaded(object sender, RoutedEventArgs e)
		{
			// set the control values
			Log.Info("+CameraPage.HandleLoaded");
			titleTextBlock.Text = newCamera ? Res.Str.NewCamera : Res.Str.EditCamera;
			networkTextBlock.Text = camera.Network;
			nameTextBox.Text = camera.Name;
			addressTextBox.Text = camera.Address;
			portTextBox.Text = camera.Port.ToString();

			// set the focus
			nameTextBox.Focus(FocusState.Programmatic);
			if (newCamera)
			{
				nameTextBox.Select(nameTextBox.Text.Length, 0);
			}
			Log.Info("-CameraPage.HandleLoaded");
		}

		/// <summary>
		/// Save the camera and return to the previous page.
		/// </summary>
		private void HandleSaveButtonClick(object sender, RoutedEventArgs e)
		{
			// get the camera name
			Log.Info("+CameraPage.HandleSaveButtonClick");
			string name = nameTextBox.Text.Trim();
			if (name.Length == 0)
			{
				Utils.ErrorAsync(Res.Error.NoCameraName);
				return;
			}

			// get the cameras
			Camera existingCamera = settings.Cameras.Find(camera.Network, name);

			// make sure the name doesn't already exist
			if ((newCamera || name != camera.Name) && (existingCamera != null))
			{
				Utils.ErrorAsync(Res.Error.NameAlreadyExists);
				return;
			}

			// make sure there's an address
			string address = addressTextBox.Text.Trim();
			if (string.IsNullOrEmpty(address))
			{
				Utils.ErrorAsync(Res.Error.NoAddress);
				return;
			}

			// check the address
			if (!Utils.IsIpAddress(address) && !Utils.IsHostname(address))
			{
				Utils.ErrorAsync(Res.Error.BadAddress);
				return;
			}

			// get and check the port number
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

			// update the cameras
			Log.Info("CameraPage.HandleSaveButtonClick: {0} {1} {2}", name, address, port);
			if (newCamera)
			{
				camera.Name = name;
				camera.Address = address;
				camera.Port = port;
				settings.Cameras.Add(camera);
			}
			else
			{
				existingCamera = settings.Cameras.Find(camera.Network, camera.Name);
				existingCamera.Name = name;
				existingCamera.Address = address;
				existingCamera.Port = port;
			}
			settings.Save();

			// return to the previous page
			Frame.GoBack();
			Log.Info("-CameraPage.HandleSaveButtonClick");
		}

		/// <summary>
		/// Return to the previous page.
		/// </summary>
		private void HandleCancelButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("CameraPage.HandleCancelButtonClick");
			Frame.GoBack();
		}
	}
}
