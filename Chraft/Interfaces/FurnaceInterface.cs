using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net.Packets;
using Chraft.World;
using Chraft.Interfaces.Recipes;
using System.Threading;

namespace Chraft.Interfaces
{
    /// <summary>
    /// TODO: this is a work in progress for working furnaces - there is currently no timer, the output just appears immediately, and multi-client support is not implemented
    /// </summary>
    public class FurnaceInterface : SingleContainerInterface
    {
        class FurnaceInstance
        {
            List<FurnaceInterface> Interfaces = new List<FurnaceInterface>();
            object _instanceLock = new object();

            public string Name
            {
                get
                {
                    return String.Format("{0}-{1},{2},{3}", this.World.Name, X, Y, Z);
                }
            }
            public int X { get; private set; }
            public int Y { get; private set; }
            public int Z { get; private set; }
            public WorldManager World { get; private set; }
            public volatile bool IsBurning;
            public volatile bool IsSmelting;

            public FurnaceInstance(WorldManager world, int x, int y, int z)
            {
                this.World = world;
                X = x;
                Y = y;
                Z = z;
            }

            public void Add(FurnaceInterface furnaceInterface)
            {
                lock (_instanceLock)
                {
                    if (!Interfaces.Contains(furnaceInterface))
                    {
                        Interfaces.Add(furnaceInterface);
                    }
                }
            }

            public void Remove(FurnaceInterface furnaceInterface)
            {
                lock (_instanceLock)
                {
                    Interfaces.Remove(furnaceInterface);
                }
            }

            public void SendBurnerProgressPacket()
            {
                foreach (var furnaceInterface in this.Interfaces)
                {
                    furnaceInterface.PacketHandler.SendPacket(new UpdateProgressBarPacket
                    {
                        WindowId = furnaceInterface.Handle,
                        ProgressBar = 1,
                        Value = 0,
                    });
                }
                return;
            }
        }

        #region Global tracking of Furnaces
        private static Dictionary<string, FurnaceInstance> _furnaceInstances = new Dictionary<string, FurnaceInstance>();
        private static object _staticLock = new object();

        /// <summary>
        /// Adds the FurnaceInterface to a FurnaceInstance, then returns the FurnaceInstance it is added to.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="furnace"></param>
        /// <returns>The FurnaceInstance that the FurnaceInterface was added to</returns>
        private static FurnaceInstance AddFurnaceInterface(int x, int y, int z, FurnaceInterface furnace)
        {
            string id = String.Format("{0}-{1},{2},{3}", furnace.World.Name, x, y, z);
            lock (_staticLock)
            {
                FurnaceInstance furnaceInstance;
                if (!_furnaceInstances.ContainsKey(id))
                {
                    furnaceInstance = new FurnaceInstance(furnace.World, x, y, z);
                    _furnaceInstances[id] = furnaceInstance;
                }
                else
                {
                    furnaceInstance = _furnaceInstances[id];
                }
                furnaceInstance.Add(furnace);

                return furnaceInstance;
            }
        }

        private static void RemoveFurnaceInterface(int x, int y, int z, FurnaceInterface furnace)
        {
            string id = String.Format("{0}-{1},{2},{3}", furnace.World.Name, x, y, z);
            lock (_staticLock)
            {
                if (_furnaceInstances.ContainsKey(id))
                {
                    _furnaceInstances[id].Remove(furnace);
                }
            }
        }
        #endregion

        public FurnaceInterface(World.WorldManager world, int x, int y, int z)
            : base(world, InterfaceType.Furnace, x, y, z, 3)
		{
            
		}

        private FurnaceInstance _furnaceInstance;
        protected override void DoOpen()
        {
            base.DoOpen();
            this._furnaceInstance = AddFurnaceInterface(this.X, this.Y, this.Z, this);
        }

        protected override void DoClose()
        {
            RemoveFurnaceInterface(this.X, this.Y, this.Z, this);
            _furnaceInstance = null;
            base.DoClose();
        }

        const short INPUT_SLOT = 0;
        const short FUEL_SLOT = 1;
        const short OUTPUT_SLOT = 2;

        internal bool HasFuel()
        {
            return !ItemStack.IsVoid(this[FUEL_SLOT]);
        }

        internal bool HasIngredient()
        {
            return !ItemStack.IsVoid(this[INPUT_SLOT]);
        }

        private SmeltingRecipe GetSmeltingRecipe(ItemStack item)
        {
            SmeltingRecipe recipe = null;
            if (!ItemStack.IsVoid(item))
                recipe = SmeltingRecipe.GetRecipe(Server.GetSmeltingRecipes(), item);
            return recipe;
        }


