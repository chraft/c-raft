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

using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks;

namespace Chraft.Entity.Items.Base
{
    public abstract class ItemInventory : ItemBase, IItemInventory
    {
        //public short Slot { get; set; }

        /*public bool IsEnchantable()
        {
            if ((Type >= 256 && Type <= 258) || Type == 261 || (Type >= 267 && Type <= 279)
                || (Type >= 283 && Type <= 286) || (Type >= 290 && Type <= 294) || (Type >= 298 && Type <= 317))
                return true;
            return false;
        }*/

        public ItemInventory()
        {
            Damage = 1;
        }

        internal override void Write(Net.BigEndianStream stream)
        {
            base.Write(stream);
            if (Type >= 0)
            {
                if (Durability > 0 || IsEnchantable)
                    stream.Write((short)-1);
                // TODO: Remove the two lines above and implement items and enchantments write
                /* 
                 * if (Item.CanBeDamaged())
                 * {
                 *      if(_enchantments != null)
                 *          WriteEnchantmentsToNBT(stream);
                 *      else
                 *          stream.Write(-1);
                 * }
                 */

            }
        }

        public void DestroyBlock(IStructBlock blockHit)
        {
            var player = Owner.GetPlayer() as Player;
            BlockHelper.Instance.CreateBlockInstance(blockHit.Type).Destroy(player, blockHit);
            if (player != null && player.GameMode == GameMode.Normal)
                DamageItem(blockHit);
        }

        public void DamageItem(short value)
        {
            short durability = 0;

            BlockData.ToolDuarability.TryGetValue((BlockData.Items)Type, out durability);

            if (durability > 0)
            {
                if (Durability >= durability)
                {
                    if (Count == 1)
                    {
                        Owner.SetItem(Slot, ItemHelper.Void);
                    }
                    else // This will allow stacked tools to work properly.
                    {
                        Durability = 0;
                        Count--;
                    }
                }
                else
                {
                    Durability += value;
                }
            }

        }

        public void DamageItem(IStructBlock blockHit)
        {
            // TODO: Apply proper damage
            DamageItem(1);
        }

        public new ItemInventory Clone()
        {
            return base.Clone() as ItemInventory;
        }
    }
}
