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

using Chraft.Entity.Items;
using Chraft.Interfaces.Containers;
using Chraft.Net.Packets;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.World;

namespace Chraft.Interfaces
{
    /// <summary>
    /// A container interface that persists within the World (e.g. Small Chest, Dispenser, Large Chest)
    /// </summary>
    /// <remarks>
    /// The storage mechanism used here is a temporary measure until the data is correctly stored within Chunks instead
    /// </remarks>
    public abstract class PersistentContainerInterface: Interface
    {
        public WorldManager World { get; private set; }
        public UniversalCoords Coords { get; private set; }

        public PersistentContainer Container;

        internal PersistentContainerInterface(WorldManager world, UniversalCoords coords, InterfaceType interfaceType, sbyte slotCount)
            : base(interfaceType, slotCount)
        {
            World = world;
            Coords = coords;
        }

        protected override void DoClose()
        {
            ContainerFactory.Close(this, Coords);
            base.DoClose();
        }

        internal override void OnClicked(WindowClickPacket packet)
        {
            if (IsSharedSlot(packet.Slot))
            {
                if (!SharedSlotClicked(packet))
                    return;
                base.OnClicked(packet);
                Container.ChangeSlot(packet.WindowId, packet.Slot, this[packet.Slot]);
            }
            else
                base.OnClicked(packet);
        }

        protected virtual bool IsSharedSlot(short slot)
        {
            return (slot >= 0 && slot < SlotCount);
        }

        protected virtual bool SharedSlotClicked(WindowClickPacket packet)
        {
            if (Container.SlotCanBeChanged(packet))
                return true;

            Owner.Client.SendPacket(new TransactionPacket
            {
                Accepted = false,
                Transaction = packet.Transaction,
                WindowId = packet.WindowId
            });
            return false;
        }

        protected override void DoOpen()
        {
            for (short i = 0; i < Container.SlotsCount; i++)
                this[i] = Container[i];
            base.DoOpen();
        }
    }
}
