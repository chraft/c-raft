using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net.Packets;
using Chraft.World;
using Chraft.Interfaces.Recipes;
using System.Threading;
using Chraft.World.Blocks;

namespace Chraft.Interfaces
{
    /// <summary>
    /// TODO: this is a work in progress for working furnaces - there is currently no timer, the output just appears immediately, and multi-client support is not implemented
    /// </summary>
    public class FurnaceInterface : SingleContainerInterface
    {
        class FurnaceInstance
        {
            protected const short FullFire = 250;
            protected const short FullProgress = 185;

            List<FurnaceInterface> Interfaces = new List<FurnaceInterface>();
            object _instanceLock = new object();

            public string Name
            {
                get
                {
                    return String.Format("{0}-{1},{2},{3}", this.World.Name, Coords.WorldX, Coords.WorldY, Coords.WorldZ);
                }
            }

            public UniversalCoords Coords { get; private set; }
            public WorldManager World { get; private set; }
            public volatile bool IsBurning;

            protected ItemStack FuelSlot;
            protected ItemStack InputSlot;
            protected ItemStack OutputSlot;

            private short _fuelTicksLeft;
            private short _fuelTicksFull;
            private short _progressTicks;
            private Timer _burnerTimer;

            public FurnaceInstance(WorldManager world, UniversalCoords coords)
            {
                this.World = world;
                Coords = coords;
            }

            public void InitSlots(ItemStack input, ItemStack fuel, ItemStack output)
            {
                lock (_instanceLock)
                {
                    InputSlot = (ItemStack.IsVoid(input) ? ItemStack.Void : new ItemStack(input.Type, input.Count, input.Durability));
                    FuelSlot = (ItemStack.IsVoid(fuel) ? ItemStack.Void : new ItemStack(fuel.Type, fuel.Count, fuel.Durability));
                    OutputSlot = (ItemStack.IsVoid(output) ? ItemStack.Void : new ItemStack(output.Type, output.Count, output.Durability));
                }
            }

            public void ChangeFuel(ItemStack newItem)
            {
                lock (_instanceLock)
                {
                    if (ItemStack.IsVoid(newItem))
                        FuelSlot = ItemStack.Void;
                    else
                    {
                        FuelSlot.Type = newItem.Type;
                        FuelSlot.Count = newItem.Count;
                        FuelSlot.Durability = newItem.Durability;
                    }
                    foreach (var fInterface in Interfaces)
                    {
                        fInterface[(short)FurnaceSlots.Fuel] = FuelSlot;
                        fInterface.SendUpdate((short)FurnaceSlots.Fuel);
                    }
                }
                TryBurn();
            }

            public void ChangeInput(ItemStack newItem)
            {
                lock (_instanceLock)
                {
                    if (ItemStack.IsVoid(newItem))
                        InputSlot = ItemStack.Void;
                    else
                    {
                        InputSlot.Type = newItem.Type;
                        InputSlot.Count = newItem.Count;
                        InputSlot.Durability = newItem.Durability;
                    }
                    foreach (var fInterface in Interfaces)
                    {
                        fInterface[(short)FurnaceSlots.Input] = InputSlot;
                        fInterface.SendUpdate((short)FurnaceSlots.Input);
                    }
                }
                TryBurn();
            }

