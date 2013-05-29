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
using Chraft.Interfaces;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;
using Chraft.Utilities.Misc;
using Chraft.Utils;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Sheep: Animal
    {
        public override string Name
        {
            get { return "Sheep"; }
        }

        public override short MaxHealth { get { return 10; } }

        static WeightedPercentValue<WoolColor>[] _woolColor = new[]{
                WeightedPercentValue.Create(0.8184, WoolColor.White), // 81.84% chance for White
                WeightedPercentValue.Create(0.05, WoolColor.Silver),  // 5% chance for light gray
                WeightedPercentValue.Create(0.05, WoolColor.Gray),    // 5% chance for gray
                WeightedPercentValue.Create(0.05, WoolColor.Black),   // 5% chance for black
                WeightedPercentValue.Create(0.03, WoolColor.Brown),   // 3% chance for brown
                WeightedPercentValue.Create(0.0016, WoolColor.Pink),  // 0.16% chance for pink
            };

        internal Sheep(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Sheep, data)
        {
            Data.Sheared = false;
            Data.WoolColor = _woolColor.SelectRandom(world.Server.Rand);
        }

        protected WoolColor DyeColorToWoolColor(MetaData.Dyes dyeColor)
        {
            switch (dyeColor)
            {
                case MetaData.Dyes.InkSack:
                    return WoolColor.Black;
                case MetaData.Dyes.LapisLazuli:
                    return WoolColor.Blue;
                case MetaData.Dyes.CocoBeans:
                    return WoolColor.Brown;
                case MetaData.Dyes.CactusGreen:
                    return WoolColor.Green;
                case MetaData.Dyes.Cyan:
                    return WoolColor.Cyan;
                case MetaData.Dyes.Gray:
                    return WoolColor.Gray;
                case MetaData.Dyes.LightBlue:
                    return WoolColor.LightBlue;
                case MetaData.Dyes.Lime:
                    return WoolColor.Lime;
                case MetaData.Dyes.Magenta:
                    return WoolColor.Magenta;
                case MetaData.Dyes.Orange:
                    return WoolColor.Orange;
                case MetaData.Dyes.Pink:
                    return WoolColor.Pink;
                case MetaData.Dyes.Purple:
                    return WoolColor.Purple;
                case MetaData.Dyes.RoseRed:
                    return WoolColor.Red;
                case MetaData.Dyes.LightGray:
                    return WoolColor.Silver;
                case MetaData.Dyes.BoneMeal:
                    return WoolColor.White;
                case MetaData.Dyes.DandelionYellow:
                    return WoolColor.Yellow;
            }
            return WoolColor.White;
        }

        protected override void DoInteraction(IClient client, IItemInventory item)
        {
            base.DoInteraction(client, item);

            if (client != null && item != null && !ItemHelper.IsVoid(item))
            {
                if (item.Type == (short)BlockData.Items.Shears && !Data.Sheared)
                {
                    // Drop wool when sheared
                    sbyte count = (sbyte)Server.Rand.Next(2, 4);

                    if (count > 0)
                    {
                        var drop = ItemHelper.GetInstance(BlockData.Blocks.Wool);
                        drop.Count = count;
                        drop.Durability = (short)Data.WoolColor;
                        Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), drop);
                    }
                    Data.Sheared = true;

                    SendMetadataUpdate();
                }
                else if (item.Type == (short)BlockData.Items.Ink_Sack)
                {
                    // Set the wool colour of this Sheep based on the item.Durability
                    // Safety check. Values of 16 and higher (color do not exist) may crash the client v1.8.1 and below
                    if (item.Durability >= 0 && item.Durability <= 15)
                    {
                        //this.Data.WoolColor = (WoolColor)Enum.ToObject(typeof(WoolColor), (15 - item.Durability));
                        Data.WoolColor = DyeColorToWoolColor((MetaData.Dyes)Enum.ToObject(typeof(MetaData.Dyes), item.Durability));
                        SendMetadataUpdate();
                    }
                }
            }
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            if (!Data.Sheared)
            {
                var item = ItemHelper.GetInstance(BlockData.Blocks.Wool);
                item.Count = 1;
                item.Durability = (short)Data.WoolColor;
                Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), item);
            }
            base.DoDeath(killedBy);
        }
    }
}
