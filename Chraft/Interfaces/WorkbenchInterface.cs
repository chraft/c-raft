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

		public override void Associate(Player player)
		{
			base.Associate(player);
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
            if (_useProvidedDropCoordinates)
            {
                base.DropAll(DropX, DropY, DropZ);
            }
            else
            {
                base.DropAll((int)Owner.Position.X, (int)Owner.Position.Y, (int)Owner.Position.Z);
            }
        }
	}
}
