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

using Chraft.Entity.Items.Base;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.Entity.Items
{
    class ItemWoodenDoor : ItemPlaceable
    {
        public ItemWoodenDoor()
        {
            Type = (short)BlockData.Items.Wooden_Door;
            Name = "WoodenDoor";
            Durability = 0;
            Damage = 1;
            Count = 1;
            IsStackable = false;
            MaxStackSize = 1;
        }

        public override void Place(PluginSystem.World.Blocks.IStructBlock baseBlock, BlockFace face)
        {
            switch (baseBlock.Type)
            {
                case (byte)BlockData.Blocks.Air:
                case (byte)BlockData.Blocks.Water:
                case (byte)BlockData.Blocks.Lava:
                case (byte)BlockData.Blocks.Still_Water:
                case (byte)BlockData.Blocks.Still_Lava:
                    return;
            }

            var player = Owner.GetPlayer() as Player;
            byte bType = (byte)BlockData.Blocks.Wooden_Door;
            byte bMetaData = (byte)player.Inventory.ActiveItem.Durability;

            var coordsFromFace = UniversalCoords.FromFace(baseBlock.Coords, face);
            var bBlock = new StructBlock(coordsFromFace, bType, bMetaData, player.World);

            BlockHelper.Instance.CreateBlockInstance(bType).Place(player, bBlock, baseBlock, face);
        }
    }
}
