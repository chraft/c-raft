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
    public class Witch : Monster
    {
        public override string Name
        {
            get { return "Witch"; }
        }

        public override short MaxHealth { get { return 26; } } // 13 hearts

        public override BehaviourType bt { get { return BehaviourType.Hostile; } }

        public override short AttackStrength
        {
            get
            {
                return 9; // 4.5 hearts
            }
        }

        internal Witch(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Witch, data)
        {
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            /*
             * 0–6  Glass Bottle - 1
             * 0–6  Glowstone Dust - 2
             * 0–6  Gunpowder - 3
             * 0–6  Redstone - 4
             * 0–6  Spider Eye - 5
             * 0–6  Stick - 6
             * 0–6  Sugar - 7
             */

            var killedByMob = killedBy as Mob;
            UniversalCoords coords = UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z);
            sbyte count = (sbyte)Server.Rand.Next(7);
            sbyte a;
            switch (count)
            {
                case 1:
                    a = (sbyte)Server.Rand.Next(6);
                    if (a > 0)
                    {
                        var item = ItemHelper.GetInstance(BlockData.Items.GlassBottle);
                        item.Count = count;
                        Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), item);
                    }
                    break;
                case 2:
                    a = (sbyte)Server.Rand.Next(6);
                    if (a > 0)
                    {
                        var item = ItemHelper.GetInstance(BlockData.Items.Lightstone_Dust);
                        item.Count = count;
                        Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), item);
                    }
                    break;
                case 3:
                    a = (sbyte)Server.Rand.Next(6);
                    if (a > 0)
                    {
                        var item = ItemHelper.GetInstance(BlockData.Items.Gunpowder);
                        item.Count = count;
                        Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), item);
                    }
                    break;
                case 4:
                    a = (sbyte)Server.Rand.Next(6);
                    if (a > 0)
                    {
                        var item = ItemHelper.GetInstance(BlockData.Items.Redstone);
                        item.Count = count;
                        Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), item);
                    }
                    break;
                case 5:
                    a = (sbyte)Server.Rand.Next(6);
                    if (a > 0)
                    {
                        var item = ItemHelper.GetInstance(BlockData.Items.SpiderEye);
                        item.Count = count;
                        Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), item);
                    }
                    break;
                case 6:
                    a = (sbyte)Server.Rand.Next(6);
                    if (a > 0)
                    {
                        var item = ItemHelper.GetInstance(BlockData.Items.Stick);
                        item.Count = count;
                        Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), item);
                    }
                    break;
                case 7:
                    a = (sbyte)Server.Rand.Next(6);
                    if (a > 0)
                    {
                        var item = ItemHelper.GetInstance(BlockData.Items.Sugar);
                        item.Count = count;
                        Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), item);
                    }
                    break;
            }
            base.DoDeath(killedBy);
        }
    }
}