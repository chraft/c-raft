#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.World;

namespace ChraftTestClient
{
    public class TestClient
    {
        private ConcurrentQueue<Packet> _packetsToSend = new ConcurrentQueue<Packet>();
        //private PacketWriter _packetWriter;
        private PacketReader _packetReader;
        private bool _running;
        private string _userName;
        public const StreamRole Role = StreamRole.Client;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public UniversalCoords SpawnPosition { get; set; }
        public AbsWorldCoords Position { get; set; }

        private Socket _socket;

        private Task _sendTask;

        private ByteQueue _receiveBufferQueue;
        private ByteQueue _readingBufferQueue;
        private ByteQueue _fragPackets;

        private byte[] _recvBuffer = new byte[2048];

        private SocketAsyncEventArgs _socketAsyncArgs;

        private object _queueLock = new object();
        private Thread _receiveQueueReader;
        private Timer _globalTimer;

        private AutoResetEvent _recv = new AutoResetEvent(true);

        private int _chunksReceived;

        private int _time;

        public bool WaitInitialPositionRequest = true;

        private Random _rand;

        public TestClient(string name, Random rand)
        {
            _rand = rand;
            _userName = name;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _receiveBufferQueue = new ByteQueue();
            _readingBufferQueue = new ByteQueue();
            _fragPackets = new ByteQueue();
            _socketAsyncArgs = new SocketAsyncEventArgs();
            _receiveQueueReader = new Thread(ProcessReadQueue);
            Packet.Role = StreamRole.Client;
        }

        public void Start(IPEndPoint ipEnd)
        {
            _running = true;

            try
            {
                _socket.Connect(ipEnd);          
            }
            catch(Exception e)
            {
                using (StreamWriter sw = new StreamWriter("TC" + UserName, true))
                {
                    sw.WriteLine("ConnectionExc: {0}", e);
                }
            }

            _socketAsyncArgs.Completed += RecvCompleted;
            _socketAsyncArgs.SetBuffer(_recvBuffer, 0, 2048);
            _receiveQueueReader.Start();
            Task.Factory.StartNew(RecvPacket);

            SendPacket(new HandshakePacket { Username = _userName, ProtocolVersion = 40});
        }

        public void StartTimer()
        {
            if(_running)
                _globalTimer = new Timer(GlobalTimer, null, 100, 100);
        }

        public void GlobalTimer(object state)
        {
            int time = Interlocked.Increment(ref _time);

            if (!WaitInitialPositionRequest && (time % 5) == 0)
            {
                int factor = 1;
                double rand1;
                double rand2;

                if ((rand1 = _rand.NextDouble()) > (rand2 = _rand.NextDouble()))
                    factor = -1;

                double randX = (0.1 + rand1 * 0.2) * factor;
                double randZ = (0.1 + rand2 * 0.2) * factor;

                Position = new AbsWorldCoords(Position.X + randX, Position.Y, Position.Z + randZ);

                SendPacket(new PlayerPacket{OnGround = true});
                SendPacket(new PlayerPositionPacket{OnGround = true, X = Position.X, Y = Position.Y, Z = Position.Z});
            }

            if((time % 50) == 0)
            {
                SendPacket(new KeepAlivePacket{KeepAliveID = 1});
            }
        }
        public void SendMessage(string Message)
        {
            SendPacket(new ChatMessagePacket { Message = Message });
        }

        public void Dispose()
        {
            try
            {
                _running = false;
                _recv.Set();
                _receiveQueueReader.Abort();

                if (_globalTimer != null)
                {
                    _globalTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _globalTimer = null;
                }


                if (_socket.Connected)
                    _socket.Shutdown(SocketShutdown.Both);
                            
                _socket.Close();
            }
            catch (Exception e)
            {

                throw e;
            }
            
        }


        private void SendPacket(Packet packet)
        {
            _packetsToSend.Enqueue(packet);

            int sendRunning = Interlocked.CompareExchange(ref SendRunning, 1, 0);
            if (sendRunning == 0)
                _sendTask = Task.Factory.StartNew(Send);
        }