        internal override void OnClicked(Net.Packets.WindowClickPacket packet)
        {
            if (packet.Slot == FUEL_SLOT)
            {
                if (!ItemStack.IsVoid(this.Cursor))
                {
                    // Only allow operations against valid fuel blocks/items
                    if ((this.Cursor.Type < 256 && !BlockData.BlockBurnEfficiency.ContainsKey((BlockData.Blocks)this.Cursor.Type)) ||
                        (this.Cursor.Type >= 256 && !BlockData.ItemBurnEfficiency.ContainsKey((BlockData.Items)this.Cursor.Type)))
                    {
                        PacketHandler.SendPacket(new TransactionPacket
                        {
                            Accepted = false,
                            Transaction = packet.Transaction,
                            WindowId = packet.WindowId
                        });
                        return;
                    }
                }
            }
            else if (packet.Slot == INPUT_SLOT)
            {
                if (!ItemStack.IsVoid(this.Cursor))
                {
                    // Only allow operations against valid smelting ingredients
                    SmeltingRecipe recipe = GetSmeltingRecipe(this.Cursor);

                    if (recipe == null)
                    {
                        PacketHandler.SendPacket(new TransactionPacket
                        {
                            Accepted = false,
                            Transaction = packet.Transaction,
                            WindowId = packet.WindowId
                        });
                        return;
                    }
                }
            }
            else if (packet.Slot == OUTPUT_SLOT)
            {
                lock (this._furnaceInstance)
                {
                    if (ItemStack.IsVoid(this[OUTPUT_SLOT]))
                    {
                        PacketHandler.SendPacket(new TransactionPacket
                            {
                                Accepted = false,
                                Transaction = packet.Transaction,
                                WindowId = packet.WindowId
                            });
                        return;
                    }
                    else
                    {
                        if (!ItemStack.IsVoid(Cursor))
                        {
                            if (!Cursor.StacksWith(this[OUTPUT_SLOT]) || Cursor.Count + this[OUTPUT_SLOT].Count > 64)
                            {
                                PacketHandler.SendPacket(new TransactionPacket
                                {
                                    Accepted = false,
                                    Transaction = packet.Transaction,
                                    WindowId = packet.WindowId
                                });
                                return;
                            }
                        }
                        else
                        {
                            this.Cursor = ItemStack.Void;
                            this.Cursor.Slot = -1;
                            this.Cursor.Type = this[OUTPUT_SLOT].Type;
                            this.Cursor.Durability = this[OUTPUT_SLOT].Durability;
                        }

                        // Add the output item to the Cursor
                        this.Cursor.Count += this[OUTPUT_SLOT].Count;
                        this[OUTPUT_SLOT] = ItemStack.Void;
                    }
                }
                
                return;
            }

            // Base performs any adding/removing input / fuel.
            base.OnClicked(packet);

            // Check if we need to start the burner / smelting process
            if (packet.Slot >= INPUT_SLOT && packet.Slot <= OUTPUT_SLOT)
            {
                //0	 above flame
                //1	 fuel
                //2	 output

                if (!this._furnaceInstance.IsBurning)
                { // Not burning

                    if (HasIngredient() && HasFuel())
                    {
                        StartBurner();
                        StartSmelting();
                    }
                }
                else if (!this._furnaceInstance.IsSmelting)
                { // Burning but not cooking

                    if (HasIngredient())
                    {
                        StartSmelting();
                    }
                }
            }
        }

        private volatile uint _burnerStartTick;
        private volatile short _burnForTicks;
        private void StartBurner()
        {
            lock (_furnaceInstance)
            {
                if (this[FUEL_SLOT].Type < 256)
                    _burnForTicks = BlockData.BlockBurnEfficiency[(BlockData.Blocks)this[FUEL_SLOT].Type];
                else
                    _burnForTicks = BlockData.ItemBurnEfficiency[(BlockData.Items)this[FUEL_SLOT].Type];

                // Set block to burning furnace
                this.World.SetBlockAndData(this.X, this.Y, this.Z, (byte)BlockData.Blocks.Burning_Furnace, this.World.GetBlockData(this.X, this.Y, this.Z));
                _burnerStartTick = this.World.WorldTicks;
                _furnaceInstance.IsBurning = true;

                // Consume a fuel item
                this[FUEL_SLOT].Count--;
                if (this[FUEL_SLOT].Count == 0)
                    this[FUEL_SLOT] = ItemStack.Void;

                new Thread(BurnerThread).Start();
            }
        }

        private void BurnerThread(object state)
        {

        }

        private void StartSmelting()
        {
            lock (_furnaceInstance)
            {
                SmeltingRecipe recipe = GetSmeltingRecipe(this[INPUT_SLOT]);

                // TODO: preliminary work has the result being copied to OUTPUT_SLOT immediately instead of after 10secs of burn time
                if (recipe != null && (ItemStack.IsVoid(this[OUTPUT_SLOT]) || (this[OUTPUT_SLOT].StacksWith(recipe.Result) && this[OUTPUT_SLOT].Count < 64)))
                {
                    this[INPUT_SLOT].Count--;
                    if (this[INPUT_SLOT].Count == 0)
                        this[INPUT_SLOT] = ItemStack.Void;

                    if (ItemStack.IsVoid(this[OUTPUT_SLOT]))
                    {
                        ItemStack newItem = ItemStack.Void;
                        newItem.Type = recipe.Result.Type;
                        newItem.Durability = recipe.Result.Durability;
                        newItem.Count = 1;
                        this[OUTPUT_SLOT] = newItem;
                    }
                    else
                    {
                        this[OUTPUT_SLOT].Count++;
                    }
                }
            }
        }
	}
}
