using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utils;
using Chraft.Entity;
using Chraft.World.Blocks;

namespace Chraft.World
{
    public static class WorldMobSpawner
    {
        private const int MaxSpawnDistance = 8;

        // TODO: Types of mobs and total number that can spawn should be controlled by the Biome not hard coded here (see #128)
        private static readonly List<WeightedValue<MobType>> Monsters = new List<WeightedValue<MobType>>
                                                {
                                                    WeightedValue.Create(10, MobType.Creeper),
                                                    WeightedValue.Create(10, MobType.CaveSpider),
                                                    WeightedValue.Create(10, MobType.Skeleton),
                                                    WeightedValue.Create(10, MobType.Zombie),
                                                    WeightedValue.Create(2, MobType.Enderman),
                                                };
        private const int MaxMonsters = 70;

        private static readonly List<WeightedValue<MobType>> Creatures = new List<WeightedValue<MobType>>
                                                 {
                                                     WeightedValue.Create(12, MobType.Sheep),
                                                     WeightedValue.Create(8, MobType.Cow),
                                                     WeightedValue.Create(10, MobType.Wolf), // TODO: wolves are only supposed to spawn in certain Bimoes (see #128)
                                                     WeightedValue.Create(10, MobType.Hen), // TODO: hens are only supposed to spawn in certain Biomes (see #128)
                                                     WeightedValue.Create(10, MobType.Pig),
                                                 };

        private const int MaxCreatures = 15;

        private static readonly List<WeightedValue<MobType>> WaterCreatures = new List<WeightedValue<MobType>>
                                                      {
                                                          WeightedValue.Create(10, MobType.Squid),
                                                      };

        private const int MaxWaterCreatures = 5;


        static UniversalCoords GetRandomPointInChunk(WorldManager world, int chunkX, int chunkZ)
        {
            return UniversalCoords.FromBlock(chunkX, chunkZ, world.Server.Rand.Next(16), world.Server.Rand.Next(128),
                                             world.Server.Rand.Next(16));
        }
        
        public static void SpawnMobs(WorldManager world, bool spawnHostileMobs, bool spawnPeacefulMobs)
        {
            var players = world.Server.GetAuthenticatedClients().Where(c => c.Owner.World == world).Select(c => c.Owner).ToArray();
            HashSet<int> chunksToSpawnIn = new HashSet<int>();

            #region Get a list of all chunks within 8 chunks of any players
            foreach (var player in players)
            {
                UniversalCoords coord = UniversalCoords.FromAbsWorld(player.Position);

                for (int x = -MaxSpawnDistance; x <= MaxSpawnDistance; x++)
                {
                    for (int z = -MaxSpawnDistance; z <= MaxSpawnDistance; z++)
                    {
                        chunksToSpawnIn.Add(UniversalCoords.FromChunkToPackedChunk(coord.ChunkX + x, coord.ChunkZ + z));
                    }
                }
            }
            #endregion

            // Get a list of Mob entities outside of the loop so we only get it once
            Mob[] mobEntities = world.GetEntities().Where(e => e is Mob).Select((e) => e as Mob).ToArray();

            // TODO: need to use Biome to get the list of mob types available for each category
            // TODO: make the maximum count of mobs per category configurable
            if (spawnHostileMobs)
            {
                DoSpawn(world, chunksToSpawnIn, mobEntities, Monsters, MaxMonsters);
            }
            if (spawnPeacefulMobs)
            {
                DoSpawn(world, chunksToSpawnIn, mobEntities, Creatures, MaxCreatures);
                DoSpawn(world, chunksToSpawnIn, mobEntities, WaterCreatures, MaxWaterCreatures, true);
            }
        }

