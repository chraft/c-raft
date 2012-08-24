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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Interfaces.Containers;
using Chraft.Net.Packets;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.World;
using Chraft.Interfaces.Recipes;
using System.Threading;
using Chraft.World.Blocks;

namespace Chraft.Interfaces
{
    public class FurnaceInterface : PersistentContainerInterface
    {

        public FurnaceInterface(WorldManager world, UniversalCoords coords)
            : base(world, coords, InterfaceType.Furnace, 3)
		{
		}

        protected override bool SharedSlotClicked(WindowClickPacket packet)
        {
            if (packet.Slot == (short)FurnaceSlots.Output)
            {
                if (Container[packet.Slot].IsVoid())
                {
                    Owner.Client.SendPacket(new TransactionPacket
                    {
                        Accepted = false,
                        Transaction = packet.Transaction,
                        WindowId = packet.WindowId
                    });
                    return false;
                }
                ItemStack output = Container[packet.Slot];
                if (!Cursor.IsVoid())
                {
                    if (!Cursor.StacksWith(output) || (Cursor.StacksWith(output) && Cursor.Count >= 64))
                    {
                        Owner.Client.SendPacket(new TransactionPacket
                        {
                            Accepted = false,
                            Transaction = packet.Transaction,
                            WindowId = packet.WindowId
                        });
                        return false;
                    }
                }
                ItemStack newOutput = ItemStack.Void;
                if (Cursor.IsVoid())
                {
                    Cursor = new ItemStack(output.Type, output.Count, output.Durability);
                    Cursor.Slot = -1;
                }
                else
                {
                    int freeSpaceInCursor = 64 - Cursor.Count;
                    int takeFromOutput = (output.Count > freeSpaceInCursor ? freeSpaceInCursor : output.Count);
                    Cursor.Count += (sbyte)takeFromOutput;
                    if (takeFromOutput < output.Count)
                        newOutput = new ItemStack(output.Type, (sbyte)(output.Count - takeFromOutput), output.Durability);
                }
                Container.ChangeSlot(Handle, packet.Slot, newOutput);
                this[(short)FurnaceSlots.Output] = newOutput;
                return false;
            }

            return base.SharedSlotClicked(packet);
        }

        public void SendUpdateProgressBar(FurnaceContainer.FurnaceBar bar, short level)
        {
            Owner.Client.SendPacket(new UpdateWindowPropertyPacket
            {
                WindowId = Handle,
                Property = (short)bar,
                Value = level,
            });
        }

        public enum FurnaceSlots : short
        {
            Input = 0,
            Fuel = 1,
            Output = 2,
            InventoryFirst = 3,
            InventoryLast = 29,
            QuickSlotFirst = 30,
            QuickSlotLast = 38
        }
	}
}