        private void RecvPacket()
        {
            if (!_running)
            {
                Dispose();
                return;
            }

            bool pending = _socket.ReceiveAsync(_socketAsyncArgs);

            if(!pending)
                RecvCompleted(null, _socketAsyncArgs);
        }

        private int Sends;
        private int SendRunning;

        private void Send()
        {
            int sends = Interlocked.Increment(ref Sends);

            if(sends > 1)
            {
                using (StreamWriter sw = new StreamWriter(String.Format("sends_{0}.log", _userName), true))
                {
                    sw.WriteLine("Multiple send tasks present: {0}", sends);
                }
            }
            if (!_running)
                return;

            int count = _packetsToSend.Count;
            for(int i = 0; i < count; ++i)
            {
                if (!_running)
                    return;

                Packet packet;
                _packetsToSend.TryDequeue(out packet);

                if (packet == null)
                    return;

                packet.Write();

                byte[] data = packet.GetBuffer();

                try
                {
                    _socket.Send(data);
                }
                catch (Exception e)
                {
                    if (_running)
                    {
                        Dispose();
                    }
                }
               
            }

            Interlocked.Exchange(ref SendRunning, 0);

            Interlocked.Decrement(ref Sends);
            if (_running && !_packetsToSend.IsEmpty)
            {
                int running = Interlocked.Exchange(ref SendRunning, 1);

                if(running == 0)
                    _sendTask = Task.Factory.StartNew(Send);
            }

        }

        private void RecvCompleted(object sender, SocketAsyncEventArgs e)
        {
            if(e.SocketError != SocketError.Success)
            {
                if(_running)
                    Dispose();
                return;
            }

            if (!_running)
                return;

            if (e.BytesTransferred > 0)
            {
                lock (_queueLock)
                    _receiveBufferQueue.Enqueue(e.Buffer, 0, e.BytesTransferred);

                _recv.Set();
                RecvPacket();
            }
        }

        private void ProcessReadQueue()
        {
            while (_recv.WaitOne())
            {
                ByteQueue temp;

                lock (_queueLock)
                {
                    temp = _receiveBufferQueue;
                    _receiveBufferQueue = _readingBufferQueue;
                }

                _readingBufferQueue = temp;

                int length = _fragPackets.Size + _readingBufferQueue.Size;

                while (length > 0)
                {
                    byte packetType;

                    if (_fragPackets.Size > 0)
                        packetType = _fragPackets.GetPacketID();
                    else
                        packetType = _readingBufferQueue.GetPacketID();

                    /*using(StreamWriter sw = new StreamWriter(String.Format("recv_packets{0}.log", _userName), true))
                    {
                        byte[] enums = (byte[])Enum.GetValues(typeof (PacketType));
                        int i = Array.BinarySearch(enums, 0, enums.Length, packetType);

                        if(i != -1)
                            sw.WriteLine("{0} - Received packet {1}", DateTime.Now, ((PacketType)packetType));
                        else
                            sw.WriteLine("{0} - Received packet {1}", DateTime.Now, packetType);
                    }*/

                    //result = HandlePacket(packetType, length);

                    ClientPacketHandler handler = PacketHandlers.GetHandler((PacketType)packetType);

                    if (handler == null)
                    {
                        using (StreamWriter sw = new StreamWriter(String.Format("unhandled_packets_{0}.log", _userName), true))
                        {
                            byte[] unhandled = GetBufferToBeRead(length);

                            sw.WriteLine("{0} - PacketType: {1}", DateTime.Now, unhandled[0]);
                            sw.WriteLine(BitConverter.ToString(unhandled));
                        }

                        length = 0;
                    }
                    else if (handler.Length == 0)
                    {
                        byte[] data = GetBufferToBeRead(length);
                        /*using (StreamWriter sw = new StreamWriter(String.Format("recv_packets{0}.log", _userName), true))
                        {*/
                            if (length >= handler.MinimumLength)
                            {
                                PacketReader reader = new PacketReader(data, length);

                                handler.OnReceive(this, reader);
                                
                                // If we failed it's because the packet isn't complete
                                if (reader.Failed)
                                {
                                    //sw.WriteLine("Failed Fragmented: {0}", BitConverter.ToString(data));
                                    EnqueueFragment(data);
                                    length = 0;
                                }
                                else
                                {
                                    //sw.WriteLine("Variable length: {0}", BitConverter.ToString(data, 0, reader.Index));
                                    _readingBufferQueue.Enqueue(data, reader.Index, data.Length - reader.Index);
                                    length = _readingBufferQueue.Length;
                                }
                            }
                            else
                            {
                                //sw.WriteLine("Fragmented variable: {0}", BitConverter.ToString(data));
                                // Not enough data
                                EnqueueFragment(data);
                                length = 0;
                            }
                        //}

                    }
                    else if (length >= handler.Length)
                    {
                        byte[] data = GetBufferToBeRead(handler.Length);
                        /*using (StreamWriter sw = new StreamWriter(String.Format("recv_packets{0}.log", _userName), true))
                        {                          
                            sw.WriteLine("Fixed length: {0}", BitConverter.ToString(data));
                        }*/
                        PacketReader reader = new PacketReader(data, handler.Length);

                        handler.OnReceive(this, reader);

                        // If we failed it's because the packet is wrong
                        if (reader.Failed)
                        {
                            Dispose();
                            length = 0;
                        }
                        else
                        {
                            if (_fragPackets.Length > 0)
                                throw new Exception("Fragpackets must be empy here!");
                            length = _readingBufferQueue.Length;
                        }
                            
                    }
                    else
                    {
                        /*using (StreamWriter sw = new StreamWriter(String.Format("recv_packets{0}.log", _userName), true))
                        { */                          
                            byte[] data = GetBufferToBeRead(length);
                            //sw.WriteLine("Fragmented fixed: {0}", BitConverter.ToString(data));
                            EnqueueFragment(data);
                            length = 0;
                        //}
                    }
                }
            }
        }

