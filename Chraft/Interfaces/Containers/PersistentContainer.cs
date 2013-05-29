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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.Net;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Config;
using Chraft.World;

namespace Chraft.Interfaces.Containers
{
    public abstract class PersistentContainer
    {
        protected string DataPath { get { return Path.Combine(World.Folder, ChraftConfig.ContainersFolder); } }
        protected string ContainerFolder;
        protected string DataFile;

        protected object _savingLock = new object();
        protected volatile bool _saving;

        protected object _containerLock = new object();

        public WorldManager World;
        public UniversalCoords Coords;
        protected virtual ItemInventory[] Slots { get; set; }
        public short SlotsCount;
        public virtual ItemInventory this[int slot]
        {
            get
            {
                return Slots[slot];
            }
            protected set
            {
                lock (_containerLock)
                {
                    Slots[slot] = value ?? ItemHelper.Void;
                }
            }
        }

        public List<PersistentContainerInterface> Interfaces = new List<PersistentContainerInterface>();

        public virtual void Initialize(WorldManager world, UniversalCoords coords)
        {
            World = world;
            Coords = coords;
            Slots = new ItemInventory[SlotsCount];
            DataFile = string.Format("x{0}y{1}z{2}.dat", Coords.WorldX, Coords.WorldY, Coords.WorldZ);
            string chunkFolder = string.Format("x{0}z{1}", Coords.ChunkX, Coords.ChunkZ);
            ContainerFolder = Path.Combine(DataPath, chunkFolder);
            if (!Directory.Exists(ContainerFolder))
            {
                Directory.CreateDirectory(ContainerFolder);
            }
            Load();
        }

        public virtual bool IsEmpty
        {
            get
            {
                bool empty = true;
                foreach (var item in Slots)
                {
                    if (item != null && !ItemHelper.IsVoid(item))
                    {
                        empty = false;
                        break;
                    }
                }
                return empty;
            }
        }

        #region Save and load

        protected virtual void DoLoad(int slotStart, int slotsCount, string dataFile)
        {
            string file = Path.Combine(ContainerFolder, dataFile);
            if (File.Exists(file))
            {
                using (FileStream containerStream = File.Open(file, FileMode.Open, FileAccess.Read))
                {
                    using (BigEndianStream bigEndian = new BigEndianStream(containerStream, StreamRole.Server))
                    {
                        for (int i = slotStart; i < slotsCount; i++)
                            //Slots[i] = new ItemStack(bigEndian);
                            Slots[i] = ItemHelper.GetInstance(bigEndian);
                        LoadExtraData(bigEndian);
                    }
                }
            }
        }

        protected void Load()
        {
            Monitor.Enter(_savingLock);
            try
            {
                DoLoad(0, SlotsCount, DataFile);
                return;
            }
            catch (Exception ex)
            {
                World.Logger.Log(ex);
                return;
            }
            finally
            {
                Monitor.Exit(_savingLock);
            }
        }

        private bool EnterSave()
        {
            lock (_savingLock)
            {
                if (_saving)
                    return false;
                _saving = true;
                return true;
            }
        }

        private void ExitSave()
        {
            _saving = false;
        }

        public void Save()
        {
            if (!EnterSave())
                return;

            try
            {
                DoSave(0, SlotsCount, DataFile);
            }
            catch(Exception ex)
            {
                World.Logger.Log(ex);
            }
            finally
            {
                ExitSave();
            }
        }

        protected virtual void DoSave(int slotStart, int slotsCount, string dataFile)
        {
            string file = Path.Combine(ContainerFolder, dataFile);
            if (IsEmpty)
            {
                File.Delete(file);
                return;
            }
            try
            {
                using (FileStream fileStream = File.Create(file + ".tmp"))
                {
                    using (BigEndianStream bigEndianStream = new BigEndianStream(fileStream, StreamRole.Server))
                    {
                        ItemInventory stack = ItemHelper.Void;
                        for (int i = slotStart; i < slotsCount; i++)
                        {
                            stack = Slots[i];
                            if (stack != null)
                            {
                                stack.Write(bigEndianStream);
                            }
                            else
                            {
                                ItemHelper.Void.Write(bigEndianStream);
                            }
                        }
                        SaveExtraData(bigEndianStream);
                    }

                }
            }
            finally
            {
                File.Delete(file);
                File.Move(file + ".tmp", file);
            }
         }

        protected virtual void LoadExtraData(BigEndianStream stream)
        { }

        protected virtual void SaveExtraData(BigEndianStream stream)
        { }
        #endregion

        #region Interface management
        public virtual void AddInterface(PersistentContainerInterface containerInterface)
        {
            lock (_containerLock)
            {
                if (!Interfaces.Contains(containerInterface))
                {
                    containerInterface.Container = this;
                    Interfaces.Add(containerInterface);
                }
            }
        }

        public virtual void RemoveInterface(PersistentContainerInterface containerInterface)
        {
            lock (_containerLock)
            {
                Save();
                Interfaces.Remove(containerInterface);
            }
        }

        public virtual bool IsUnused()
        {
            return !HasInterfaces();
        }

        public bool HasInterfaces()
        {
            return Interfaces.Count > 0;
        }
        #endregion

        public virtual bool SlotCanBeChanged(Net.Packets.WindowClickPacket packet)
        {
            return true;
        }

        public virtual void ChangeSlot(sbyte senderWindowId, short slot, ItemInventory newItem)
        {
            Slots[slot] = newItem;
            foreach (var persistentInterface in Interfaces)
                if (persistentInterface.Handle != senderWindowId)
                    persistentInterface[slot] = newItem;
            Save();
        }

        public virtual void Destroy()
        {
            foreach (var persistentInterface in Interfaces)
                persistentInterface.Close(true);
            DropContent();
        }

        public virtual void DropContent()
        {
            lock (_containerLock)
            {
                for (short i = 0; i < Slots.Count(); i++)
                {
                    ItemInventory stack = Slots[i];
                    if (stack != null && !ItemHelper.IsVoid(stack))
                    {
                        World.Server.DropItem(World, Coords, stack);
                        this[i] = ItemHelper.Void;
                    }
                }
            Save();
            }
        }
    }
}
