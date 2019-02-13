// Copyright © 2019 Shawn Baker using the MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

		// instance variables
		private Settings settings = new Settings();
		private Camera camera;
		private bool isCancelled;
		private StreamSocket socket;
		private bool decoding = false;
		private MediaStreamSource streamSource = null;
		private MediaStreamSourceSampleRequest request = null;
		private MediaStreamSourceSampleRequestDeferral deferral = null;
		private Queue<Nal> availableNals = new Queue<Nal>();
		private Queue<Nal> usedNals = new Queue<Nal>();

		public VideoPage()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			camera = e.Parameter as Camera;
			nameTextBlock.Text = camera.Name;

			media.RealTimePlayback = true;
			//media.IsFullWindow = true;
			media.AreTransportControlsEnabled = false;

			// create the NAL buffers
			for (int i = 0; i < 10; i++)
			{
				availableNals.Enqueue(new Nal(NAL_SIZE_INC));
			}

			// launch the main thread
			Task.Run(ReadSocketAsync);
		}

		private void HandleCloseButtonClick(object sender, RoutedEventArgs e)
		{
			//ImageButton button = (ImageButton)sender;
			if (isCancelled)
			{
				Frame.GoBack();
			}
			else
			{
				isCancelled = true;
			}
		}

		private void HandleSnapshotButtonClick(object sender, RoutedEventArgs e)
		{
			//ImageButton button = (ImageButton)sender;
		}

		private async Task ReadSocketAsync()
		{
			Nal nal = availableNals.Dequeue();
			int numZeroes = 0;
			int numReadErrors = 0;

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
				DataReader reader = new DataReader(socket.InputStream);
				reader.InputStreamOptions = InputStreamOptions.Partial;
				//DataWriter writer = new DataWriter(nal.AsStream());

				while (!isCancelled)
				{
					uint len = await reader.LoadAsync(BUFFER_SIZE);
					//Debug.WriteLine("numBytes = {0}", len);

					if (nal == null)
					{
						lock (availableNals)
						{
							//Debug.WriteLine("availableNals.Dequeue: {0}", availableNals.Count);
							nal = (availableNals.Count > 0) ? availableNals.Dequeue() : null;
						}
					}
					if (nal == null)
					{
						if (len > 0)
						{
							reader.ReadBuffer(len);
						}
					}

					// process the input buffer
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
										nal.Buffer.Length = (uint)nal.Stream.Length - 4;
										int nalType = ProcessNal(nal);
										if (isCancelled) break;
										if (nalType != -1)
										{
											lock (availableNals)
											{
												//Debug.WriteLine("availableNals.Dequeue: {0}", availableNals.Count);
												nal = (availableNals.Count > 0) ? availableNals.Dequeue() : null;
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
											//Debug.WriteLine("null");
										}
									}
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
							//setMessage(R.string.error_lost_connection);
							break;
						}
					}
				}
				reader.Dispose();
				socket.Dispose();
				SetDecodingState(false);
				await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
				{
					Frame.GoBack();
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine("EXCEPTION: {0}", ex.ToString());
				return;
			}
		}

		/// <summary>
		/// Sets the decoding state.
		/// </summary>
		/// <param name="newDecoding">New decoding state.</param>
		private void SetDecodingState(bool newDecoding)
		{
			try
			{
				if (newDecoding != decoding && streamSource != null)
				{
					Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
					{
						if (newDecoding)
						{
							media.Play();
						}
						else
						{
							media.Stop();
						}
					});
					decoding = newDecoding;
				}
				decoding = newDecoding;
			}
			catch { }
		}

		/// <summary>
		/// Processes a NAL.
		/// </summary>
		/// <param name="nal">Array of bytes containing the NAL.</param>
		/// <param name="nalLen">Number of bytes in the NAL.</param>
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
			//Debug.WriteLine(String.Format("NAL: type = {0}, len = {1}", nalType, nal.Length));

			// process the first SPS record we encounter
			if (nalType == 7 && !decoding)
			{
				byte[] sps = new byte[nal.Buffer.Length];
				nal.Buffer.CopyTo(sps);
				SpsParser parser = new SpsParser(sps, (int)nal.Buffer.Length);
				VideoEncodingProperties properties = VideoEncodingProperties.CreateH264();
				properties.ProfileId = H264ProfileIds.High;
				properties.Width = (uint)parser.width;
				properties.Height = (uint)parser.height;
				//properties.Bitrate = (uint)parser.bitrate;
				//properties.Bitrate = 1000000;
				//properties.FrameRate = parser.fps;
				//Debug.WriteLine(String.Format("SPS: {0}x{1} @ {2}", parser.width, parser.height, parser.fps));

				streamSource = new MediaStreamSource(new VideoStreamDescriptor(properties));
				streamSource.BufferTime = TimeSpan.Zero;
				streamSource.CanSeek = false;
				streamSource.Duration = TimeSpan.Zero;
				streamSource.SampleRequested += HandleSampleRequested;
				streamSource.SampleRendered += HandleSampleRendered;

				Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
				{
					media.SetMediaStreamSource(streamSource);
				});

				SetDecodingState(true);
			}

			// queue the frame
			if (nalType > 0 && decoding)
			{
				//Debug.WriteLine("usedNals.Enqueue");
				if (deferral != null)
				{
					//request.Sample = MediaStreamSample.CreateFromBuffer(nal, new TimeSpan(0, 0, 0, 0, 66));
					request.Sample = MediaStreamSample.CreateFromBuffer(nal.Buffer, new TimeSpan(0, 0, 0, 0, 66));
					lock (availableNals)
					{
						//Debug.WriteLine("availableNals.Enqueue");
						availableNals.Enqueue(nal);
					}
					deferral.Complete();
					deferral = null;
					request = null;
					//Debug.WriteLine("Deferral Complete");
				}
				else
				{
					lock (usedNals)
					{
						usedNals.Enqueue(nal);
					}
				}
			}
			return decoding ? nalType : -1;
		}

		private void HandleSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
		{
			//Debug.WriteLine("HandleSampleRequested");
			Nal nal;
			lock (usedNals)
			{
				//Debug.WriteLine("usedNals.Dequeue: {0}", usedNals.Count);
				nal = (usedNals.Count > 0) ? usedNals.Dequeue() : null;
			}
			if (nal != null)
			{
				//args.Request.Sample = MediaStreamSample.CreateFromBuffer(nal, new TimeSpan(0, 0, 0, 0, 66));
				args.Request.Sample = MediaStreamSample.CreateFromBuffer(nal.Buffer, new TimeSpan(0, 0, 0, 0, 66));
				lock (availableNals)
				{
					//Debug.WriteLine("availableNals.Enqueue");
					availableNals.Enqueue(nal);
				}
			}
			else
			{
				//Debug.WriteLine("Deferred");
				request = args.Request;
				deferral = args.Request.GetDeferral();
			}
		}

		private void HandleSampleRendered(MediaStreamSource sender, MediaStreamSourceSampleRenderedEventArgs args)
		{
			//Debug.WriteLine("HandleSampleRendered");
		}
	}
}
