using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Interfaces.Recipes;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.World;

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

	    private UniversalCoords _DropCoords;

        /// <summary>
        /// Opens the Workbench and specifies where items should be dropped if exiting the workbench with items still in it.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public virtual void Open(UniversalCoords coords)
        {
            _useProvidedDropCoordinates = true;
            _DropCoords = coords;

            this.Open();
        }

        protected override void DoClose()
        {
            base.DoClose();

            // Drop all items from the workbench
            if (_useProvidedDropCoordinates)
            {
                base.DropAll(_DropCoords);
            }
            else
            {
                base.DropAll(UniversalCoords.FromWorld((int)Owner.Position.X, (int)Owner.Position.Y, (int)Owner.Position.Z));
            }
        }
	}
}
