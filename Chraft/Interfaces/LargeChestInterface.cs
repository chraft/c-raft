using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Chraft.World;

namespace Chraft.Interfaces
{
	public class LargeChestInterface : PersistentContainerInterface
	{
        protected String NEDataFile { get { return Path.Combine(DataPath, String.Format("x{0}y{1}z{2}.dat", NEChest.WorldX, NEChest.WorldY, NEChest.WorldZ)); } }
        protected String SWDataFile { get { return Path.Combine(DataPath, String.Format("x{0}y{1}z{2}.dat", SWChest.WorldX, SWChest.WorldY, SWChest.WorldZ)); } }

        protected UniversalCoords NEChest { get; private set; }
        protected UniversalCoords SWChest { get; private set; }

        /// <summary>
        /// Creates a Large Chest interface for the two chests specified (North or East chest, and South or West chest)
        /// </summary>
        /// <param name="world"></param>
        /// <param name="neChest">The North or East chest coordinates</param>
        /// <param name="swChest">The South or West chest coordinates</param>
        public LargeChestInterface(World.WorldManager world, UniversalCoords neChest, UniversalCoords swChest)
            : base(world, InterfaceType.Chest, 54)
		{
            NEChest = neChest;
            SWChest = swChest;

            Load();
		}

        protected override void DoLoad()
        {
            ItemStack[] neChestItems = new ItemStack[27];
            ItemStack[] swChestItems = new ItemStack[27];

            // try..finally just to be sure we load both files - exception will be handled by caller in base
            try
            {
                DoLoadFromFile(neChestItems, NEDataFile);
            }
            finally
            {
                DoLoadFromFile(swChestItems, SWDataFile);
            }

            Slots = neChestItems.Concat(swChestItems).ToArray();
        }

        protected override void DoSave()
        {
            // try..finally just to be sure we load both files - exception will be handled by caller in base
            try
            {
                DoSaveToFile(Slots.Take(27).ToArray(), NEDataFile);
            }
            finally
            {
                DoSaveToFile(Slots.Skip(27).ToArray(), SWDataFile);
            }
        }

        public enum LargeChestSlots : short
        {
            ChestFirst = 0,
            ChestLast = 53,
            InventoryFirst = 54,
            InventoryLast = 80,
            QuickSlotFirst = 81,
            QuickSlotLast = 89
        }
	}
}
