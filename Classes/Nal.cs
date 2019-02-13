// Copyright © 2019 Shawn Baker using the MIT License.
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace RPiCameraViewer
{
	public class Nal
	{
		// public constants
		public const uint DefaultCapacity = 16384;

		// instance variables
		public Buffer Buffer;
		public Stream Stream;

		/// <summary>
		/// Constructor - Allocates the buffer.
		/// </summary>
		/// <param name="capacity">Size of the buffer to allocate.</param>
		public Nal(uint capacity = DefaultCapacity)
		{
			if (capacity <= 0)
			{
				capacity = DefaultCapacity;
			}
			Buffer = new Buffer(capacity);
			Stream = Buffer.AsStream();
		}

		/// <summary>
		/// Resizes the buffer.
		/// </summary>
		/// <param name="capacity">New capacity.</param>
		public void Resize(uint capacity)
		{
			Stream = null;
			Buffer newBuffer = new Buffer(capacity);
			Buffer.CopyTo(0, newBuffer, 0, Buffer.Capacity);
			Stream = newBuffer.AsStream();
			Stream.Seek(Buffer.Capacity, SeekOrigin.Begin);
			Stream.SetLength(Buffer.Capacity);
			Buffer = newBuffer;
		}
	}
}
