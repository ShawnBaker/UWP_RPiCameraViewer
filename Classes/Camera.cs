// Copyright © 2019 Shawn Baker using the MIT License.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

		/// <summary>
		/// Creates a string representing the camera.
		/// </summary>
		/// <returns>A string representing the camera.</returns>
		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3}", Network, Name, Address, Port);
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

		public void SortByName()
		{
			List<Camera> sorted = Items.OrderBy(x => x.Name).ToList();
			for (int i = 0; i < sorted.Count; i++)
			{
				Move(IndexOf(sorted[i]), i);
			}
		}

		public void SortByAddress()
		{
			List<Camera> sorted = Items.OrderBy(x => x.Address).ToList();
			for (int i = 0; i < sorted.Count; i++)
			{
				Move(IndexOf(sorted[i]), i);
			}
		}
	}
}
