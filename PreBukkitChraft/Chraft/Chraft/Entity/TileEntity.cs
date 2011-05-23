using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity
{
	public partial class TileEntity : EntityBase
	{
		public TileEntity(Server server, int entityId)
			: base(server, entityId)
		{
		}
	}
}
