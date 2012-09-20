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

using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.Entity.Items.Base
{
    public abstract class ItemBaseHoe : ItemUsable
    {
        public override void Use(IStructBlock baseBlock, BlockFace face)
        {
            if (baseBlock.Type == (byte)BlockData.Blocks.Dirt || baseBlock.Type == (byte)BlockData.Blocks.Grass)
            {
                var soilBlock = (StructBlock)baseBlock;
                soilBlock.Type = (byte)BlockData.Blocks.Soil;
                // Think the client has a Notch bug where hoe's durability is not updated properly.
                BlockHelper.Instance.CreateBlockInstance(soilBlock.Type).Spawn(soilBlock);
            }
        }
    }
}
