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
using System.Collections.Generic;
using Chraft.Entity;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.World.Blocks.Base;
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
            var item = ItemHelper.GetInstance(Type);
            item.Count = 1;
            LootTable.Add(item);
        }

        protected override void DropItems(EntityBase entity, StructBlock block, List<ItemInventory> overridedLoot = null)
        {
            var player = entity as Player;
            if (player != null)
            {
                if ((player.Inventory.ActiveItem is ItemWoodenShovel ||
                    player.Inventory.ActiveItem is ItemStoneShovel ||
                    player.Inventory.ActiveItem is ItemIronShovel ||
                    player.Inventory.ActiveItem is ItemGoldShovel ||
                    player.Inventory.ActiveItem is ItemDiamondShovel) &&
                    block.World.Server.Rand.Next(10) == 0)
                {
                    overridedLoot = new List<ItemInventory>(1);
                    ItemInventory item = ItemHelper.GetInstance((short) BlockData.Items.Flint);
                    item.Count = 1;
                    overridedLoot.Add(item);
                    base.DropItems(entity, block, overridedLoot);
                    return;
                }
            }
            base.DropItems(entity, block);
        }

        protected override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if ((targetBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                    targetBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                    targetBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
                StartPhysics(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }

        protected override void UpdateWorld(StructBlock block, bool isDestroyed = false)
        {
            base.UpdateWorld(block, isDestroyed);
            if (!isDestroyed && block.Coords.WorldY > 1)
                if (block.World.GetBlockId(block.Coords.WorldX, block.Coords.WorldY - 1, block.Coords.WorldZ) == (byte)BlockData.Blocks.Air)
                    StartPhysics(block);
        }

        protected void StartPhysics(StructBlock block)
        {
            var world = block.World;
            Remove(block);
            FallingGravel fgBlock = new FallingGravel(world, new AbsWorldCoords(block.Coords.WorldX + 0.5, block.Coords.WorldY + 0.5, block.Coords.WorldZ + 0.5));
            fgBlock.Start();
            world.PhysicsBlocks.TryAdd(fgBlock.EntityId, fgBlock);
        }
    }
}
