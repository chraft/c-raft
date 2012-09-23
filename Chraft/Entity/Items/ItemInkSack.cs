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
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.Entity.Items
{
    class ItemInkSack : ItemUsable
    {
        public ItemInkSack()
        {
            Type = (short)BlockData.Items.Ink_Sack;
            Name = "InkSack";
            IsStackable = true;
            MaxStackSize = 64;
        }

        public override void Use(IStructBlock baseBlock, BlockFace face)
        {
            var player = Owner.GetPlayer() as Player;
            var newBlockCoords = UniversalCoords.FromFace(baseBlock.Coords, face);

            if (Durability != 15)
                return;

            if (baseBlock.Type == (byte)BlockData.Blocks.Red_Mushroom || baseBlock.Type == (byte)BlockData.Blocks.Brown_Mushroom)
            {
                var baseMushroom = (BlockBaseMushroom)BlockHelper.Instance.CreateBlockInstance(baseBlock.Type);
                baseMushroom.Fertilize(player, (StructBlock)baseBlock);
            }

            base.Use(baseBlock, face);
        }
    }
}