        private static void DoSpawn(WorldManager world, HashSet<int> chunksToSpawnIn, Mob[] mobEntities, List<WeightedValue<MobType>> mobGroup, int maximumMobs, bool inWater = false)
        {
            // Check that we haven't already reached the maximum number of this type of mob
            if (mobGroup.Count > 0 && (mobEntities.Where(e => mobGroup.Where(mob => mob.Value == e.Type).Any()).Count() <= maximumMobs * chunksToSpawnIn.Count / 256))
            {
                foreach (var packedChunk in chunksToSpawnIn)
                {
                    MobType mobType = mobGroup.SelectRandom(world.Server.Rand);
                    UniversalCoords packSpawnPosition = GetRandomPointInChunk(world, UniversalCoords.FromPackedChunkToX(packedChunk), UniversalCoords.FromPackedChunkToZ(packedChunk));

                    BlockBase blockClass = BlockHelper.Instance(world.GetBlockId(packSpawnPosition));

                    if (!blockClass.IsOpaque && ((!inWater && blockClass.Type == BlockData.Blocks.Air) || (inWater && blockClass.IsLiquid))) // Lava is Opaque, so IsLiquid is safe to use here for water & still water
                    {
                        int spawnedCount = 0;
                        // Make 3 separate attempts to spawn around the spawnPosition at 4 varying distances
                        for (int i = 0; i < 3; i++)
                        {
                            int x = packSpawnPosition.WorldX;
                            int y = packSpawnPosition.WorldY;
                            int z = packSpawnPosition.WorldZ;

                            const int distance = 6;

                            // Make four attempts to spawn at increasing distances from spawnPosition
                            for (int j = 0; j < 4; j++)
                            {
                                x += world.Server.Rand.Next(distance) - world.Server.Rand.Next(distance);
                                y += world.Server.Rand.Next(1) - world.Server.Rand.Next(1);
                                z += world.Server.Rand.Next(distance) - world.Server.Rand.Next(distance);

                                if (CanMobTypeSpawnAtLocation(mobType, world, x, y, z))
                                {
                                    AbsWorldCoords spawnPosition = new AbsWorldCoords((double)x + 0.5, y, (double)z + 0.5);

                                    // Check that no player is within a radius of 24 blocks of the spawnPosition
                                    if (world.GetClosestPlayer(spawnPosition, 24.0) == null)
                                    {
                                        // Check that the squared distance is more than 576 from spawn (24 blocks)
                                        if (spawnPosition.ToVector().DistanceSquared(new AbsWorldCoords(world.Spawn).ToVector()) > 576.0)
                                        {
                                            Mob newMob = MobFactory.CreateMob(world, world.Server.AllocateEntity(),
                                                                              mobType);

                                            if (newMob == null)
                                                break;

                                            newMob.Position = spawnPosition;
                                            newMob.Yaw = world.Server.Rand.NextDouble()*360.0;
                                            // Finally apply any mob specific rules about spawning here
                                            if (newMob.CanSpawnHere())
                                            {
                                                ++spawnedCount;
                                                world.Server.AddEntity(newMob);
                                                MobSpecificInitialisation(newMob, world, spawnPosition);
                                                world.Server.SendEntityToNearbyPlayers(world, newMob);

                                                if (spawnedCount >= newMob.MaxSpawnedPerGroup)
                                                {
                                                    // Don't pull that face at me - there are legitimate reasons to use a goto, like break out of two for loops and continue the foreach iteration
                                                    // Ok ok, we could do a break, and duplicate the above if statement and then break in the next for and that would do the same, 
                                                    // but seriously "goto nextChunk" does make more sense, that's exactly what we are doing!
                                                    goto nextChunk;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                nextChunk:;
                }
            }
        }

        private static bool CanMobTypeSpawnAtLocation(MobType mobType, WorldManager world, int worldX, int worldY, int worldZ)
        {
            BlockBase blockClass = BlockHelper.Instance(world.GetBlockId(worldX, worldY, worldZ));
            if (mobType == MobType.Squid)
            {
                return (blockClass.IsLiquid && !blockClass.IsOpaque) // Is Water
                    && !BlockHelper.Instance((world.GetBlockId(worldX, worldY + 1, worldZ))).IsOpaque; // Has either water or air above it
            }
            else
            {
                return BlockHelper.Instance((world.GetBlockId(worldX, worldY - 1, worldZ))).IsOpaque && BlockHelper.Instance((world.GetBlockId(worldX, worldY - 1, worldZ))).IsSolid && // Is solid underneath
                       !blockClass.IsOpaque && !blockClass.IsSolid && !blockClass.IsLiquid && // Is not solid or liquid where spawning 
                       !BlockHelper.Instance((world.GetBlockId(worldX, worldY + 1, worldZ))).IsOpaque && !BlockHelper.Instance((world.GetBlockId(worldX, worldY + 1, worldZ))).IsSolid; // Is not solid 1 block above
            }
        }

        private static void MobSpecificInitialisation(Mob mob, WorldManager world, AbsWorldCoords coords)
        {
            // 1 in 100 chance to spawn a skeleton riding a spider
            if (mob.Type == MobType.Spider && world.Server.Rand.Next(100) == 0)
            {
                LivingEntity skeleton = MobFactory.CreateMob(world, world.Server.AllocateEntity(), MobType.Skeleton);
                skeleton.Position = coords;
                skeleton.Yaw = mob.Yaw;
                world.Server.AddEntity(skeleton);
                skeleton.MountEntity(mob);
            }
        }
    }
}
