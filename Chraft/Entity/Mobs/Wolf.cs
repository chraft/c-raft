using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.World;
using Chraft.Interfaces;

namespace Chraft.Entity.Mobs
{
    public class Wolf : Mob
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
        }

        protected override void DoInteraction(Client client, Chraft.Interfaces.ItemStack item)
        {
            base.DoInteraction(client, item);

            if (!ItemStack.IsVoid(item))
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
                        this.Data.TamedBy = client.Owner.Username;
                        this.Health = this.MaxHealth;
                        // TODO: begin following this.Data.TamedBy
                        SendMetadataUpdate();
                    }
                }
            }
        }
    }
}