            public void ChangeOutput(ItemStack newItem)
            {
                lock (_instanceLock)
                {
                    if (ItemStack.IsVoid(newItem))
                        OutputSlot = ItemStack.Void;
                    else
                    {
                        OutputSlot.Type = newItem.Type;
                        OutputSlot.Count = newItem.Count;
                        OutputSlot.Durability = newItem.Durability;
                    }
                    foreach (var fInterface in Interfaces)
                    {
                        fInterface[(short)FurnaceSlots.Output] = OutputSlot;
                        fInterface.SendUpdate((short)FurnaceSlots.Output);
                    }   
                }
                TryBurn();
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

            public bool HasInterfaces()
            {
                return Interfaces.Count > 0;
            }

            private SmeltingRecipe GetSmeltingRecipe(ItemStack item)
            {
                SmeltingRecipe recipe = null;
                if (!ItemStack.IsVoid(item))
                    recipe = SmeltingRecipe.GetRecipe(Server.GetSmeltingRecipes(), item);
                return recipe;
            }

            public void SendFurnaceProgressPacket(short progressLevel = 0)
            {
                foreach (var furnaceInterface in Interfaces)
                {
                    furnaceInterface.Owner.Client.SendPacket(new UpdateProgressBarPacket
                    {
                        WindowId = furnaceInterface.Handle,
                        ProgressBar = 0,
                        Value = progressLevel,
                    });
                }
            }

            public void SendFurnaceFirePacket(short fireLevel = FullFire)
            {
                foreach (var furnaceInterface in Interfaces)
                {
                    furnaceInterface.Owner.Client.SendPacket(new UpdateProgressBarPacket
                    {
                        WindowId = furnaceInterface.Handle,
                        ProgressBar = 1,
                        Value = fireLevel,
                    });
                }
            }

            public bool IsUnused()
            {
                return (ItemStack.IsVoid(InputSlot) && ItemStack.IsVoid(FuelSlot) && ItemStack.IsVoid(OutputSlot) && !IsBurning);
            }

            private bool HasFuel()
            {
                if (ItemStack.IsVoid(FuelSlot))
                    return false;
                return ((FuelSlot.Type < 256 && BlockHelper.Instance((byte)FuelSlot.Type).IsIgnitable) ||
                        (FuelSlot.Type >= 256 && BlockData.ItemBurnEfficiency.ContainsKey((BlockData.Items)FuelSlot.Type)));
            }

            private bool HasIngredient()
            {
                if (ItemStack.IsVoid(InputSlot))
                    return false;
                SmeltingRecipe recipe = GetSmeltingRecipe(InputSlot);
                return (recipe != null);
            }

            public void TryBurn()
            {
                lock (_instanceLock)
                {
                    if (!IsBurning && HasFuel() && HasIngredient())
                    {
                        if (_burnerTimer == null)
                            _burnerTimer = new Timer(Burn, null, 0, 50);
                    }
                }
            }

            private void RemoveFuel()
            {
                FuelSlot.Count--;
                if (FuelSlot.Count == 0)
                    FuelSlot = ItemStack.Void;
                foreach (var fInterface in Interfaces)
                {
                    fInterface[(short)FurnaceSlots.Fuel] = new ItemStack(FuelSlot.Type, FuelSlot.Count, FuelSlot.Durability);
                    fInterface.SendUpdate((short)FurnaceSlots.Fuel);
                }
            }

            private short GetFuelEfficiency()
            {
                if (ItemStack.IsVoid(FuelSlot))
                    return 0;

                return (FuelSlot.Type < 256 ? BlockHelper.Instance((byte)FuelSlot.Type).BurnEfficiency : BlockData.ItemBurnEfficiency[(BlockData.Items)FuelSlot.Type]);
            }

            public void StopBurning()
            {
                if (_burnerTimer != null)
                {
                    _burnerTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _burnerTimer.Dispose();
                    _burnerTimer = null;
                }
                IsBurning = false;
                _progressTicks = 0;
                _fuelTicksLeft = 0;
                _fuelTicksFull = 0;
                SendFurnaceProgressPacket();
                SendFurnaceFirePacket(0);

                Chunk chunk = World.GetChunk(Coords, false, false);

                if (chunk == null)
                    return;

                byte blockId = chunk.GetData(Coords);
                chunk.SetBlockAndData(Coords, (byte)BlockData.Blocks.Furnace, blockId);
            }

            private void Burn(object state)
            {
                lock (_instanceLock)
                {
                    Chunk chunk = World.GetChunk(Coords, false, false);

                    if (chunk == null)
                    {
                        StopBurning();
                        return;
                    }

                    if (_fuelTicksLeft <= 0)
                    {
                        if (HasIngredient() && HasFuel())
                        {
                            if (!ItemStack.IsVoid(OutputSlot) && !GetSmeltingRecipe(InputSlot).Result.StacksWith(OutputSlot))
                            {
                                StopBurning();
                                return;
                            }
                            _fuelTicksFull = GetFuelEfficiency();
                            _fuelTicksLeft = _fuelTicksFull;

                            SendFurnaceProgressPacket(_progressTicks);
                            RemoveFuel();

                            BlockData.Blocks blockId = chunk.GetType(Coords);

                            if (blockId == BlockData.Blocks.Furnace)
                                chunk.SetBlockAndData(Coords, (byte)BlockData.Blocks.Burning_Furnace, (byte)blockId);
                            
                            IsBurning = true;
                        }
                        else
                            StopBurning();
                        
                        return;
                    }

                    
                    _fuelTicksLeft--;
                    if (ItemStack.IsVoid(InputSlot) || (!ItemStack.IsVoid(OutputSlot) && (!GetSmeltingRecipe(InputSlot).Result.StacksWith(OutputSlot) || OutputSlot.Count == 64)))                   
                        _progressTicks = 0;                   
                    else
                    {
                        if (_progressTicks >= FullProgress)
                        {
                            _progressTicks = 0;
                            ItemStack output = GetSmeltingRecipe(InputSlot).Result;
                            output.Count = 1;
                            if (!ItemStack.IsVoid(OutputSlot))
                                output.Count = (sbyte)(OutputSlot.Count + 1);
                            ChangeOutput(output);
                            if (InputSlot.Count < 2)
                                ChangeInput(ItemStack.Void);
                            else
                                ChangeInput(new ItemStack(InputSlot.Type, --InputSlot.Count, InputSlot.Durability));
                        }
                        _progressTicks++;
                    }
                    
                    double fuelTickCost = ((double)(_fuelTicksFull))/FullFire;
                    short fireLevel = (short)(_fuelTicksLeft/fuelTickCost);

                    if (fireLevel % 10 == 0 || fireLevel == FullFire)
                        SendFurnaceFirePacket(fireLevel);

                    if (_progressTicks % 10 == 0 || _progressTicks == FullProgress)
                        SendFurnaceProgressPacket(_progressTicks);
                }
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
        private static FurnaceInstance AddFurnaceInterface(UniversalCoords coords, FurnaceInterface furnace)
        {
            string id = String.Format("{0}-{1},{2},{3}", furnace.World.Name, coords.WorldX, coords.WorldY, coords.WorldZ);
            lock (_staticLock)
            {
                FurnaceInstance furnaceInstance;
                if (!_furnaceInstances.ContainsKey(id))
                {
                    furnaceInstance = new FurnaceInstance(furnace.World, coords);
                    furnaceInstance.InitSlots(furnace[(short)FurnaceSlots.Input], furnace[(short)FurnaceSlots.Fuel], furnace[(short)FurnaceSlots.Output]);
                    furnaceInstance.TryBurn();
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

        private static void RemoveFurnaceInterface(UniversalCoords coords, FurnaceInterface furnace)
        {
            string id = String.Format("{0}-{1},{2},{3}", furnace.World.Name, coords.WorldX, coords.WorldY, coords.WorldZ);
            lock (_staticLock)
            {
                if (_furnaceInstances.ContainsKey(id))
                {
                    _furnaceInstances[id].Remove(furnace);
                    if (!_furnaceInstances[id].HasInterfaces() && _furnaceInstances[id].IsUnused())
                        _furnaceInstances.Remove(id);
                }
            }
        }
        #endregion

        public FurnaceInterface(World.WorldManager world, UniversalCoords coords)
            : base(world, InterfaceType.Furnace, coords, 3)
		{
            
		}

        private FurnaceInstance _furnaceInstance;

        public static void StopBurning(WorldManager world, UniversalCoords coords)
        {
            string id = String.Format("{0}-{1},{2},{3}", world.Name, coords.WorldX, coords.WorldY, coords.WorldZ);
            lock (_staticLock)
            {
                if (_furnaceInstances.ContainsKey(id))
                    _furnaceInstances[id].StopBurning();
            }
        }

        protected override void DoOpen()
        {
            base.DoOpen();
            this._furnaceInstance = AddFurnaceInterface(Coords, this);
        }

        protected override void DoClose()
        {
            RemoveFurnaceInterface(Coords, this);
            base.DoClose();
        }

        internal override void OnClicked(WindowClickPacket packet)
        {
            ItemStack newItem = ItemStack.Void;

            if (packet.Slot == (short)FurnaceSlots.Output)
            {
                lock (_furnaceInstance)
                {
                    if (!ItemStack.IsVoid(Cursor))
                    {
                        Owner.Client.SendPacket(new TransactionPacket
                        {
                            Accepted = false,
                            Transaction = packet.Transaction,
                            WindowId = packet.WindowId
                        });
                        return;
                        //if (!Cursor.StacksWith(this[OUTPUT_SLOT]) || Cursor.Count + this[OUTPUT_SLOT].Count > 64)
                        //{}
                    }
                    else
                    {
                        Cursor = ItemStack.Void;
                        Cursor.Slot = -1;
                        Cursor.Type = this[(short)FurnaceSlots.Output].Type;
                        Cursor.Durability = this[(short)FurnaceSlots.Output].Durability;
                        // Add the output item to the Cursor
                        Cursor.Count += this[(short)FurnaceSlots.Output].Count;
                        this[(short)FurnaceSlots.Output] = ItemStack.Void;
                        _furnaceInstance.ChangeOutput(ItemStack.Void);
                    }
                    return;
                }

            }

            base.OnClicked(packet);

            if (packet.Slot == (short)FurnaceSlots.Input)
            {
                lock (_furnaceInstance)
                {
                    if (!ItemStack.IsVoid(this[(short)FurnaceSlots.Input]))
                    {
                        newItem.Type = this[(short)FurnaceSlots.Input].Type;
                        newItem.Count = this[(short)FurnaceSlots.Input].Count;
                        newItem.Durability = this[(short)FurnaceSlots.Input].Durability;
                    }
                    _furnaceInstance.ChangeInput(newItem);
                }
            }
            else if (packet.Slot == (short)FurnaceSlots.Fuel)
            {
                lock (_furnaceInstance)
                {
                    if (!ItemStack.IsVoid(this[(short)FurnaceSlots.Fuel]))
                    {
                        newItem.Type = this[(short)FurnaceSlots.Fuel].Type;
                        newItem.Count = this[(short)FurnaceSlots.Fuel].Count;
                        newItem.Durability = this[(short)FurnaceSlots.Fuel].Durability;
                    }
                    _furnaceInstance.ChangeFuel(newItem);
                }
            }
        }

        public enum FurnaceSlots : short
        {
            Input = 0,
            Fuel = 1,
            Output = 2,
            InventoryFirst = 3,
            InventoryLast = 29,
            QuickSlotFirst = 30,
            QuickSlotLast = 38
        }
	}
}
