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
using Chraft.Net;

namespace ChraftTestClient
{
    public delegate void OnPacketReceive(TestClient client, PacketReader pvSrc);
    public class ClientPacketHandler
    {
        private int _Length;
        private int _MinimumLength;
        private OnPacketReceive _OnReceive;
        private PacketType _PacketId;

        public PacketType PacketId
        {
            get { return _PacketId; }
        }

        public int Length
        {
            get { return _Length; }
        }

        public int MinimumLength
        {
            get { return _MinimumLength; }
        }

        public OnPacketReceive OnReceive
        {
            get { return _OnReceive; }
        }

        public ClientPacketHandler(PacketType type, int length, int minimumLength, OnPacketReceive onReceive)
        {
            _PacketId = type;
            _Length = length;
            _MinimumLength = minimumLength;
            _OnReceive = onReceive;
        }
    }
}
