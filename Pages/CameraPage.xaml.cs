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
		private Settings settings = new Settings();
		private bool newCamera;
		private Camera camera;

		/// <summary>
		/// Constructor - Initializes the page.
		/// </summary>
		public CameraPage()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Initializes the controls.
		/// </summary>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			// get the camera
			camera = e.Parameter as Camera;
			newCamera = camera == null;
			if (newCamera)
			{
				camera = new Camera(Utils.GetNetworkName(), settings.CameraName + " ?", Utils.GetBaseIpAddress(), Settings.DEFAULT_PORT);
			}

			// set the controls
			titleTextBlock.Text = newCamera ? Res.Str.NewCamera : Res.Str.EditCamera;
			networkTextBlock.Text = camera.Network;
			nameTextBox.Text = camera.Name;
			addressTextBox.Text = camera.Address;
			portTextBox.Text = camera.Port.ToString();
		}

		/// <summary>
		/// Set the focus.
		/// </summary>
		private void HandlePageLoaded(object sender, RoutedEventArgs e)
		{
			if (newCamera)
			{
				nameTextBox.Focus(FocusState.Programmatic);
				nameTextBox.Select(nameTextBox.Text.Length, 0);
			}
		}

		/// <summary>
		/// Save the camera and return to the previous page.
		/// </summary>
		private void HandleSaveButtonClick(object sender, RoutedEventArgs e)
		{
			// get the camera name
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
		}

		/// <summary>
		/// Return to the previous page.
		/// </summary>
		private void HandleCancelButtonClick(object sender, RoutedEventArgs e)
		{
			Frame.GoBack();
		}
	}
}
