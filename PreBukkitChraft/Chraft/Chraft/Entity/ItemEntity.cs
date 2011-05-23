using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Entity
{
	public class ItemEntity : EntityBase
	{
		public short ItemId { get; set; }
		public sbyte Count { get; set; }
		public short Durability { get; set; }

		public ItemEntity(Server server, int entityId)
			: base(server, entityId)
		{
		}
	}
}
