using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.World;

namespace Chraft
{
	public class ItemEntity : BaseEntity
	{
		public short ItemId { get; set; }
		public byte Count { get; set; }
		public short Durability { get; set; }

		public ItemEntity(int entityId)
			: base(entityId)
		{
		}
	}
}
