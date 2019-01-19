﻿// Copyright © 2019 Shawn Baker using the MIT License.
using System.Collections.ObjectModel;

namespace RPiCameraViewer
{
    public class Camera
    {
		// public static properties
		public static bool ShowNetwork = false;

		// public properties
		public string Network { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
		public int Port { get; set; }

		public Camera()
		{
			Network = "";
			Name = "";
			Address = "";
			Port = Settings.DEFAULT_PORT;
		}

		/// <summary>
		/// Constructor - Initializes the properties.
		/// </summary>
		/// <param name="network">Network name.</param>
		/// <param name="name">Camera name.</param>
		/// <param name="address">IP address or host name.</param>
		/// <param name="port">Port number.</param>
		public Camera(string network, string name = null, string address = null, int port = 0)
		{
			Network = network;
			Name = name;
			Address = address;
			Port = port;
		}

		/// <summary>
		/// Gets the full address, including the network, based on the ShowNetwork property.
		/// </summary>
		public string FullAddress
		{
			get
			{
				string addr = Address + ":" + Port;
				if (ShowNetwork)
				{
					addr = Network + ":" + addr;
				}
				return addr;
			}
		}
	}

	public class Cameras : ObservableCollection<Camera>
	{
		public Camera Find(string network, string name)
		{
			foreach (Camera camera in this)
			{
				if (camera.Network == network && camera.Name == name)
				{
					return camera;
				}
			}
			return null;
		}
	}
}
