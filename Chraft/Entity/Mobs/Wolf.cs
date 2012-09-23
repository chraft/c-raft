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
using Chraft.Net;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Misc;
using Chraft.World;


namespace Chraft.Entity.Mobs
{
    public class Wolf : Animal
    {
        public override string Name
        {
            get { return "Wolf"; }
        }

        public override short AttackStrength
        {
            get
            {
                return (short)((Data.IsTamed) ? 4 : 2); // Wild 2, Tame 4;
            }
        }

        public override int AttackRange
        {
            get
            {
                return 1;
            }
        }

        public override short Health
        {
            get
            {
                return base.Health;
            }
            set
            {
                base.Health = value;
                Data.Health = Health;
            }
        }

        public override short MaxHealth
        {
            get
            {
                return (short)((Data.IsTamed) ? 20 : 8); // Wild 8, Tame 20;
            }
        }

        public override int MaxSpawnedPerGroup
        {
            get
            {
                return 8;
            }
        }

        protected virtual int BonesUntilTamed { get; set; }

        internal Wolf(WorldManager world, int entityId, MetaData data = null)
            : base(world, entityId, MobType.Wolf, data)
        {
            Data.IsSitting = false;
            Data.IsTamed = false;
            Data.IsAggressive = false;
            BonesUntilTamed = Server.Rand.Next(10); // How many bones required to tame this wolf?
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            base.DoDeath(killedBy);
        }

        protected override void DoInteraction(IClient iClient, IItemInventory item)
        {
            base.DoInteraction(iClient, item);

            Client client = iClient as Client;
            if (client == null)
                return;

            if (item != null && !ItemHelper.IsVoid(item))
            {
                if (item is ItemRawPorkchop || item is ItemCookedPorkchop)
                {
                    client.Owner.Inventory.RemoveItem(item.Slot); // consume the item
                    
                    if (Data.IsTamed)
                    {
                        // Feed a tame wolf pork chop
                        if (Health < MaxHealth)
                        {
                            if (Health < MaxHealth)
                            {
                                Health += 3; // Health is clamped, no need to check if exceeds MaxHealth
                                SendMetadataUpdate();
                            }
                        }
                    }
                }
                else if (!Data.IsTamed && item.Type == (short)BlockData.Items.Bone)
                {
                    // Give a bone
                    BonesUntilTamed--;
                    client.Owner.Inventory.RemoveItem(item.Slot); // consume the item

                    if (BonesUntilTamed <= 0)
                    {
                        Data.IsTamed = true;
                        Data.TamedBy = client.Username;
                        Health = MaxHealth;
                        // TODO: begin following this.Data.TamedBy
                        SendMetadataUpdate();
                    }
                }
            }
        }
    }
}
