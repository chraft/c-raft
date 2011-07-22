using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Chraft.Net;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using Chraft.World;

namespace MineChraft
{
	public partial class Client : Game
	{
		private Timer KeepAliveTimer;
		public PacketHandler Packets { get; set; }

		private void InitializeConnection()
		{
			TcpClient tcp = new TcpClient("localhost", 25568);
			Packets = new PacketHandler(new BigEndianStream(tcp.GetStream()));
			Packets.Handshake += new PacketEventHandler<HandshakePacket>(Packets_Handshake);
			Packets.LoginRequest += new PacketEventHandler<LoginRequestPacket>(Packets_LoginRequest);
			Packets.ChatMessage += new PacketEventHandler<ChatMessagePacket>(Packets_ChatMessage);

			Thread thread = new Thread(RxProc);
			thread.Start();

			Packets.SendPacket(new HandshakePacket
			{
				UsernameOrHash = "Test"
			});
		}

		private void RxProc()
		{
			while (true)
			{
				try
				{
					Packets.ProcessPacket();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		private void SendMessage(string message)
		{
			Packets.SendPacket(new ChatMessagePacket
			{
				Message = message
			});
		}

		private void Packets_ChatMessage(object sender, PacketEventArgs<ChatMessagePacket> e)
		{
			ChatlogLines.Add(Regex.Replace(e.Packet.Message, "§[0-9a-f]", string.Empty));
			UpdateLines();
		}

		private void Packets_LoginRequest(object sender, PacketEventArgs<LoginRequestPacket> e)
		{
			KeepAliveTimer = new Timer(KeepAliveTimer_Callback, null, 50, 50);
		}

		int KeepAliveCount = 0;
		private void KeepAliveTimer_Callback(object state)
		{
			Packets.SendPacket(new KeepAlivePacket());
			if (KeepAliveCount++ > 100)
			{
				KeepAliveCount = 0;
				Packets.SendPacket(new PlayerPacket()
				{
					OnGround = true
				});
			}
		}

		private void Packets_Handshake(object sender, PacketEventArgs<HandshakePacket> e)
		{
			Packets.SendPacket(new LoginRequestPacket
			{
				Username = "Test",
				Password = "",
				Dimension = 0,
				MapSeed = 0,
				ProtocolOrEntityId = 9
			});
		}
	}
}
