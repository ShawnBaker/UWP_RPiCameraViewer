using Windows.Networking;
using Windows.Networking.Sockets;

namespace RPiCameraViewer.Classes
{
	public class SocketReader
	{
		private StreamSocket socket;

		public SocketReader(string address, int port)
		{
			socket = new StreamSocket();
			HostName hostName = new HostName(address);
			//socket.ConnectAsync(hostName, port);
		}
	}
}
