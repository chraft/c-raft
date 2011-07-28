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

        protected override void DoClose()
        {
            base.DoClose();

            // Drop all items from the workbench
            for (short i = 0; i < SlotCount; i++)
            {
                ItemStack stack = Slots[i];
                if (!ItemStack.IsVoid(stack))
                {
                    this.Client.Server.DropItem(this.Client, stack);
                    Slots[i] = ItemStack.Void;
                }
            }
        }
	}
}
