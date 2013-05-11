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
using System.Linq;
using System.Text;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Enderman : Monster
    {
        public override string Name
        {
            get { return "Enderman"; }
        }

        public override short MaxHealth { get { return 40; } } // 20 hearts

        public override short AttackStrength
        {
            get
            {
                return 10; // 5 hearts
            }
        }

        internal Enderman(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Blaze, data)
        {
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            var killedByMob = killedBy as Mob;
            UniversalCoords coords = UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z);
            sbyte count = (sbyte)Server.Rand.Next(2);
            if (count > 0)
            {
                ItemInventory itm = ItemHelper.GetInstance(BlockData.Items.EnderPearl);
                itm.Count = count;
                Server.DropItem(World, coords, itm);
            }
            base.DoDeath(killedBy);
        }
    }
}