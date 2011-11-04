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
using Chraft.Properties;
using System.IO;
using System.Threading;
using Chraft.Net;

namespace Chraft.Interfaces
{
    /// <summary>
    /// A container interface that persists within the World (e.g. Small Chest, Dispenser, Large Chest)
    /// </summary>
    /// <remarks>
    /// The storage mechanism used here is a temporary measure until the data is correctly stored within Chunks instead
    /// </remarks>
    public abstract class PersistentContainerInterface: Interface
    {
        private object _savingLock = new object();
        private static volatile bool _saving = false;

        protected String DataPath { get { return Path.Combine(this.World.Folder, Settings.Default.ContainersFolder); } }

        protected World.WorldManager World { get; private set; }

        internal PersistentContainerInterface(World.WorldManager world, InterfaceType interfaceType, sbyte slotCount)
            : base(interfaceType, slotCount)
        {
            this.World = world;

            EnsureDirectory();
        }

        static bool directoryInitialised = false;
        private void EnsureDirectory()
        {
            if (!directoryInitialised)
            {
                if (!Directory.Exists(DataPath))
                {
                    Directory.CreateDirectory(DataPath);
                }
                directoryInitialised = true;
            }
        }

        protected override void DoClose()
        {
            base.DoClose();

            Save();
        }

        protected virtual bool CanLoad()
        {
            return Settings.Default.LoadFromSave;
        }

        protected virtual void DoLoadFromFile(ItemStack[] itemStack, string file)
        {
            if (File.Exists(file))
            {
                using (FileStream containerStream = File.Open(file, FileMode.Open, FileAccess.Read))
                {
                    using (BigEndianStream bigEndian = new BigEndianStream(containerStream, StreamRole.Server))
                    {
                        for (int i = 0; i < itemStack.Length; i++)
                        {
                            itemStack[i] = new ItemStack(bigEndian);
                        }
                    }
                }
            }
        }

        protected abstract void DoLoad();
        
        protected void Load()
        {
            if (!CanLoad())
                return;

            Monitor.Enter(_savingLock);
            try
            {
                DoLoad();
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

        protected virtual void DoSaveToFile(ItemStack[] itemStack, string file)
        {
            if (this.IsEmpty())
            {
                File.Delete(file);
            }
            else
            {
                try
                {
                    using (FileStream fileStream = File.Create(file + ".tmp"))
                    {
                        using (BigEndianStream bigEndianStream = new BigEndianStream(fileStream, StreamRole.Server))
                        {
                            foreach (ItemStack stack in itemStack)
                            {
                                if (stack != null)
                                {
                                    stack.Write(bigEndianStream);
                                }
                                else
                                {
                                    ItemStack.Void.Write(bigEndianStream);
                                }
                            }
                        }

                    }
                }
                finally
                {
                    File.Delete(file);
                    File.Move(file + ".tmp", file);
                }
            }
        }

        protected abstract void DoSave();

        public void Save()
        {
            if (!EnterSave())
                return;

            try
            {
                DoSave();
            }
            catch
            {
                // TODO: some form of logging
            }
            finally
            {
                ExitSave();
            }
        }
    }
}
