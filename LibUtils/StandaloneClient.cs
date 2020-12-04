// References:
// https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-server-socket-example
// https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-1.1/5w7b7x5f(v=vs.71)

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace DStults.SocketClient
{

	public class StandaloneClient
	{

		private class ConnectionMetadata
		{
			public Socket Listener { get; private set; }
			public const int BufferSize = 1024;
			public byte[] Buffer = new byte[BufferSize];
			public StringBuilder Data = new StringBuilder(BufferSize);

			public ConnectionMetadata(Socket listener)
			{
				if (listener == null)
					throw new ArgumentNullException("Cannot create a new connection without a listening socket!");

				this.Listener = listener;
			}
		}

		private static string GetIpAddress(string defaultIP)
		{
			if (defaultIP == null || defaultIP.Length == 0)
				defaultIP = "localhost";
			
			System.Console.Write($"Server IP or hostname [{defaultIP}]: ");
			string input = Console.ReadLine();
			if (input == "")
			{
				System.Console.WriteLine($"Using: '{defaultIP}'.");
				return defaultIP;
			}
			try
			{
				IPAddress ipAddress = IPAddress.Parse(input);
			}
			catch (System.FormatException)
			{
				System.Console.WriteLine($"Not a valid IP address, checking for dns entry for '{input}'.");
				try
				{
					IPHostEntry ipHost = Dns.GetHostEntry(input);
					input = ipHost.AddressList[0].ToString();
					System.Console.WriteLine($"DNS entry found, using first IP for '{input}': {input}.");
				}
				catch (SocketException)
				{
					System.Console.WriteLine($"No DNS entry found for '{input}', cannot connect to server.");
					input = "";
				}
			}
			return input;
		}

		private static int GetPort(int defaultPort)
		{
			if (defaultPort <= 0 || defaultPort > 65535)
				defaultPort = 11111;

			System.Console.Write($"Server listening port [{defaultPort}]: ");
			try
			{
				string input = System.Console.ReadLine();
				if (input == "") return defaultPort;
				int? port = int.Parse(input);
				if (port >= 0 || port <= 65535)
				{
					return (int)port;
				}
				return defaultPort;
			}
			catch (System.FormatException)
			{
				return defaultPort;
			}
			catch (OverflowException)
			{
				return defaultPort;
			}
		}

		public static void RunClient(string hostName = "localhost", int port = 11111)
		{
			if (hostName == null)
				throw new ArgumentNullException("Error: StandaloneClient.hostName null string");
			if (hostName.Length == 0)
				throw new ArgumentNullException("Error: StandaloneClient.hostName empty string");
			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException("Error: StandaloneClient.port must be between 1 and 65535 (inclusive).");

			bool done = false;
			TcpClient myClient = null;
			while (!done)
			{

				// ESTABLISH CONNECTION -------------------------------------------------------------------------------------
				hostName = GetIpAddress(hostName);
				if (hostName == "")
				{
					Console.WriteLine("Unable to determine server IP address, try again?  (y,[n])");
					if (Console.ReadLine().ToLower().Contains("y"))
						continue;
					else
						done = true;
						break;
				}
				port = GetPort(port);
				myClient = GetClientConnection(hostName, port);
				if (myClient == null)
				{
					Console.WriteLine("Unable to connect to server, try again?  (y,[n])");
					if (Console.ReadLine().ToLower().Contains("y"))
						continue;
					else
						done = true;
						break;
				}
				Console.WriteLine("Connected to the Server!\n");
				Console.WriteLine("==================================================================\n");

				// SET UP ASYNC LISTENER FOR INPUT -------------------------------------------------------------------------------------

				ConnectionMetadata connection = new ConnectionMetadata(myClient.Client);
				myClient.Client.BeginReceive(connection.Buffer, 0, ConnectionMetadata.BufferSize, 0, new AsyncCallback(OnReceiveData), connection);

				// ESTABLISH STREAM -------------------------------------------------------------------------------------

				using NetworkStream SteamToServer = myClient.GetStream();
				{
					byte[] data = new byte[ConnectionMetadata.BufferSize];
					string userInput;

					// MAIN OUTPUT LOOP -------------------------------------------------------------------------------------

					while (!done)
					{

						userInput = Console.ReadLine();
						try
						{
							switch (userInput)
							{
								case "disconnect":
								case "dcon":
								case "exit":
								case "quit":
									userInput = "/disconnect";
									SteamToServer.Write(Encoding.ASCII.GetBytes(userInput), 0, userInput.Length);
									Console.WriteLine("Disconnecting from server...");
									done = true;
									break;
								default:
									if (userInput == "") userInput = " ";
									SteamToServer.Write(Encoding.ASCII.GetBytes(userInput), 0, userInput.Length);
									break;
							}
						}
						catch (IOException)
						{
							Console.Write("\n\nLost connection to server!\n\nReconnect? (y,[n])");
							if (Console.ReadLine().ToLower().Contains("y"))
							{
								done = false;
								break;
							}
							else
							{
								done = true;
								break;
							}
						}
						SteamToServer.Flush();
					}
				}
			}
			if (myClient != null)
				myClient.Close();

			System.Console.WriteLine("Program gracefully terminated.");

		}

		private static TcpClient GetClientConnection(string hostname, int port)
		{
			if (hostname == null)
			{
				Console.WriteLine("Attempted to establish connection with null hostname string");
				return null;
			}
			if (hostname.Length == 0)
			{
				Console.WriteLine("Attempted to establish connection with empty hostname string");
				return null;
			}
			if (port <= 0 || port > 65535)
			{
				Console.WriteLine("Invalid port number, port number must be 1-65535");
				return null;
			}

			for (int i = 0; i < 3; i++)
			{
				System.Console.Write($"Attempting to connect to server [{hostname}:{port}] ... ");
				try
				{
					TcpClient myClient = new TcpClient(hostname, port);
					System.Console.WriteLine("connected!");
					return myClient;
				}
				catch (SocketException ex)
				{
					switch (ex.ErrorCode)
					{
						case 10060:
							System.Console.WriteLine($"failed ({ex.ErrorCode}): Connection timed out!");
							break;
						case 10061:
							System.Console.WriteLine($"failed ({ex.ErrorCode}): Connection refused!");
							break;
						default:
							System.Console.WriteLine($"failed ({ex.ErrorCode}): {ex.Message}!");
							break;
					}
				}
			}
			return null;
		}

		public static void OnReceiveData(IAsyncResult ar)
		{
			ConnectionMetadata connection = (ConnectionMetadata)ar.AsyncState;
			if (connection.Listener.Connected)
			{
				try
				{
					int bytesReceived = connection.Listener.EndReceive(ar);
					if (bytesReceived > 0)
					{
						connection.Data.Clear();
						connection.Data.Append(Encoding.ASCII.GetString(connection.Buffer, 0, bytesReceived));

						Console.Write(connection.Data.ToString());

						connection.Listener.BeginReceive(connection.Buffer, 0, ConnectionMetadata.BufferSize, 0, new AsyncCallback(OnReceiveData), connection);
					}
				}
				catch (SocketException ex)
				{
					System.Console.WriteLine($" > Connection Error ({ex.ErrorCode}): {ex.Message}");
					if (connection.Listener.Connected)
						connection.Listener.Close();
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception);
				}
			}
		}

	}

}
