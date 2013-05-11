using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.Net;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;
using Chraft.Utilities.Misc;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Mooshroom : Animal
    {
        public override string Name
        {
            get { return "Mooshroom"; }
        }

        public override short MaxHealth { get { return 10; } }

        internal Mooshroom(WorldManager world, int entityId, MobType type, MetaData data)
            : base(world, entityId, type, data)
        {
            Data.Sheared = false;
        }

        protected override void DoDeath(EntityBase killedBy)
        {

            UniversalCoords coords = UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z);
            sbyte count = (sbyte)Server.Rand.Next(2);
            ItemInventory item;
            if (count > 0)
            {
                item = ItemHelper.GetInstance(BlockData.Items.Leather);
                item.Count = count;
                Server.DropItem(World, coords, item);
            }
            count = (sbyte)Server.Rand.Next(1, 3);
            if (count > 0)
            {
                item = ItemHelper.GetInstance(BlockData.Items.RawBeef);
                item.Count = count;
                Server.DropItem(World, coords, item);
            }
            
            base.DoDeath(killedBy);
        }

        protected override void DoInteraction(IClient client, IItemInventory item)
        {
            base.DoInteraction(client, item);

            if (client != null && item != null && !ItemHelper.IsVoid(item))
            {
                if (item.Type == (short)BlockData.Items.Shears && !Data.Sheared)
                {
                    // Drop Red mushroom when sheared
                    sbyte count = 5;

                    var drop = ItemHelper.GetInstance(BlockData.Blocks.Red_Mushroom);
                    drop.Count = count;
                    Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), drop);

                    Data.Sheared = true;

                    SendMetadataUpdate();
                }
                else if (item.Type == (short)BlockData.Items.Bowl)
                {
                    short slot = (short)item.Slot;
                    client.GetOwner().GetInventory().RemoveItem(slot);
                    client.GetOwner().GetInventory().AddItem((short)BlockData.Items.Mushroom_Soup, 1, 0);
                }
                else if (item.Type == (short)BlockData.Items.Bucket)
                {
                    short slot = (short)item.Slot;
                    client.GetOwner().GetInventory().RemoveItem(slot);
                    client.GetOwner().GetInventory().AddItem((short)BlockData.Items.Milk_Bucket, 1, 0);
                }
            }
        }
    }
}
