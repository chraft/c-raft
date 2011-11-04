using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity.Mobs;
using System.Reflection;
using Chraft.Net;

namespace Chraft.Entity
{

    /// <summary>
    /// Factory for creating mobs
    /// </summary>
    public static class MobFactory
    {
        public static Type GetMobClass(Chraft.World.WorldManager world, MobType type)
        {
            // TODO: extension point to allow plugin to override class for MobType for world
            Type mobType = null;

            // If a custom Mob has not been created, return a built-in Mob
            if (mobType == null)
            {
                switch (type)
                {
                    case MobType.Cow:
                        mobType = typeof(Cow);
                        break;
                    case MobType.Creeper:
                        mobType = typeof(Creeper);
                        break;
                    case MobType.Ghast:
                        mobType = typeof(Ghast);
                        break;
                    case MobType.Giant:
                        mobType = typeof(GiantZombie);
                        break;
                    case MobType.Hen:
                        mobType = typeof(Hen);
                        break;
                    case MobType.Pig:
                        mobType = typeof(Pig);
                        break;
                    case MobType.PigZombie:
                        mobType = typeof(ZombiePigman);
                        break;
                    case MobType.Sheep:
                        mobType = typeof(Sheep);
                        break;
                    case MobType.Skeleton:
                        mobType = typeof(Skeleton);
                        break;
                    case MobType.Slime:
                        mobType = typeof(Slime);
                        break;
                    case MobType.Spider:
                        mobType = typeof(Spider);
                        break;
                    case MobType.Squid:
                        mobType = typeof(Squid);
                        break;
                    case MobType.Wolf:
                        mobType = typeof(Wolf);
                        break;
                    case MobType.Zombie:
                        mobType = typeof(Zombie);
                        break;
                    default:
                        mobType = null;
                        break;
                }
            }

            if (mobType != null && typeof(Mob).IsAssignableFrom(mobType) && mobType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Chraft.World.WorldManager), typeof(int), typeof(Chraft.Net.MetaData) }, null) != null)
            {
                return mobType;
            }

            return null;
        }

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
            Type mobType = GetMobClass(world, type);

            if (mobType != null)
            {
                ConstructorInfo ci =
                    mobType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null,
                                           new Type[]
                                            {
                                                typeof (Chraft.World.WorldManager), typeof (int),
                                                typeof (Chraft.Net.MetaData)
                                            }, null);
                if (ci != null)
                    return (Mob)ci.Invoke(new object[] {world, entityId, data});
            }

            return null;
        }
    }
}
