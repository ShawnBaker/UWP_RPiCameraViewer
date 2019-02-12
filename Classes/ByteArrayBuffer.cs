// Copyright © 2019 Shawn Baker using the MIT License.
using System;
using Windows.Storage.Streams;

namespace RPiCameraViewer
{
	public class ByteArrayBuffer : IBuffer
	{
		// public constants
		public const uint DefaultCapacity = 16384;

		// instance variables
		private byte[] buffer;
		private uint length;

		/// <summary>
		/// Constructor - Allocates the buffer.
		/// </summary>
		/// <param name="capacity">Size of the buffer to allocate.</param>
		public ByteArrayBuffer(uint capacity = DefaultCapacity)
		{
			if (capacity == 0)
			{
				capacity = DefaultCapacity;
			}
			buffer = new byte[capacity];
		}

		/// <summary>
		/// Array of bytes that is the buffer.
		/// </summary>
		public byte[] Buffer => buffer;

		/// <summary>
		/// Size of the buffer in bytes.
		/// </summary>
		public uint Capacity => (uint)buffer.Length;

		/// <summary>
		/// Number of bytes being used.
		/// </summary>
		public uint Length { get => length; set => length = Math.Max(Math.Min(value, Capacity), 0); }

		/// <summary>
		/// Resizes the buffer.
		/// </summary>
		/// <param name="capacity">New capacity.</param>
		public void Resize(uint capacity)
		{
			Array.Resize(ref buffer, (int)capacity);
		}
	}
}
