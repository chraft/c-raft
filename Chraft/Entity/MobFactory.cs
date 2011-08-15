using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity.Mobs;

namespace Chraft.Entity
{
    /// <summary>
    /// Factory for creating mobs
    /// </summary>
    public static class MobFactory
    {
        /// <summary>
        /// Create a new Mob object based on MobType and provided data
        /// </summary>
        /// <param name="world"></param>
        /// <param name="entityId"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Mob CreateMob(Chraft.World.WorldManager world, int entityId, MobType type, Chraft.Net.MetaData data = null)
        {
            // TODO: extensibility point to allow plugin to return custom class as Mobs instead of the built-in classes
            Mob mob = null;

            // If a custom Mob has not been created, return a built-in Mob
            if (mob == null)
            {
                switch (type)
                {
                    case MobType.Cow:
                        mob = new Cow(world, entityId, data);
                        break;
                    case MobType.Creeper:
                        mob = new Creeper(world, entityId, data);
                        break;
                    case MobType.Ghast:
                        mob = new Ghast(world, entityId, data);
                        break;
                    case MobType.Giant:
                        mob = new GiantZombie(world, entityId, data);
                        break;
                    case MobType.Hen:
                        mob = new Hen(world, entityId, data);
                        break;
                    case MobType.Pig:
                        mob = new Pig(world, entityId, data);
                        break;
                    case MobType.PigZombie:
                        mob = new ZombiePigman(world, entityId, data);
                        break;
                    case MobType.Sheep:
                        mob = new Sheep(world, entityId, data);
                        break;
                    case MobType.Skeleton:
                        mob = new Skeleton(world, entityId, data);
                        break;
                    case MobType.Slime:
                        mob = new Slime(world, entityId, data);
                        break;
                    case MobType.Spider:
                        mob = new Spider(world, entityId, data);
                        break;
                    case MobType.Squid:
                        mob = new Squid(world, entityId, data);
                        break;
                    case MobType.Wolf:
                        mob = new Wolf(world, entityId, data);
                        break;
                    case MobType.Zombie:
                        mob = new Zombie(world, entityId, data);
                        break;
                    default:
                        mob = null;
                        break;
                }
            }

            return mob;
        }
    }
}
