using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DStults.SocketServer
{

	public interface IClientConnectable
	{
		string Nickname { get; }
		string ExtraInfo { get; }
		Socket Socket { get; }
		byte[] Buffer {get;}
		string GetImmediateReplyForClient();
		void HandleInputFromClientToServer(string incomingData);
	}

	internal static class NetworkServer
	{

		public const int BufferSize = 1024;
		private static int port = 11111;
		private static bool listenerClosed = true;

		private static Socket listenerSocket;

		private static int ClientCounter = 0; // total connections ever, not current total
		private static List<IClientConnectable> Clients = new List<IClientConnectable>();

		private static void Log(string text)
		{
			if (text == null)
				throw new ArgumentNullException("Error: NetworkServer.Log null text");
			if (text.Length == 0)
				throw new ArgumentException("Error: NetworkServer.Log empty text");

			Console.Write(text);
		}

		public static string GetConnectionPolling(bool verbose = false, bool detailed = false)
		{
			StringBuilder sb = new StringBuilder();
			if (Clients.Count == 0)
			{
				if (verbose)
					return " > No clients connected!";
			}
			else
			{
				bool pollMissing;
				var stopwatch = new Stopwatch();
				for (int i = Clients.Count - 1; i >= 0; i--)
				{
					if (verbose)
					{
						stopwatch.Reset();
						stopwatch.Start();
					}
					pollMissing = Clients[i].Socket.Poll(1, SelectMode.SelectRead);
					if (verbose)
					{
						stopwatch.Stop();
						sb.Append($"\n > {Clients[i].Nickname}    (Ping: {stopwatch.ElapsedMilliseconds} ms){Clients[i].ExtraInfo}\n");
						if (detailed)
							sb.Append(GetSocketInfo(Clients[i].Socket));
					}
					if (pollMissing)
					{
						RemoveClient(Clients[i], true);
					}
				}
			}
			return $"{sb.ToString()}\n";
		}

		public static bool TryTransmitToClient(IClientConnectable client, string text)
		{
			if (client == null)
				throw new ArgumentNullException("Error: NetworkServer.TransmitToClient null client");
			if (!Clients.Contains(client))
				return false;
			if (text == null)
				throw new ArgumentNullException("Error: NetworkServer.TransmitToClient null string");
			if (text.Length == 0)
				throw new ArgumentException("Error: NetworkServer.TransmitToClient empty string");

			if (text.Length > 0)
			{
				try
				{
					byte[] msg = Encoding.ASCII.GetBytes(text);
					client.Socket.SendAsync(msg, SocketFlags.None);
					return true;
				}
				catch (SocketException ex)
				{
					ErrorMessage(ex);
					RemoveClient(client, false);
					return false;
				}
				catch (Exception ex)
				{
					ErrorMessage(ex);
					RemoveClient(client, false);
					return false;
				}
			}
			return false;
		}

		// =======================================================================================
		//     ASYNC LISTENER
		// =======================================================================================

		public static void CreateAsyncListener<T>() where T : IClientConnectable
		{
			Log(" - Enabling async listener...");
			listenerClosed = false;
			listenerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			listenerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			listenerSocket.Listen(1);
			listenerSocket.BeginAccept(new AsyncCallback(TryAcceptConnection<T>), null);
			Log("done.\n");
		}

		public static void CloseAsyncListener()
		{
			Log(" - Disabling async listener...");
			listenerClosed = true;
			listenerSocket.Close();
			Log("done.\n");
		}

		private static void TryAcceptConnection<T>(IAsyncResult AR) where T : IClientConnectable
		{
			if (listenerClosed)
				return;
			try
			{
				Socket newSocket = listenerSocket.EndAccept(AR);
				IClientConnectable newClient = (T)Activator.CreateInstance(typeof(T), new object[] { newSocket, ++ClientCounter });
				AddClient(newClient);
				newClient.Socket.BeginReceive(newClient.Buffer, 0, BufferSize, SocketFlags.None, new AsyncCallback(OnReceiveData), newClient);
				listenerSocket.BeginAccept(new AsyncCallback(TryAcceptConnection<T>), null);

				if (TryTransmitToClient(newClient, "\nYou are now connected to the network server. For connection-related support, type '/help'.\n\n"))
					TryTransmitToClient(newClient, newClient.GetImmediateReplyForClient());
			}
			catch (Exception ex)
			{
				ErrorMessage(ex);
			}
		}

		// =======================================================================================
		//     ASYNC RECEIVER
		// =======================================================================================

		private static void OnReceiveData(IAsyncResult ar)
		{
			IClientConnectable thisClient = (IClientConnectable)ar.AsyncState;
			if (thisClient == null)
				return;
			if (!Clients.Contains(thisClient))
			{
				Log($"Discarding listener for {thisClient}.\n");
				return;
			}

			int bytesReceived = 0;
			if (thisClient.Socket.Connected)
			{
				try
				{
					bytesReceived = thisClient.Socket.EndReceive(ar);
					if (bytesReceived > 0)
					{
						string incomingData = Encoding.ASCII.GetString(thisClient.Buffer, 0, bytesReceived).Trim();
						if (incomingData.Length > 0 && incomingData.Substring(0, 1) == "/")
						{
							string feedback = HandleConsoleCommands(thisClient, incomingData);
							if (TryTransmitToClient(thisClient, $"{feedback}\n"))
								thisClient.Socket.BeginReceive(thisClient.Buffer, 0, BufferSize, 0, new AsyncCallback(OnReceiveData), thisClient);
						}
						else
						{
							Log($"{thisClient.Nickname}: [{incomingData}]\n");

							thisClient.HandleInputFromClientToServer(incomingData);
							if (TryTransmitToClient(thisClient, thisClient.GetImmediateReplyForClient()))
								thisClient.Socket.BeginReceive(thisClient.Buffer, 0, BufferSize, 0, new AsyncCallback(OnReceiveData), thisClient);
						}
					}
				}
				catch (SocketException ex)
				{
					switch (ex.ErrorCode)
					{
						case 10054:
							Log($"Error ({ex.ErrorCode}): {thisClient.Nickname} dropped!\n");
							RemoveClient(thisClient, false);
							break;
						default:
							ErrorMessage($"SocketException ({ex.ErrorCode}): {ex.Message}!", ex);
							break;
					}
					if (thisClient.Socket.Connected)
						thisClient.Socket.Close();
				}
				catch (Exception ex)
				{
					ErrorMessage(ex);
				}
			}
		}

		private static string HandleConsoleCommands(IClientConnectable client, string incomingData)
		{
			if (client == null)
				throw new ArgumentNullException("Error: NetworkServer.HandleConsoleCommands null client");
			if (incomingData == null)
				throw new ArgumentNullException("Error: NetworkServer.HandleConsoleCommands null data");
			if (incomingData.Length == 0)
				throw new ArgumentNullException("Error: NetworkServer.HandleConsoleCommands empty data");

			List<string> args = new List<string>(incomingData.Split(' '));
			if (args.Count > 0)
			{
				string arg0 = args[0].ToLower();
				if (arg0.Contains("help"))
				{
					return "Console commands: '/disconnect' or '/dcon' disconnects client";
				}
				else if (arg0.Contains("disconnect") || arg0.Contains("dcon"))
				{
					DisconnectClient(client, "You have been disconnected from the server.", "client disconnect");
					return "Client Disconnect Confirm";
				}
			}
			return $"Unhandled or unknown command: {incomingData}, type '/help' for help.";
		}

		public static void DisconnectClient(IClientConnectable client, string fullText, string reason)
		{			
			if (client == null)
				throw new ArgumentNullException("Error: NetworkServer.DisconnectClient null client");
			if (fullText == null)
				throw new ArgumentNullException("Error: NetworkServer.DisconnectClient null text");
			if (fullText.Length == 0)
				throw new ArgumentException("Error: NetworkServer.DisconnectClient empty text");
			if (reason == null)
				throw new ArgumentNullException("Error: NetworkServer.DisconnectClient null reason");
			if (fullText.Length == 0)
				throw new ArgumentException("Error: NetworkServer.DisconnectClient empty reason");

			TryTransmitToClient(client, fullText);
			RemoveClient(client, true, reason);
		}

		private static void AddClient(IClientConnectable client)
		{
			if (client == null)
				throw new ArgumentNullException("Error: NetworkServer.AddClient null client");

			lock (Clients)
			{
				Clients.Add(client);
			}
			Log($" > {client.Nickname} connected!\n");
		}

		private static void RemoveClient(IClientConnectable client, bool wasGraceful, string extraInfo = "")
		{
			if (client == null)
				throw new ArgumentNullException("Error: NetworkServer.RemoveClient null client");
			if (extraInfo == null)
				extraInfo = "";
			// extraInfo.Length == 0  OK

			if (wasGraceful)
			{
				if (extraInfo != "")
					Log($" > {client.Nickname} disconnected: {extraInfo}\n");
				else
					Log($" > {client.Nickname} disconnected!\n");
			}

			if (client.Socket.Connected)
				client.Socket.Close();
			lock (Clients)
			{
				if (Clients.Contains(client))
					Clients.Remove(client);
			}

		}

		private static void ErrorMessage(Exception ex)
		{
			ErrorMessage("Unhandled Exception!\nPlease send this information to drankof@gmail.com", ex);
		}

		private static void ErrorMessage(string message, Exception ex)
		{
			if (message == null)
				throw new ArgumentNullException("Error: NetworkServer.ErrorMessage null message");
			if (message.Length == 0)
				throw new ArgumentException("Error: NetworkServer.ErrorMessage empty message");

			Log($"\n\n  ====>  {message}:\n\n{ex}\n");
		}

		private static string GetSocketInfo(Socket socket)
		{
			if (socket == null)
				throw new ArgumentNullException("Error: NetworkServer.GetSocketInfo null socket");

			StringBuilder sb = new StringBuilder();
			sb.Append("Socket:\n");
			sb.Append($"      AddressFamily: {socket.AddressFamily}\n");
			sb.Append($"          Available: {socket.Available}\n");
			sb.Append($"           Blocking: {socket.Blocking}\n");
			sb.Append($"          Connected: {socket.Connected}\n");
			//sb.Append($"       DontFragment: {socket.DontFragment}\n");         // breaks program
			//sb.Append($"           DualMode: {mySocket.DualMode}\n");           // breaks program
			//sb.Append($"    EnableBroadcast: {mySocket.EnableBroadcast}\n");    // breaks program
			sb.Append($"ExclusiveAddressUse: {socket.ExclusiveAddressUse}\n");
			sb.Append($"             Handle: {socket.Handle}\n");
			sb.Append($"            IsBound: {socket.IsBound}\n");
			sb.Append($"        LingerState: {socket.LingerState.Enabled}: {socket.LingerState.LingerTime}\n");
			if (!socket.IsBound)
				sb.Append($"      LocalEndPoint: ( not bound )\n");
			else
				sb.Append($"      LocalEndPoint: {socket.LocalEndPoint}\n");
			//sb.Append($"  MulticastLoopback: {mySocket.MulticastLoopback}\n");  // breaks program
			sb.Append($"            NoDelay: {socket.NoDelay}\n");
			sb.Append($"       ProtocolType: {socket.ProtocolType}\n");
			sb.Append($"  ReceiveBufferSize: {socket.ReceiveBufferSize}\n");
			sb.Append($"     ReceiveTimeout: {socket.ReceiveTimeout}\n");
			if (!socket.Connected)
				sb.Append($"     RemoteEndPoint: ( not connected )\n");
			else
				sb.Append($"     RemoteEndPoint: {socket.RemoteEndPoint}\n");
			sb.Append($"         SafeHandle: IsClosed? {socket.SafeHandle.IsClosed}  Invalid? {socket.SafeHandle.IsInvalid}\n");
			sb.Append($"     SendBufferSize: {socket.SendBufferSize}\n");
			sb.Append($"  MulticastLoopback: {socket.SendTimeout}\n");
			sb.Append($"         SocketType: {socket.SocketType}\n");
			sb.Append($"                Ttl: {socket.Ttl}\n");
			sb.Append($"UseOnlyOverlappedIO: {socket.UseOnlyOverlappedIO}\n");
			return sb.ToString();
		}

	}

}