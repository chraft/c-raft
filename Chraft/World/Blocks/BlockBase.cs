using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    public struct StructBlock
    {
        public byte Type;
        public int X;
        public int Y;
        public int Z;
        public byte MetaData;
        public Chunk Chunk;
        public WorldManager World;

        public StructBlock(int x, int y, int z, byte type, byte metaData, WorldManager world)
        {
            Type = type;
            X = x;
            Y = y;
            Z = z;
            MetaData = metaData;
            World = world;
            Chunk = World.GetBlockChunk(x, y, z);
        }
    }

    public abstract class BlockBase : IBlockBase
    {
        /// <summary>
        /// String representation of the block name
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Block type
        /// </summary>
        public BlockData.Blocks Type { get; set; }

        public bool IsAir { get; protected set; }
        public bool IsLiquid { get; protected set; }
        public bool IsSolid { get; protected set; }

        public byte Opacity { get; protected set; }
        public bool IsOpaque
        {
            get { return (Opacity == 0xf); }
        }

        /// <summary>
        /// Requires single hit to destroy
        /// </summary>
        public bool IsSingleHit { get; protected set; }

        /// <summary>
        /// Can the greens (but not the crops) grow on it
        /// </summary>
        public bool IsFertile { get; protected set; }

        /// <summary>
        /// Was the block plowed and made suitable for the crops
        /// </summary>
        public bool IsPlowed { get; protected set; }

        /// <summary>
        /// Can the block be burn
        /// </summary>
        public bool IsIgnitable
        {
            get { return (BurnEfficiency > 0); }
        }

        /// <summary>
        /// Block Flammability/Burn Efficiency measured in world ticks (x0.05secs). Value / 20 => number of seconds burn time. 10secs = 1 item smelted
        /// </summary>
        public short BurnEfficiency { get; protected set; }

        /// <summary>
        /// Light emitted by the block
        /// </summary>
        public byte Luminance { get; protected set; }

        /// <summary>
        /// Block that is being dropped on destruction
        /// </summary>
        public BlockData.Blocks DropBlock { get; protected set; }
        /// <summary>
        /// Amount of blocks that are dropped on destruction
        /// </summary>
        public sbyte DropBlockAmount { get; protected set; }

        /// <summary>
        /// Metadata of the block that is being dropped on destruction
        /// </summary>
        public short DropBlockMeta { get; protected set; }

        /// <summary>
        /// Item that is being dropped on destruction
        /// </summary>
        public BlockData.Items DropItem { get; protected set; }

        /// <summary>
        /// Amount of items that are dropped on destruction
        /// </summary>
        public sbyte DropItemAmount { get; protected set; }

        /// <summary>
        /// Metadata of the item that is being dropped on destruction
        /// </summary>
        public short DropItemMeta { get; protected set; }

        /// <summary>
        /// Base contructor
        /// </summary>
        protected BlockBase()
        {
            Name = "BaseBlock";
            Type = BlockData.Blocks.Air;
            IsAir = false;
            IsLiquid = false;
            Opacity = 0xf;
            IsSolid = false;
            IsSingleHit = false;
            IsFertile = false;
            IsPlowed = false;
            BurnEfficiency = 0;
            DropBlock = BlockData.Blocks.Air;
            DropBlockAmount = 0;
            DropBlockMeta = 0;
            DropItem = BlockData.Items.Pork;
            DropItemAmount = 0;
            DropItemMeta = 0;
            Luminance = 0;
        }

        public virtual void Destroy(StructBlock block)
        {
            Destroy(null, block);
        }


        /// <summary>
        /// Destroy the block and drop the loot (if any).
        /// </summary>
        public virtual void Destroy(EntityBase entity, StructBlock block)
        {
            if (!RaiseDestroyEvent(entity, block))
                return;

            UpdateOnDestroy(block);

            DropItems(entity, block);

            DamageItem(entity);
        }

        /// <summary>
        /// Method that is called when the player touches the block. For future use - pressure plates, proximity sensors etc.
        /// </summary>
        public virtual void Touch(EntityBase entity, StructBlock block) { }

        public virtual void Place(StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            Place(null, block, targetBlock, face);
        }
        /// <summary>
        /// Places the block
        /// </summary>
        public virtual void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            if (!CanBePlacedOn(entity, block, targetBlock, face))
                return;

            if (!RaisePlaceEvent(entity, block))
                return;

            UpdateOnPlace(block);
            RemoveItem(entity);
        }

        /// <summary>
        /// The BLOCK_DESTROY event invoker
        /// </summary>
        /// <returns>true if the block will be destroyed</returns>
        protected virtual bool RaiseDestroyEvent(EntityBase entity, StructBlock block)
        {
            BlockDestroyEventArgs e = new BlockDestroyEventArgs(this, entity);
            block.World.Server.PluginManager.CallEvent(Plugins.Events.Event.BLOCK_DESTROY, e);
            if (entity != null && e.EventCanceled) return false;
            return true;
        }

        /// <summary>
        /// The BLOCK_PLACE event invoker
        /// </summary>
        /// <returns>true if the block will be placed</returns>
        protected virtual bool RaisePlaceEvent(EntityBase entity, StructBlock block)
        {
            BlockPlaceEventArgs e = new BlockPlaceEventArgs(this, entity);
            block.World.Server.PluginManager.CallEvent(Plugins.Events.Event.BLOCK_PLACE, e);

            // Destruction made not by the living can not be interrupted?
            if (entity == null)
                return true;
            return !e.EventCanceled;
        }

        /// <summary>
        /// Updates world data upon block destruction
        /// </summary>
        protected virtual void UpdateOnDestroy(StructBlock block)
        {
            block.World.SetBlockAndData(block.X, block.Y, block.Z, (byte)BlockData.Blocks.Air, 0);
            block.World.Update(block.X, block.Y, block.Z);
        }

        /// <summary>
        /// Updates the world data upon block placement
        /// </summary>
        protected virtual void UpdateOnPlace(StructBlock block)
        {
            block.World.SetBlockAndData(block.X, block.Y, block.Z, block.Type, block.MetaData);
            block.World.Update(block.X, block.Y, block.Z, false);
        }


        protected virtual void DropItems(StructBlock block)
        {
            DropItems(null, block);
        }

        /// <summary>
        /// Invoked to drop the loot after block destruction
        /// </summary>
        /// <param name="entity">the entity who destroyed the block</param>
        protected virtual void DropItems(EntityBase entity, StructBlock block)
        {
            if (DropBlock != BlockData.Blocks.Air && DropBlockAmount > 0)
            {
                
                block.World.Server.DropItem(block.World, block.X, block.Y, block.Z, new ItemStack((short)DropBlock, DropBlockAmount, DropBlockMeta));
            }
            if (DropItemAmount > 0)
            {
                block.World.Server.DropItem(block.World, block.X, block.Y, block.Z, new ItemStack((short)DropItem, DropItemAmount, DropItemMeta));
            }
        }

        /// <summary>
        /// Removes the active item from inventory when block is placed
        /// </summary>
        /// <param name="entity">the entity who placed the block</param>
        protected virtual void RemoveItem(EntityBase entity)
        {
            Client client = entity as Client;
            if (client != null && client.GameMode == 0)
                client.Inventory.RemoveItem(client.Inventory.ActiveSlot);
        }

        /// <summary>
        /// Damages the active item in the inventory when the block is destroyed
        /// </summary>
        /// <param name="entity">the entity who destroyed the block</param>
        protected virtual void DamageItem(EntityBase entity)
        {
            Client client = entity as Client;
            if (client != null && client.GameMode == 0)
                client.Inventory.DamageItem(client.Inventory.ActiveSlot);
        }

        /// <summary>
        /// Checks if this block can be placed next to the target one
        /// </summary>
        /// <returns>true if can be placed, false otherwise</returns>
        protected virtual bool CanBePlacedOn(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            BlockBase tBlock = targetBlock.World.BlockHelper.Instance(targetBlock.Type);
            if (!tBlock.IsSolid)
                return false;

            byte originalBlock = block.World.GetBlockId(block.X, block.Y, block.Z);

            if ( originalBlock != (byte)BlockData.Blocks.Air &&
                originalBlock != (byte)BlockData.Blocks.Water &&
                originalBlock != (byte)BlockData.Blocks.Still_Water &&
                originalBlock != (byte)BlockData.Blocks.Lava &&
                originalBlock != (byte)BlockData.Blocks.Still_Lava)
                return false;
            return true;
        }


    }
}
