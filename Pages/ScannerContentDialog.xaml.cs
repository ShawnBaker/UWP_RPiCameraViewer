// Copyright © 2019 Shawn Baker using the MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace RPiCameraViewer
{
	public sealed partial class ScannerContentDialog : ContentDialog
	{
		// local constants
		private const int NO_DEVICE = -1;
		private const int LAST_DEVICE = 254;
		private const int NUM_THREADS = 40;
		private const int SLEEP_TIMEOUT = 10;
		private const int DISMISS_TIMEOUT = 1500;

		// instance variables
		private Settings settings;
		private bool isCancelled;
		private string network, baseAddress;
		private int device, numDone, thisDevice;
		private Cameras cameras, newCameras;
		private object deviceObj = new object();
		private object doneObj = new object();

		/// <summary>
		/// Constructor - Initializes the controls and launchs the main thread.
		/// </summary>
		public ScannerContentDialog(Settings settings)
		{
			InitializeComponent();
			this.settings = settings;
			Loaded += HandleLoaded;
		}

		/// <summary>
		/// Initializes the controls.
		/// </summary>
		private void HandleLoaded(object sender, RoutedEventArgs e)
		{
			// initialize the controls
			Log.Info("+ScannerContentDialog.HandleLoaded");
			portTextBlock.Text = string.Format(Res.Str.ScanningOnPort, settings.Port);
			statusTextBlock.Text = string.Format(Res.Str.NewCamerasFound, 0);
			progressBar.Value = 0;
			cancelButton.Background = new SolidColorBrush(Utils.PrimaryColor);
			cancelButton.Foreground = new SolidColorBrush(Utils.TextColor);
			cancelButton.Content = Res.Str.Cancel;

			// launch the main thread
			Task.Run(ScanAsync);
			Log.Info("-ScannerContentDialog.HandleLoaded");
		}

		/// <summary>
		/// Cancel the scan or close the dialog.
		/// </summary>
		private void HandleCancelButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("ScannerContentDialog.HandleCancelButtonClick");
			if (isCancelled || numDone >= LAST_DEVICE)
			{
				Hide();
			}
			else
			{
				isCancelled = true;
			}
		}

		/// <summary>
		/// Main scanning thread.
		/// </summary>
		private async Task ScanAsync()
		{
			// get the network info
			Log.Info("+ScannerContentDialog.ScanAsync");
			network = Utils.GetNetworkName();
			baseAddress = Utils.GetBaseIpAddress();
			string ipAddress = Utils.GetLocalIpAddress();
			thisDevice = 0;
			if (!string.IsNullOrEmpty(ipAddress))
			{
				int i = ipAddress.LastIndexOf('.');
				if (i != -1)
				{
					int.TryParse(ipAddress.Substring(i + 1), out thisDevice);
				}
			}
			Log.Info("ScannerContentDialog.ScanAsync: {0} {1} {2} {3}", network, baseAddress, thisDevice, settings.ToString());

			// initialize the scan state
			device = 0;
			numDone = 0;
			cameras = Utils.GetNetworkCameras(network, false);
			newCameras = new Cameras();
			isCancelled = false;

			// create and start the threads
			for (int t = 0; t < NUM_THREADS; t++)
			{
				Log.Info("ScannerContentDialog.ScanAsync: starting thread {0}", t);
				Task task = Task.Run(CheckDeviceConnectionsAsync);
			}

			// wait for the threads to finish
			while (!isCancelled && numDone < LAST_DEVICE)
			{
				await Task.Delay(SLEEP_TIMEOUT);
			}

			// add the new cameras
			SetStatus(true);
			if (!isCancelled && newCameras.Count > 0)
			{
				AddCamerasAsync();
				await Task.Delay(DISMISS_TIMEOUT);
			}

			// hide the dialog if we were cancelled or we found new cameras
			if (isCancelled || newCameras.Count > 0)
			{
				await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
				{
					Hide();
				});
			}
			Log.Info("-ScannerContentDialog.ScanAsync");
		}

		/// <summary>
		/// Thread that checks for connections to devices.
		/// </summary>
		private async Task CheckDeviceConnectionsAsync()
		{
			for (int dev = GetNextDevice(); !isCancelled && dev != NO_DEVICE; dev = GetNextDevice())
			{
				// skip this device
				if (dev != thisDevice)
				{
					string address = baseAddress + dev;

					// look for a TCP/IP connection
					try
					{
						// try to connect to the device
						Log.Info("ScannerContentDialog.CheckDeviceConnectionsAsync: connecting to {0}", address);
						StreamSocket socket = new StreamSocket();
						HostName hostName = new HostName(address);
						CancellationTokenSource tokenSource = new CancellationTokenSource();
						tokenSource.CancelAfter(settings.ScanTimeout);
						await socket.ConnectAsync(hostName, settings.Port.ToString()).AsTask(tokenSource.Token);

						// if we get here, we found a new camera
						Log.Info("ScannerContentDialog.CheckDeviceConnectionsAsync: adding {0}", address);
						Camera camera = new Camera(network, "", address, settings.Port);
						AddCamera(camera);
					}
					catch (Exception ex)
					{
						Log.Error("ScannerContentDialog.CheckDeviceConnectionsAsync: {0}", ex.Message);
					}
				}

				// update the status
				DoneDevice();
			}
		}

		/// <summary>
		/// Adds the new cameras to the list of cameras.
		/// </summary>
		private async void AddCamerasAsync()
		{
			// sort the new cameras by IP address
			Log.Info("+ScannerContentDialog.AddCamerasAsync");
			newCameras.SortByAddress();

			// get the highest number from the existing camera names
			int highest = Utils.GetHighestCameraNumber(cameras);

			// set the camera names and add the new cameras to the list of all cameras
			await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				foreach (Camera camera in newCameras)
				{
					camera.Name = settings.CameraName + " " + ++highest;
					settings.Cameras.Add(camera);
					Log.Info("ScannerContentDialog.AddCamerasAsync: {0}", camera.ToString());
				}
				settings.Save();
			});
			Log.Info("-ScannerContentDialog.AddCamerasAsync");
		}

		/// <summary>
		/// Adds a camera to the list of network cameras.
		/// </summary>
		/// <param name="newCamera">Camera to be added,</param>
		private void AddCamera(Camera newCamera)
		{
			lock (newCameras)
			{
				bool found = false;
				foreach (Camera camera in cameras)
				{
					if (newCamera.Address == camera.Address && newCamera.Port == camera.Port)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					Log.Info("ScannerContentDialog.AddCamera: " + newCamera.ToString());
					newCameras.Add(newCamera);
				}
			}
		}

		/// <summary>
		/// Gets the next device number.
		/// </summary>
		/// <returns>The next device number.</returns>
		private int GetNextDevice()
		{
			int dev = NO_DEVICE;
			lock (deviceObj)
			{
				if (device < LAST_DEVICE)
				{
					device++;
					dev = device;
				}
			}
			return dev;
		}

		/// <summary>
		/// Updates the status in a thread safe way.
		/// </summary>
		private void DoneDevice()
		{
			lock (doneObj)
			{
				numDone++;
				SetStatus(false);
			}
		}

		/// <summary>
		/// Updates the status on the UI thread.
		/// </summary>
		/// <param name="last">True if we're done scanning, false if not.</param>
		private async void SetStatus(bool last)
		{
			await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				progressBar.Value = numDone;
				statusTextBlock.Text = string.Format(Res.Str.NewCamerasFound, newCameras.Count);
				if (newCameras.Count > 0)
				{
					statusTextBlock.Foreground = new SolidColorBrush(Utils.GoodColor);
				}
				else if (last)
				{
					statusTextBlock.Foreground = new SolidColorBrush(Utils.BadColor);
				}
				if (last)
				{
					cancelButton.Content = Res.Str.Done;
				}
			});
		}
	}
}
