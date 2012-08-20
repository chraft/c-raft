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
using Chraft.Interfaces;

namespace Chraft.Persistence
{
    [Serializable]
    public sealed class ClientSurrogate
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Yaw { get; set; }
        public double Pitch { get; set; }
        public short Health { get; set; }
        public short Food { get; set; }
        public float FoodSaturation { get; set; }
        public Inventory Inventory { get; set; }
        public byte GameMode { get; set; }
        public string DisplayName { get; set; }
        public int SightRadius { get; set; }
    }
}
