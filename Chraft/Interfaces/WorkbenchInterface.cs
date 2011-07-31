using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Interfaces.Recipes;
using Chraft.Net;
using Chraft.Net.Packets;

namespace Chraft.Interfaces
{
	public class WorkbenchInterface : CraftingInterface
	{
		public WorkbenchInterface()
			: base(InterfaceType.Workbench, 9, 10)
		{
		}

		public override void Associate(Client client)
		{
			base.Associate(client);
		}

        bool _useProvidedDropCoordinates = false;
        int DropX;
        int DropY;
        int DropZ;

        /// <summary>
        /// Opens the Workbench and specifies where items should be dropped if exiting the workbench with items still in it.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public virtual void Open(int x, int y, int z)
        {
            _useProvidedDropCoordinates = true;
            DropX = x;
            DropY = y;
            DropZ = z;

            this.Open();
        }

        protected override void DoClose()
        {
            base.DoClose();

            // Drop all items from the workbench
            for (short i = 0; i < SlotCount; i++)
            {
                ItemStack stack = Slots[i];
                if (!ItemStack.IsVoid(stack))
                {
                    if (_useProvidedDropCoordinates)
                    {
                        this.Client.Server.DropItem(this.Client.World, DropX, DropY, DropZ, stack);
                    }
                    else
                    {
                        this.Client.Server.DropItem(this.Client, stack);
                    }
                    Slots[i] = ItemStack.Void;
                }
            }
        }
	}
}
