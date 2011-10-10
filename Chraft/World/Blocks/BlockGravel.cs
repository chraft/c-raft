using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Physics;

namespace Chraft.World.Blocks
{
    class BlockGravel : BlockBase
    {
        public BlockGravel()
        {
            Name = "Gravel";
            Type = BlockData.Blocks.Gravel;
            IsSolid = true;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            Player player = entity as Player;
            if (player != null)
            {
                LootTable = new List<ItemStack>();
                if ((player.Inventory.ActiveItem.Type == (short)BlockData.Items.Wooden_Spade ||
                    player.Inventory.ActiveItem.Type == (short)BlockData.Items.Stone_Spade ||
                    player.Inventory.ActiveItem.Type == (short)BlockData.Items.Iron_Spade ||
                    player.Inventory.ActiveItem.Type == (short)BlockData.Items.Gold_Spade ||
                    player.Inventory.ActiveItem.Type == (short)BlockData.Items.Diamond_Spade) &&
                    block.World.Server.Rand.Next(10) == 0)
                {
                    LootTable.Add(new ItemStack((short)BlockData.Items.Flint, 1));
                }
                else
                {
                    LootTable.Add(new ItemStack((short)Type, 1));
                }
            }
            base.DropItems(entity, block);
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if ((targetBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                    targetBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                    targetBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
            {
                StartPhysics(targetBlock);
            }
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            if (!CanBePlacedOn(entity, block, targetBlock, face))
                return;

            if (!RaisePlaceEvent(entity, block))
                return;

            UpdateOnPlace(block);

            RemoveItem(entity);

            if (block.Coords.WorldY > 1)
                if (block.World.GetBlockId(block.Coords.WorldX, block.Coords.WorldY - 1, block.Coords.WorldZ) == (byte)BlockData.Blocks.Air)
                    StartPhysics(block);
        }

        protected void StartPhysics(StructBlock block)
        {
            Remove(block);
            FallingGravel fgBlock = new FallingGravel(block.World, new Location(block.Coords.WorldX + 0.5, block.Coords.WorldY + 0.5, block.Coords.WorldZ + 0.5));
            fgBlock.Start();
            block.World.PhysicsBlocks.TryAdd(fgBlock.EntityId, fgBlock);
        }
    }
}
