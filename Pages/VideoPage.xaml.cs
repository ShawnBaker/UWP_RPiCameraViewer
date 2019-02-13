#define ENABLE_DECODING
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
#if ENABLE_DECODING
		private MediaStreamSource streamSource = null;
		private MediaStreamSourceSampleRequest request = null;
		private MediaStreamSourceSampleRequestDeferral deferral = null;
		private Queue<Nal> availableNals = new Queue<Nal>();
		private Queue<Nal> usedNals = new Queue<Nal>();
#endif
		public VideoPage()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			// get the camera, display its name
			camera = e.Parameter as Camera;
			nameTextBlock.Text = camera.Name;
#if ENABLE_DECODING
			// configure the media element
			media.RealTimePlayback = true;
			//media.IsFullWindow = true;
			media.AreTransportControlsEnabled = false;

			// create the NAL buffers
			for (int i = 0; i < 100; i++)
			{
				availableNals.Enqueue(new Nal(NAL_SIZE_INC));
			}
#endif
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
#if ENABLE_DECODING
			Nal nal = availableNals.Dequeue();
#else
			Nal nal = new Nal(NAL_SIZE_INC);
#endif
			int numZeroes = 0;
			int numReadErrors = 0;
			bool gotHeader = false;

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

				while (!isCancelled)
				{
					uint len = await reader.LoadAsync(BUFFER_SIZE);
					//Debug.WriteLine("numBytes = {0}", len);
#if ENABLE_DECODING
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
					else
#endif
					// process the input buffer
					if (len > 0)
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
#if ENABLE_DECODING
											if (nalType != -1)
											{
												lock (availableNals)
												{
													//Debug.WriteLine("availableNals.Dequeue: {0}", availableNals.Count);
													nal = (availableNals.Count > 0) ? availableNals.Dequeue() : null;
												}
											}
#endif
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
							//setMessage(R.string.error_lost_connection);
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
				Debug.WriteLine("EXCEPTION: {0}", ex.ToString());
				return;
			}
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
			//Debug.WriteLine(String.Format("NAL: type = {0}, len = {1}", nalType, nal.Buffer.Length));

			// process the first SPS record we encounter
			if (nalType == 7 && !decoding)
			{
				byte[] sps = new byte[nal.Buffer.Length];
				nal.Buffer.CopyTo(sps);
				SpsParser parser = new SpsParser(sps, (int)nal.Buffer.Length);
				//Debug.WriteLine(String.Format("SPS: {0}x{1} @ {2}", parser.width, parser.height, parser.fps));
#if ENABLE_DECODING
				VideoEncodingProperties properties = VideoEncodingProperties.CreateH264();
				properties.ProfileId = H264ProfileIds.High;
				properties.Width = (uint)parser.width;
				properties.Height = (uint)parser.height;
				//properties.Bitrate = (uint)parser.bitrate;
				//properties.Bitrate = 1000000;
				//properties.FrameRate = parser.fps;

				streamSource = new MediaStreamSource(new VideoStreamDescriptor(properties));
				streamSource.BufferTime = TimeSpan.Zero;
				streamSource.CanSeek = false;
				streamSource.Duration = TimeSpan.Zero;
				streamSource.SampleRequested += HandleSampleRequested;
				streamSource.SampleRendered += HandleSampleRendered;

				Windows.Foundation.IAsyncAction action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
				{
					media.SetMediaStreamSource(streamSource);
					media.Play();
				});
#endif
				decoding = true;
			}
#if ENABLE_DECODING
			// queue the frame
			if (nalType > 0 && decoding)
			{
				if (deferral != null)
				{
					request.Sample = MediaStreamSample.CreateFromBuffer(nal.Buffer, new TimeSpan(0));
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
					//Debug.WriteLine("usedNals.Enqueue");
					lock (usedNals)
					{
						usedNals.Enqueue(nal);
					}
				}
			}
#endif
			return decoding ? nalType : -1;
		}
#if ENABLE_DECODING
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
				args.Request.Sample = MediaStreamSample.CreateFromBuffer(nal.Buffer, new TimeSpan(0));
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
#endif
	}
}
