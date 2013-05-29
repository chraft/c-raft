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

using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.Entity.Items.Base
{
    public abstract class ItemPlaceable : ItemInventory, IItemPlaceable
    {
        protected virtual bool CanBePlacedOn(IStructBlock baseBlock, BlockFace face)
        {
            switch (baseBlock.Type)
            {
                case (byte)BlockData.Blocks.Air:
                case (byte)BlockData.Blocks.Water:
                case (byte)BlockData.Blocks.Lava:
                case (byte)BlockData.Blocks.Still_Water:
                case (byte)BlockData.Blocks.Still_Lava:
                    return false;
            }
            return true;
        }

        protected virtual byte GetBlockToPlace(IStructBlock baseBlock, BlockFace face)
        {
            return (byte)Type;
        }

        public virtual void Place(IStructBlock baseBlock, BlockFace face)
        {
            if (!CanBePlacedOn(baseBlock, face))
                return;

            var player = Owner.GetPlayer() as Player;
            byte bType = GetBlockToPlace(baseBlock, face);
            byte bMetaData = (byte)Durability;

            var coordsFromFace = UniversalCoords.FromFace(baseBlock.Coords, face);
            var bBlock = new StructBlock(coordsFromFace, bType, bMetaData, player.World);

            BlockHelper.Instance.CreateBlockInstance(bType).Place(player, bBlock, baseBlock, face);
            //Owner.GetPlayer().GetInventory().RemoveItem(Slot);
        }
    }
}