        public static void HandlePacketKeepAlive(TestClient client, KeepAlivePacket ka)
        {
            client.SendPacket(new KeepAlivePacket());
        }

        public static void HandlePacketLoginRequest(TestClient client, LoginRequestPacket lr)
        {
            // Do something when logged
            client.StartTimer();
        }

        public static void HandlePacketChatMessage(TestClient client, ChatMessagePacket cm)
        {
            // Handle some commands?
        }

        public static void HandlePacketDisconnect(TestClient client, DisconnectPacket dp)
        {
            client.Dispose();

            using (StreamWriter sw = new StreamWriter(String.Format("disconnect_reason_{0}.log", client.UserName), true))
            {
                sw.WriteLine("{0} - {1}", DateTime.Now, dp.Reason);
            }
        }

        public static void HandlePacketPlayerPositionRotation(TestClient client, PlayerPositionRotationPacket ppr)
        {
            client.Position = new AbsWorldCoords(ppr.X, ppr.Y, ppr.Z);

            client.SendPacket(ppr);
            client.WaitInitialPositionRequest = false;
        }

        public static void HandlePacketSpawnPosition(TestClient client, SpawnPositionPacket sp)
        {
            client.SpawnPosition = UniversalCoords.FromWorld(sp.X, sp.Y, sp.Z);
        }

        private void EnqueueFragment(byte[] data)
        {
            int fragPacketWaiting = _fragPackets.Length;
            // We are waiting for more data than an uncompressed chunk, it's not possible
            if (fragPacketWaiting > 81920)
                Dispose();
            else
                _fragPackets.Enqueue(data, 0, data.Length);
        }     

        private byte[] GetBufferToBeRead(int length)
        {
            int availableData = _fragPackets.Size + _readingBufferQueue.Size;

            if (length > availableData)
                return null;

            int fromFrag;

            byte[] data = new byte[length];

            if (length >= _fragPackets.Size)
                fromFrag = _fragPackets.Size;
            else
                fromFrag = length;

            _fragPackets.Dequeue(data, 0, fromFrag);

            int fromProcessed = length - fromFrag;

            _readingBufferQueue.Dequeue(data, fromFrag, fromProcessed);

            return data;
        }
    }
}
