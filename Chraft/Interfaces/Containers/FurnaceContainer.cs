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
using System.Threading;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.Interfaces.Recipes;
using Chraft.PluginSystem.Item;
using Chraft.Utilities.Blocks;
using Chraft.World;
using Chraft.World.Blocks;

namespace Chraft.Interfaces.Containers
{
    public class FurnaceContainer : PersistentContainer
    {
        protected const short FullFire = 250;
        protected const short FullProgress = 185;

        public bool IsBurning { get { return _burnerTimer != null; }}

        private short _fuelTicksLeft;
        private short _fuelTicksFull;
        private short _progressTicks;
        private Timer _burnerTimer;

        private ItemInventory InputSlot
        {
            get { return this[(int)FurnaceInterface.FurnaceSlots.Input]; }
            set { ChangeSlot(-1, (short)FurnaceInterface.FurnaceSlots.Input, value); }
        }

        private ItemInventory FuelSlot
        {
            get { return this[(int)FurnaceInterface.FurnaceSlots.Fuel]; }
            set { ChangeSlot(-1, (short)FurnaceInterface.FurnaceSlots.Fuel, value); }
        }

        private ItemInventory OutputSlot
        {
            get { return this[(int)FurnaceInterface.FurnaceSlots.Output]; }
            set { ChangeSlot(-1, (short)FurnaceInterface.FurnaceSlots.Output, value); }
        }

        public FurnaceContainer()
        {
            SlotsCount = 3;
        }

        public override void ChangeSlot(sbyte senderWindowId, short slot, ItemInventory newItem)
        {
            base.ChangeSlot(senderWindowId, slot, newItem);
            StartBurning();
        }

        private SmeltingRecipe GetSmeltingRecipe(ItemInventory item)
        {
            SmeltingRecipe recipe = null;
            if (item != null && !ItemHelper.IsVoid(item))
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
            if (ItemHelper.IsVoid(FuelSlot))
                return false;
            return ((FuelSlot.Type < 256 && BlockHelper.Instance.IsIgnitable((byte)FuelSlot.Type)) ||
                    (FuelSlot.Type >= 256 && FuelSlot is IItemFuel));
        }

        private bool HasIngredient()
        {
            if (ItemHelper.IsVoid(InputSlot))
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
                if (!IsBurning)
                {
                    if (_fuelTicksLeft > 0)
                    {
                        Chunk chunk = World.GetChunk(Coords, false, false) as Chunk;
                        if (chunk == null)
                            return;
                        chunk.SetType(Coords, BlockData.Blocks.Burning_Furnace);
                    }
                    if ((HasFuel() && HasIngredient()) || _fuelTicksLeft > 0)
                        StartBurnerTimer();
                }
            }
        }

        protected void StartBurnerTimer()
        {
            if (_burnerTimer == null)
                _burnerTimer = new Timer(Burn, null, 0, 50);
        }

        protected void StopBurnerTimer()
        {
            if (_burnerTimer != null)
            {
                _burnerTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _burnerTimer.Dispose();
                _burnerTimer = null;
            }
        }

        public void StopBurning()
        {
            StopBurnerTimer();
            _progressTicks = 0;
            _fuelTicksLeft = 0;
            _fuelTicksFull = 0;
            Save();
            SendFurnaceProgressPacket(0);
            SendFurnaceFirePacket(0);

            Chunk chunk = World.GetChunk(Coords) as Chunk;

            if (chunk == null)
                return;

            chunk.SetType(Coords, BlockData.Blocks.Furnace);
        }

        private short GetFuelEfficiency()
        {
            if (ItemHelper.IsVoid(FuelSlot))
                return 0;
            var fuelItem = FuelSlot as IItemFuel;
            return (FuelSlot.Type < 256 ? BlockHelper.Instance.BurnEfficiency((byte)FuelSlot.Type) : (fuelItem == null ? (short)0 : fuelItem.BurnEfficiency));
        }

        private void RemoveFuel()
        {
            if (FuelSlot.Count > 1)
                //FuelSlot = new ItemStack(FuelSlot.Type, --FuelSlot.Count, FuelSlot.Durability);
                FuelSlot.Count--;
            else
                FuelSlot = ItemHelper.Void;
        }

        private void RemoveIngredient()
        {
            if (InputSlot.Count > 1)
                //InputSlot = new ItemStack(InputSlot.Type, --InputSlot.Count, InputSlot.Durability);
                InputSlot.Count--;
            else
                InputSlot = ItemHelper.Void;
        }

        private void AddOutput()
        {
            if (!ItemHelper.IsVoid(OutputSlot))
            {
                //OutputSlot = new ItemStack(OutputSlot.Type, ++OutputSlot.Count, OutputSlot.Durability);
                OutputSlot.Count++;
                return;
            }
            ItemInventory output = GetSmeltingRecipe(InputSlot).Result;
            output.Count = 1;
            OutputSlot = output;
        }

        private void Burn(object state)
        {
            lock (_containerLock)
            {
                Chunk chunk = World.GetChunk(Coords) as Chunk;

                if (chunk == null)
                {
                    StopBurning();
                    return;
                }

                if (_fuelTicksLeft <= 0)
                {
                    if (HasIngredient() && HasFuel())
                    {
                        if (!ItemHelper.IsVoid(OutputSlot) && !GetSmeltingRecipe(InputSlot).Result.StacksWith(OutputSlot))
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
                    }
                    else
                        StopBurning();

                    return;
                }

                _fuelTicksLeft--;
                if (ItemHelper.IsVoid(InputSlot) || (!ItemHelper.IsVoid(OutputSlot) && (!GetSmeltingRecipe(InputSlot).Result.StacksWith(OutputSlot) || OutputSlot.Count == 64)))
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

        public override void AddInterface(PersistentContainerInterface containerInterface)
        {
            base.AddInterface(containerInterface);
            FurnaceInterface fi = containerInterface as FurnaceInterface;
            if (fi == null)
                return;
            if (IsBurning)
            {
                double fuelTickCost = ((double) (_fuelTicksFull))/FullFire;
                short fireLevel = (short) (_fuelTicksLeft/fuelTickCost);
                fi.SendUpdateProgressBar(FurnaceBar.Progress, _progressTicks);
                fi.SendUpdateProgressBar(FurnaceBar.Fire, fireLevel);
            }
        }

        protected override void SaveExtraData(Net.BigEndianStream stream)
        {
            base.SaveExtraData(stream);
            stream.Write(_progressTicks);
            stream.Write(_fuelTicksLeft);
            stream.Write(_fuelTicksFull);
        }

        protected override void LoadExtraData(Net.BigEndianStream stream)
        {
            base.LoadExtraData(stream);
            _progressTicks = stream.ReadShort();
            _fuelTicksLeft = stream.ReadShort();
            _fuelTicksFull = stream.ReadShort();
        }

        public void Unload()
        {
            StopBurnerTimer();
            Save();
        }

        public enum FurnaceBar : short
        {
            Progress = 0,
            Fire = 1
        }
    }
}
