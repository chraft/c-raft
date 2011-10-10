using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utils;

namespace Chraft.World.Blocks.Physics
{
    public class FallingSand : BlockBasePhysics
    {
        protected byte BlockId;

        public FallingSand(WorldManager world, Location pos) : base(world, pos)
        {
            Type = Net.Packets.AddObjectVehiclePacket.ObjectType.FallingSand;
            BlockId = (byte) BlockData.Blocks.Sand;
            Velocity = new Vector3(0, -0.4D, 0);
        }

        public override void Simulate()
        {
            int x = MathHelper.floor_double(Position.X);
            int y = MathHelper.floor_double(Position.Y);
            int z = MathHelper.floor_double(Position.Z);
            byte blockId = World.GetBlockId(x, y, z);
            if (blockId != (byte)BlockData.Blocks.Air)
            {
                Stop();
                return;
            }

            if (Position.Y <= 1)
            {
                Stop();
                return;
            }

            Position.Vector += Velocity;
        }

        protected override void OnStop()
        {
            UniversalCoords uc = UniversalCoords.FromWorld(MathHelper.floor_double(Position.X), MathHelper.floor_double(Position.Y) + 1, MathHelper.floor_double(Position.Z));
            StructBlock block = new StructBlock(uc, BlockId, 0, World);
            BlockHelper.Instance(BlockId).Spawn(block);
            base.OnStop();
        }
    }
}
