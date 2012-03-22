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
using Chraft.Net;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Misc;
using Chraft.World;
using Chraft.Interfaces;

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
                return (short)((this.Data.IsTamed) ? 4 : 2); // Wild 2, Tame 4;
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
                this.Data.Health = this.Health;
            }
        }

        public override short MaxHealth
        {
            get
            {
                return (short)((this.Data.IsTamed) ? 20 : 8); // Wild 8, Tame 20;
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

        internal Wolf(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Wolf, data)
        {
            this.Data.IsSitting = false;
            this.Data.IsTamed = false;
            this.Data.IsAggressive = false;
            this.BonesUntilTamed = Server.Rand.Next(10); // How many bones required to tame this wolf?
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            base.DoDeath(killedBy);
        }

        protected override void DoInteraction(IClient iClient, IItemStack item)
        {
            base.DoInteraction(iClient, item);

            Client client = iClient as Client;
            if (item != null && !item.IsVoid())
            {
                if ((item.Type == (short)BlockData.Items.Pork || item.Type == (short)BlockData.Items.Grilled_Pork))
                {
                    client.Owner.Inventory.RemoveItem(item.Slot); // consume the item
                    
                    if (this.Data.IsTamed)
                    {
                        // Feed a tame wolf pork chop
                        if (this.Health < this.MaxHealth &&
                            (item.Type == (short)BlockData.Items.Pork || item.Type == (short)BlockData.Items.Grilled_Pork))
                        {
                            if (this.Health < this.MaxHealth)
                            {
                                this.Health += 3; // Health is clamped, no need to check if exceeds MaxHealth
                                SendMetadataUpdate();
                            }
                        }
                    }
                }
                else if (!this.Data.IsTamed && item.Type == (short)BlockData.Items.Bone)
                {
                    // Give a bone
                    this.BonesUntilTamed--;
                    client.Owner.Inventory.RemoveItem(item.Slot); // consume the item

                    if (this.BonesUntilTamed <= 0)
                    {
                        this.Data.IsTamed = true;
                        this.Data.TamedBy = client.Username;
                        this.Health = this.MaxHealth;
                        // TODO: begin following this.Data.TamedBy
                        SendMetadataUpdate();
                    }
                }
            }
        }
    }
}
