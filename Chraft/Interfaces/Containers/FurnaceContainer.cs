using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Chraft.Interfaces.Recipes;
using Chraft.Net.Packets;
using Chraft.World;
using Chraft.World.Blocks;

namespace Chraft.Interfaces.Containers
{
    public class FurnaceContainer : PersistentContainer
    {
        protected const short FullFire = 250;
        protected const short FullProgress = 185;

        public volatile bool IsBurning;

        private short _fuelTicksLeft;
        private short _fuelTicksFull;
        private short _progressTicks;
        private Timer _burnerTimer;

       private ItemStack InputSlot
        {
            get { return this[(int)FurnaceInterface.FurnaceSlots.Input]; }
            set { ChangeSlot(-1, (short)FurnaceInterface.FurnaceSlots.Input, value); }
        }

        private ItemStack FuelSlot
        {
            get { return this[(int)FurnaceInterface.FurnaceSlots.Fuel]; }
            set { ChangeSlot(-1, (short)FurnaceInterface.FurnaceSlots.Fuel, value); }
        }

        private ItemStack OutputSlot
        {
            get { return this[(int)FurnaceInterface.FurnaceSlots.Output]; }
            set { ChangeSlot(-1, (short)FurnaceInterface.FurnaceSlots.Output, value); }
        }

        public FurnaceContainer()
        {
            SlotsCount = 3;
        }

        public override void ChangeSlot(sbyte senderWindowId, short slot, ItemStack newItem)
        {
            base.ChangeSlot(senderWindowId, slot, newItem);
            StartBurning();
        }

        private SmeltingRecipe GetSmeltingRecipe(ItemStack item)
        {
            SmeltingRecipe recipe = null;
            if (!ItemStack.IsVoid(item))
                recipe = SmeltingRecipe.GetRecipe(Server.GetSmeltingRecipes(), item);
            return recipe;
        }

        public void SendFurnaceProgressPacket(short progressLevel)
        {
            FurnaceInterface fi;
            foreach (var furnaceInterface in Interfaces)
            {
                fi = (FurnaceInterface)furnaceInterface;
                if (fi == null)
                    continue;
                fi.SendUpdateProgressBar(FurnaceBar.Progress, progressLevel);
            }
        }

        public void SendFurnaceFirePacket(short fireLevel)
        {
            FurnaceInterface fi;
            foreach (var furnaceInterface in Interfaces)
            {
                fi = (FurnaceInterface)furnaceInterface;
                if (fi == null)
                    continue;
                fi.SendUpdateProgressBar(FurnaceBar.Fire, fireLevel);
            }
        }

        public override bool IsUnused()
        {
            if (IsBurning)
                return false;
            return base.IsUnused();
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

        public override void Destroy()
        {
            StopBurning();
            base.Destroy();
        }
        public void StartBurning()
        {
            lock (_containerLock)
            {
                if (!IsBurning && HasFuel() && HasIngredient())
                {
                    if (_burnerTimer == null)
                        _burnerTimer = new Timer(Burn, null, 0, 50);
                }
                else if (!IsBurning && _burnerTimer == null)
                {
                    Chunk chunk = World.GetChunk(Coords, false, false);
                    if (chunk == null)
                        return;
                    if (chunk.GetType(Coords) == BlockData.Blocks.Burning_Furnace)
                        chunk.SetType(Coords, BlockData.Blocks.Furnace);
                }
            }
        }

        private short GetFuelEfficiency()
        {
            if (ItemStack.IsVoid(FuelSlot))
                return 0;

            return (FuelSlot.Type < 256 ? BlockHelper.Instance((byte)FuelSlot.Type).BurnEfficiency : BlockData.ItemBurnEfficiency[(BlockData.Items)FuelSlot.Type]);
        }

        private void RemoveFuel()
        {
            if (FuelSlot.Count > 1)
                FuelSlot = new ItemStack(FuelSlot.Type, --FuelSlot.Count, FuelSlot.Durability);
            else
                FuelSlot = ItemStack.Void;
        }

        private void RemoveIngredient()
        {
            if (InputSlot.Count > 1)
                InputSlot = new ItemStack(InputSlot.Type, --InputSlot.Count, InputSlot.Durability);
            else
                InputSlot = ItemStack.Void;
        }

        private void AddOutput()
        {
            if (!ItemStack.IsVoid(OutputSlot))
            {
                OutputSlot = new ItemStack(OutputSlot.Type, ++OutputSlot.Count, OutputSlot.Durability);
                return;
            }
            ItemStack output = GetSmeltingRecipe(InputSlot).Result;
            output.Count = 1;
            OutputSlot = output;
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
            SendFurnaceProgressPacket(0);
            SendFurnaceFirePacket(0);

            Chunk chunk = World.GetChunk(Coords, false, false);

            if (chunk == null)
                return;

            chunk.SetType(Coords, BlockData.Blocks.Furnace);
        }

        private void Burn(object state)
        {
            lock (_containerLock)
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
                            chunk.SetType(Coords, BlockData.Blocks.Burning_Furnace);

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
                        AddOutput();
                        RemoveIngredient();
                    }
                    _progressTicks++;
                }

                double fuelTickCost = ((double)(_fuelTicksFull)) / FullFire;
                short fireLevel = (short)(_fuelTicksLeft / fuelTickCost);

                if (fireLevel % 10 == 0 || fireLevel == FullFire)
                    SendFurnaceFirePacket(fireLevel);

                if (_progressTicks % 10 == 0 || _progressTicks == FullProgress)
                    SendFurnaceProgressPacket(_progressTicks);
            }
        }

        public enum FurnaceBar : short
        {
            Progress = 0,
            Fire = 1
        }
    }
}
