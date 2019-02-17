// Copyright © 2019 Shawn Baker using the MIT License.
using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace RPiCameraViewer
{
	public class ZoomPan
	{
		// instance variables
		private Border border;
		private MediaElement mediaElement;
		private ScaleTransform scaleTransform;
		private TranslateTransform translateTransform;
		private bool isEnabled = false;
		private bool isCaptured = false;
		private Size videoSize = Size.Empty;
		private Size fitSize = Size.Empty;
		private Point pan = new Point(0, 0);
		private Point panStart, panOrigin;
		private double zoom = 1;
		private double minZoom = 1;
		private double maxZoom = 10;

		/// <summary>
		/// Constructor - Configures the media element.
		/// </summary>
		/// <param name="mediaElement">Media element to add zoom/pan to.</param>
		public ZoomPan(Border border, MediaElement mediaElement)
		{
			// save the parameters
			this.mediaElement = mediaElement;

			// create the transforms
			mediaElement.RenderTransformOrigin = new Point(0.5, 0.5);
			TransformGroup transformGroup = new TransformGroup();
			translateTransform = new TranslateTransform();
			transformGroup.Children.Add(translateTransform);
			scaleTransform = new ScaleTransform();
			transformGroup.Children.Add(scaleTransform);
			mediaElement.RenderTransform = transformGroup;

			// attach the event handlers
			mediaElement.SizeChanged += HandleMediaSizeChanged;
			mediaElement.CurrentStateChanged += HandleMediaCurrentStateChanged;
			mediaElement.PointerWheelChanged += HandleMediaPointerWheelChanged;
			mediaElement.PointerPressed += HandleMediaPointerPressed;
			mediaElement.PointerReleased += HandleMediaPointerReleased;
			mediaElement.PointerMoved += HandleMediaPointerMoved;
			mediaElement.DoubleTapped += MediaElement_DoubleTapped;
			mediaElement.IsDoubleTapEnabled = true;
		}

		/// <summary>
		/// Sets the video size when playback starts.
		/// </summary>
		private void HandleMediaCurrentStateChanged(object sender, RoutedEventArgs e)
		{
			if (mediaElement.CurrentState == MediaElementState.Playing)
			{
				SetVideoSize(new Size(mediaElement.NaturalVideoWidth, mediaElement.NaturalVideoHeight));
			}
		}

		/// <summary>
		/// Resets the zoom/pan when the control size changes.
		/// </summary>
		private void HandleMediaSizeChanged(object sender, SizeChangedEventArgs e)
		{
			Reset();
		}

		/// <summary>
		/// Zooms in/out using the mouse wheel.
		/// </summary>
		private void HandleMediaPointerWheelChanged(object sender, PointerRoutedEventArgs e)
		{
			if (isEnabled)
			{
				PointerPoint point = e.GetCurrentPoint(mediaElement);
				int delta = point.Properties.MouseWheelDelta;
				double newZoom = zoom + (delta > 0 ? 0.2 : -0.2);
				newZoom = Math.Max(minZoom, Math.Min(newZoom, maxZoom));
				if (newZoom != zoom)
				{
					double diff = (newZoom / zoom) - 1;
					Point offset = new Point(point.Position.X - mediaElement.ActualWidth / 2 + pan.X,
											 point.Position.Y - mediaElement.ActualHeight / 2 + pan.Y);
					//Debug.WriteLine("Wheel: {0} {1} {2} {3} {4}", zoom, newZoom, diff, offset, pan);
					SetZoomPan(newZoom, pan.X - offset.X * diff, pan.Y - offset.Y * diff);
				}
			}
		}

		/// <summary>
		/// Starts panning.
		/// </summary>
		private void HandleMediaPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			if (isEnabled)
			{
				PointerPoint point = e.GetCurrentPoint(border);
				if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse || point.Properties.IsLeftButtonPressed)
				{
					panStart = pan;
					panOrigin = point.Position;
					isCaptured = mediaElement.CapturePointer(e.Pointer);
					e.Handled = true;
					//Debug.WriteLine("Pressed: {0} {1} {2} {3} {4}", panStart, panOrigin, isCaptured, zoom, pan);
				}
			}
		}

		/// <summary>
		/// Ends panning.
		/// </summary>
		private void HandleMediaPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			if (isCaptured)
			{
				mediaElement.ReleasePointerCapture(e.Pointer);
				isCaptured = false;
				e.Handled = true;
			}
		}

		/// <summary>
		/// Pans the view.
		/// </summary>
		private void HandleMediaPointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (isCaptured)
			{
				PointerPoint point = e.GetCurrentPoint(border);
				Point distance = new Point((point.Position.X - panOrigin.X) / zoom, (point.Position.Y - panOrigin.Y) / zoom);
				SetPan(panStart.X + distance.X, panStart.Y + distance.Y);
				//Debug.WriteLine("Moved: {0} {1} {2} {3}", distance, point.Position, zoom, pan);
				e.Handled = true;
			}
		}

		/// <summary>
		/// Sets the zoom back to 1.
		/// </summary>
		private void MediaElement_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			SetZoomPan(1, 0, 0);
		}

		/// <summary>
		/// Resets the zoom/pan based on the video and control size.
		/// </summary>
		public void Reset()
		{
			// get the fitted view size
			Size viewSize = new Size(mediaElement.ActualWidth, mediaElement.ActualHeight);
			double viewAspect = viewSize.Height / viewSize.Width;
			double videoAspect = videoSize.Height / videoSize.Width;
			if (videoAspect < viewAspect)
			{
				fitSize.Width = viewSize.Width;
				fitSize.Height = videoSize.Height * viewSize.Width / videoSize.Width;
			}
			else
			{
				fitSize.Width = videoSize.Width * viewSize.Height / videoSize.Height;
				fitSize.Height = viewSize.Height;
			}

			// initialize the zoom and pan
			SetZoomPan(zoom, pan);
		}

		/// <summary>
		/// Sets the video size and enable zoom/pan if non-zero.
		/// </summary>
		/// <param name="newSize">The new video size.</param>
		public void SetVideoSize(Size newSize)
		{
			if (newSize != videoSize)
			{
				videoSize = newSize;
				isEnabled = videoSize.Width != 0 && videoSize.Height != 0;
				Reset();
			}
		}

		/// <summary>
		/// Sets the zoom.
		/// </summary>
		/// <param name="newZoom">New zoom level.</param>
		public void SetZoom(double newZoom)
		{
			zoom = newZoom;
			CheckPan();
			SetTransform();
		}

		/// <summary>
		/// Sets the pan.
		/// </summary>
		/// <param name="newPan">New pan value.</param>
		public void SetPan(Point newPan)
		{
			pan = newPan;
			CheckPan();
			SetTransform();
		}

		/// <summary>
		/// Sets the pan.
		/// </summary>
		/// <param name="newPanX">New X value.</param>
		/// <param name="newPanY">New Y value.</param>
		public void SetPan(double newPanX, double newPanY)
		{
			SetPan(new Point(newPanX, newPanY));
		}

		/// <summary>
		/// Sets the zoom and pan.
		/// </summary>
		/// <param name="newZoom">New zoom level.</param>
		/// <param name="newPan">New pan value.</param>
		public void SetZoomPan(double newZoom, Point newPan)
		{
			zoom = newZoom;
			pan = newPan;
			CheckPan();
			SetTransform();
		}

		/// <summary>
		/// Sets the zoom and pan.
		/// </summary>
		/// <param name="newZoom">New zoom level.</param>
		/// <param name="newPanX">New X pan value.</param>
		/// <param name="newPanY">New Y pan value.</param>
		public void SetZoomPan(double newZoom, double newPanX, double newPanY)
		{
			SetZoomPan(newZoom, new Point(newPanX, newPanY));
		}

		/// <summary>
		/// Checks the pan and makes sure it is within range.
		/// </summary>
		private void CheckPan()
		{
			if (isEnabled)
			{
				Point maxPan = GetMaxPan();
				//Debug.WriteLine("MaxPan: {0}", maxPan);

				if (maxPan.X == 0) pan.X = 0;
				else if (pan.X < -maxPan.X) pan.X = -maxPan.X;
				else if (pan.X > maxPan.X) pan.X = maxPan.X;

				if (maxPan.Y == 0) pan.Y = 0;
				else if (pan.Y < -maxPan.Y) pan.Y = -maxPan.Y;
				else if (pan.Y > maxPan.Y) pan.Y = maxPan.Y;
			}
		}

		/// <summary>
		/// Gets the maximum pan.
		/// </summary>
		/// <returns>Maximum pan.</returns>
		private Point GetMaxPan()
		{
			return new Point(Math.Max(Math.Round((fitSize.Width * zoom - mediaElement.ActualWidth) / 2 / zoom), 0),
							  Math.Max(Math.Round((fitSize.Height * zoom - mediaElement.ActualHeight) / 2 / zoom), 0));
		}

		/// <summary>
		/// Sets the control's transform.
		/// </summary>
		private void SetTransform()
		{
			if (isEnabled)
			{
				// scale relative to the center
				scaleTransform.ScaleX = zoom;
				scaleTransform.ScaleY = zoom;

				// set the panning
				translateTransform.X = pan.X;
				translateTransform.Y = pan.Y;
			}
		}
	}
}
