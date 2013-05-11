using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity.Items;
using Chraft.Net;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Misc;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Ocelot : Animal
    {
        public override string Name
        {
            get { return "Ocelot"; }
        }

        public override short MaxHealth { get { return 10; } }

        internal Ocelot(WorldManager world, int entityId, MobType type, MetaData data)
            : base(world, entityId, type, data)
        {
            Data.IsSitting = false;
            Data.IsTamed = false;
            Data.IsAggressive = false;
            FishUntilTamed = Server.Rand.Next(20);
        }

        protected virtual int FishUntilTamed { get; set; }

        protected override void DoInteraction(IClient iClient, IItemInventory item)
        {
            base.DoInteraction(iClient, item);

            Client client = iClient as Client;
            if (client == null)
                return;

            if (item != null && !ItemHelper.IsVoid(item))
            {
                if (item.Type == (short)BlockData.Items.Raw_Fish && !Data.IsTamed)
                {
                    FishUntilTamed--;
                    client.Owner.Inventory.RemoveItem(item.Slot); // consume the item

                    if (FishUntilTamed <= 0)
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
