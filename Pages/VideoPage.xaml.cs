// Copyright © 2019 Shawn Baker using the MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using UWP_Components;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace RPiCameraViewer
{
	/// <summary>
	/// Video page.
	/// </summary>
	public sealed partial class VideoPage : Page
	{
		// local constants
		private const int BUFFER_SIZE = 16384;
		private const int NAL_SIZE_INC = 4096;
		private const int MAX_READ_ERRORS = 300;
		private const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);

		// instance variables
		private Settings settings = new Settings();
		private Camera camera;
		private bool isCancelled;
		private StreamSocket socket;
		private bool decoding = false;
		private ZoomPan zoomPan;
		private Storyboard storyboard;
		private MediaStreamSource streamSource = null;
		private MediaStreamSourceSampleRequest request = null;
		private MediaStreamSourceSampleRequestDeferral deferral = null;
		private Queue<Nal> availableNals = new Queue<Nal>();
		private Queue<Nal> usedNals = new Queue<Nal>();

		/// <summary>
		/// Constructor - Initializes the page.
		/// </summary>
		public VideoPage()
		{
			InitializeComponent();
			Loaded += HandleLoaded;
		}

		/// <summary>
		/// Get the camera parameter.
		/// </summary>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			Log.Info("+VideoPage.OnNavigatedTo");
			base.OnNavigatedTo(e);

			// get the camera
			camera = e.Parameter as Camera;
			Log.Info("-VideoPage.OnNavigatedTo");
		}

		/// <summary>
		/// Initializes the controls.
		/// </summary>
		private void HandleLoaded(object sender, RoutedEventArgs e)
		{
			// set the status message color
			Log.Info("+VideoPage.HandleLoaded: {0}", camera.ToString());
			statusTextBlock.Foreground = new SolidColorBrush(Utils.GoodColor);
			statusTextBlock.Text = Res.Str.InitializingVideo;

			// display the camera name
			nameTextBlock.Text = camera.Name;

			// configure the media element
			media.RealTimePlayback = true;
			media.AreTransportControlsEnabled = false;
			media.AddVideoEffect(typeof(SnapshotVideoEffect).FullName, true, null);
			media.Tapped += HandleMediaTapped;

			// create the NAL buffers
			for (int i = 0; i < 10; i++)
			{
				availableNals.Enqueue(new Nal(NAL_SIZE_INC));
			}

			// create the zoom/pan handler
			zoomPan = new ZoomPan(media);

			// create the fade out animation
			storyboard = new Storyboard();
			DoubleAnimation animation = new DoubleAnimation();
			animation.From = 1.0;
			animation.To = 0.0;
			animation.Duration = new Duration(new TimeSpan(0, 0, 1));
			animation.BeginTime = new TimeSpan(0, 0, 8);
			animation.Completed += HandleAnimationCompleted;
			Storyboard.SetTarget(animation, closeButton);
			Storyboard.SetTargetProperty(animation, "Opacity");
			storyboard.Children.Add(animation);

			// launch the main thread
			Task.Run(ReadSocketAsync);
			Log.Info("-VideoPage.HandleLoaded");
		}

		/// <summary>
		/// Disables the controls after they fade out.
		/// </summary>
		private void HandleAnimationCompleted(object sender, object e)
		{
			closeButton.Opacity = 0;
			closeButton.IsEnabled = false;
			storyboard.Stop();
		}

		/// <summary>
		/// Reenables and shows the controls when the video is tapped.
		/// </summary>
		private void HandleMediaTapped(object sender, TappedRoutedEventArgs e)
		{
			StartFadeout();
		}

		/// <summary>
		/// Shows the controls and starts the fade out animation
		/// </summary>
		private void StartFadeout()
		{
			storyboard.Stop();
			closeButton.Opacity = 1;
			closeButton.IsEnabled = true;
			storyboard.Begin();
		}

		/// <summary>
		/// Displays a status message.
		/// </summary>
		/// <param name="message">Message to be displayed.</param>
		private void DisplayStatusMessage(string message)
		{
			Log.Error(message);
			var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				statusTextBlock.Text = message;
				statusTextBlock.Foreground = new SolidColorBrush(Utils.BadColor);
				statusTextBlock.Visibility = Visibility.Visible;
			});
		}

		/// <summary>
		/// Closes the video page.
		/// </summary>
		private void HandleCloseButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("VideoPage.HandleCloseButtonClick");
			if (isCancelled)
			{
				Frame.GoBack();
			}
			else
			{
				statusTextBlock.Text = Res.Str.ClosingVideo;
				statusTextBlock.Foreground = new SolidColorBrush(Utils.GoodColor);
				statusTextBlock.Visibility = Visibility.Visible;
				isCancelled = true;
			}
		}

		/// <summary>
		/// Taks a snapshot of the stream.
		/// </summary>
		private async void HandleSnapshotButtonClick(object sender, RoutedEventArgs e)
		{
			// create the pictures subfolder
			Log.Info("+VideoPage.HandleSnapshotButtonClick");
			StorageLibrary pictures = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
			StorageFolder folder = await pictures.SaveFolder.CreateFolderAsync("RPiCameraViewer", CreationCollisionOption.OpenIfExists);
			folder = await folder.CreateFolderAsync(camera.Network, CreationCollisionOption.OpenIfExists);
			folder = await folder.CreateFolderAsync(camera.Name, CreationCollisionOption.OpenIfExists);

			// create a unique file name
			int imageNumber = 0;
			string date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string fileName = date + ".jpg";
			while (await folder.TryGetItemAsync(fileName) != null)
			{
				imageNumber++;
				fileName = date + "_" + imageNumber + ".jpg";
			}
			StorageFile file = await folder.CreateFileAsync(fileName);
			Log.Info("VideoPage.HandleSnapshotButtonClick: {0}", fileName);

			// save the snapshot to the file
			using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
			{
				// create the encoder
				BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
				encoder.SetSoftwareBitmap(SnapshotVideoEffect.GetSnapshot());
				encoder.IsThumbnailGenerated = true;

				// save the file with a thumbnail
				try
				{
					await encoder.FlushAsync();
					await Utils.PlaySoundAsync("shutter");
					Log.Info("VideoPage.HandleSnapshotButtonClick: with thumbnail");
				}
				catch (Exception ex)
				{
					Log.Error("VideoPage.HandleSnapshotButtonClick EXCEPTION1: {0}", ex.Message);
					if (ex.HResult == WINCODEC_ERR_UNSUPPORTEDOPERATION)
					{
						encoder.IsThumbnailGenerated = false;
					}
					else
					{
						await Utils.PlaySoundAsync("error");
					}
				}

				// save the file without a thumbnail
				if (!encoder.IsThumbnailGenerated)
				{
					try
					{
						await encoder.FlushAsync();
						await Utils.PlaySoundAsync("shutter");
						Log.Info("VideoPage.HandleSnapshotButtonClick: without thumbnail");
					}
					catch (Exception ex)
					{
						Log.Error("VideoPage.HandleSnapshotButtonClick EXCEPTION2: {0}", ex.Message);
						await Utils.PlaySoundAsync("error");
					}
				}
			}
			StartFadeout();
			Log.Info("-VideoPage.HandleSnapshotButtonClick");
		}

		/// <summary>
		/// Reads bytes form the socket and parses them into NALs.
		/// </summary>
		/// <returns>The asynchronous task.</returns>
		private async Task ReadSocketAsync()
		{
			Log.Info("+VideoPage.ReadSocketAsync");
			Nal nal = availableNals.Dequeue();
			int numZeroes = 0;
			int numReadErrors = 0;
			bool gotHeader = false;
			bool connected = false;

			// look for a TCP/IP connection
			try
			{
				// try to connect to the device
				socket = new StreamSocket();
				HostName hostName = new HostName(camera.Address);
				CancellationTokenSource tokenSource = new CancellationTokenSource();
				tokenSource.CancelAfter(settings.ScanTimeout);
				await socket.ConnectAsync(hostName, camera.Port.ToString()).AsTask(tokenSource.Token);

				// if we get here, we opened the socket
				connected = true;
				DataReader reader = new DataReader(socket.InputStream);
				reader.InputStreamOptions = InputStreamOptions.Partial;

				while (!isCancelled)
				{
					uint len = await reader.LoadAsync(BUFFER_SIZE);
					//Log.Verbose("numBytes = {0}", len);
					if (nal == null)
					{
						lock (availableNals)
						{
							//Log.Verbose("availableNals.Dequeue: {0}", availableNals.Count);
							nal = (availableNals.Count > 0) ? availableNals.Dequeue() : null;
						}
					}

					// process the input buffer
					if (nal == null)
					{
						if (len > 0)
						{
							reader.ReadBuffer(len);
						}
					}
					else if (len > 0)
					{
						numReadErrors = 0;
						for (int i = 0; i < len && nal != null && !isCancelled; i++)
						{
							// resize the NAL if necessary
							if (nal.Stream.Length == nal.Buffer.Capacity)
							{
								nal.Resize(nal.Buffer.Capacity + NAL_SIZE_INC);
							}

							// add the byte to the NAL
							byte b = reader.ReadByte();
							nal.Stream.WriteByte(b);

							// look for a header
							if (b == 0)
							{
								numZeroes++;
							}
							else
							{
								if (b == 1 && numZeroes == 3)
								{
									if (nal.Stream.Length > 4)
									{
										if (gotHeader)
										{
											nal.Buffer.Length = (uint)nal.Stream.Length - 4;
											int nalType = ProcessNal(nal);
											if (isCancelled) break;
											if (nalType != -1)
											{
												lock (availableNals)
												{
													//Log.Verbose("availableNals.Dequeue: {0}", availableNals.Count);
													nal = (availableNals.Count > 0) ? availableNals.Dequeue() : null;
												}
											}
										}
										if (nal != null)
										{
											nal.Stream.Seek(0, SeekOrigin.Begin);
											nal.Stream.SetLength(0);
											nal.Stream.WriteByte(0);
											nal.Stream.WriteByte(0);
											nal.Stream.WriteByte(0);
											nal.Stream.WriteByte(1);
										}
										else
										{
											//Log.Verbose("null");
										}
									}
									gotHeader = true;
								}
								numZeroes = 0;
							}
						}
					}
					else
					{
						numReadErrors++;
						if (numReadErrors >= MAX_READ_ERRORS)
						{
							DisplayStatusMessage(Res.Error.LostConnection);
							break;
						}
					}
				}
				reader.Dispose();
				socket.Dispose();
				decoding = false;
				await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
				{
					media.Stop();
					Frame.GoBack();
				});
			}
			catch (Exception ex)
			{
				DisplayStatusMessage(connected ? Res.Error.LostConnection : Res.Error.CouldntConnect);
				Log.Error("EXCEPTION: {0}", ex.ToString());
				return;
			}
			Log.Info("-VideoPage.ReadSocketAsync");
		}

		/// <summary>
		/// Processes a NAL.
		/// </summary>
		/// <param name="nal">The NAL to be processed.</param>
		/// <returns>Type of NAL.</returns>
		private int ProcessNal(Nal nal)
		{
			// get the NAL type
			int nalType = -1;
			if (nal.Buffer.Length > 4)
			{
				byte[] header = new byte[5];
				nal.Buffer.CopyTo(0, header, 0, 5);
				nalType = (header[0] == 0 && header[1] == 0 && header[2] == 0 && header[3] == 1) ? (header[4] & 0x1F) : -1;
			}
			//Log.Verbose("NAL: type = {0}, len = {1}", nalType, nal.Buffer.Length);

			// process the first SPS record we encounter
			if (nalType == 7 && !decoding)
			{
				byte[] sps = new byte[nal.Buffer.Length];
				nal.Buffer.CopyTo(sps);
				SpsParser parser = new SpsParser(sps, (int)nal.Buffer.Length);
				//Log.Verbose("SPS: {0}x{1} @ {2}", parser.width, parser.height, parser.fps);

				VideoEncodingProperties properties = VideoEncodingProperties.CreateH264();
				properties.ProfileId = H264ProfileIds.High;
				properties.Width = (uint)parser.width;
				properties.Height = (uint)parser.height;

				streamSource = new MediaStreamSource(new VideoStreamDescriptor(properties));
				streamSource.BufferTime = TimeSpan.Zero;
				streamSource.CanSeek = false;
				streamSource.Duration = TimeSpan.Zero;
				streamSource.SampleRequested += HandleSampleRequested;

				var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
				{
					statusTextBlock.Visibility = Visibility.Collapsed;
					media.SetMediaStreamSource(streamSource);
					media.Play();
					storyboard.Begin();
				});
				decoding = true;
			}

			// queue the frame
			if (nalType > 0 && decoding)
			{
				if (deferral != null)
				{
					request.Sample = MediaStreamSample.CreateFromBuffer(nal.Buffer, new TimeSpan(0));
					lock (availableNals)
					{
						//Log.Verbose("availableNals.Enqueue");
						availableNals.Enqueue(nal);
					}
					deferral.Complete();
					deferral = null;
					request = null;
					//Log.Verbose("Deferral Complete");
				}
				else
				{
					//Log.Verbose("usedNals.Enqueue");
					lock (usedNals)
					{
						usedNals.Enqueue(nal);
					}
				}
			}

			// return the NAL type
			return decoding ? nalType : -1;
		}

		/// <summary>
		/// Dequeues a NAL and gives it to the decoder.
		/// </summary>
		private void HandleSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
		{
			//Log.Verbose("HandleSampleRequested");
			Nal nal;
			lock (usedNals)
			{
				//Log.Verbose("usedNals.Dequeue: {0}", usedNals.Count);
				nal = (usedNals.Count > 0) ? usedNals.Dequeue() : null;
			}
			if (nal != null)
			{
				args.Request.Sample = MediaStreamSample.CreateFromBuffer(nal.Buffer, new TimeSpan(0));
				lock (availableNals)
				{
					//Log.Verbose("availableNals.Enqueue");
					availableNals.Enqueue(nal);
				}
			}
			else
			{
				//Log.Verbose("Deferred");
				request = args.Request;
				deferral = args.Request.GetDeferral();
			}
		}
	}
}
