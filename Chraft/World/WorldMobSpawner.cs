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
using Chraft.Net;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Event;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;
using Chraft.Utilities.Misc;
using Chraft.Utils;
using Chraft.Entity;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Base;

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
            Client[] authClients = world.Server.GetAuthenticatedClients() as Client[];
            var players = authClients.Where(c => c.Owner.World == world).Select(c => c.Owner).ToArray();
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
            // Based on original server spawn logic and minecraft wiki (http://www.minecraftwiki.net/wiki/Spawn)

            // Check that we haven't already reached the maximum number of this type of mob
            if (mobGroup.Count > 0 && (mobEntities.Where(e => mobGroup.Where(mob => mob.Value == e.Type).Any()).Count() <= maximumMobs * chunksToSpawnIn.Count / 256))
            {
                foreach (var packedChunk in chunksToSpawnIn)
                {
                    MobType mobType = mobGroup.SelectRandom(world.Server.Rand);
                    UniversalCoords packSpawnPosition = GetRandomPointInChunk(world, UniversalCoords.FromPackedChunkToX(packedChunk), UniversalCoords.FromPackedChunkToZ(packedChunk));

                    byte? blockId = world.GetBlockId(packSpawnPosition);

                    if (blockId == null)
                        continue;

                    BlockBase blockClass = BlockHelper.Instance.CreateBlockInstance((byte)blockId);

                    if (!blockClass.IsOpaque && ((!inWater && blockClass.Type == BlockData.Blocks.Air) || (inWater && blockClass.IsLiquid))) // Lava is Opaque, so IsLiquid is safe to use here for water & still water
                    {
                        int spawnedCount = 0;
                        int x = packSpawnPosition.WorldX;
                        int y = packSpawnPosition.WorldY;
                        int z = packSpawnPosition.WorldZ;
 
                        for (int i = 0; i < 21; i++)
                        {
                            // Every 4th attempt reset the coordinates to the centre of the pack spawn
                            if (i % 4 == 0)
                            {
                                x = packSpawnPosition.WorldX;
                                y = packSpawnPosition.WorldY;
                                z = packSpawnPosition.WorldZ;
                            }

                            const int distance = 6;

                            x += world.Server.Rand.Next(distance) - world.Server.Rand.Next(distance);
                            y += world.Server.Rand.Next(1) - world.Server.Rand.Next(1);
                            z += world.Server.Rand.Next(distance) - world.Server.Rand.Next(distance);

                            if (CanMobTypeSpawnAtLocation(mobType, world, x, y, z))
                            {
                                AbsWorldCoords spawnPosition = new AbsWorldCoords(x + 0.5, y, z + 0.5);

                                // Check that no player is within a radius of 24 blocks of the spawnPosition
                                if (world.GetClosestPlayer(spawnPosition, 24.0) == null)
                                {
                                    // Check that the squared distance is more than 576 from spawn (24 blocks)
                                    if (spawnPosition.ToVector().DistanceSquared(new AbsWorldCoords(world.Spawn).ToVector()) > 576.0)
                                    {
                                        Mob newMob = MobFactory.Instance.CreateMob(world, world.Server,
                                                                          mobType) as Mob;

                                        if (newMob == null)
                                            break;

                                        newMob.Position = spawnPosition;
                                        newMob.Yaw = world.Server.Rand.NextDouble()*360.0;
                                        // Finally apply any mob specific rules about spawning here
                                        if (newMob.CanSpawnHere())
                                        {
                                            //Event
                                            EntitySpawnEventArgs e = new EntitySpawnEventArgs(newMob, newMob.Position);
                                            world.Server.PluginManager.CallEvent(Event.EntitySpawn, e);
                                            if (e.EventCanceled)
                                                continue;
                                            newMob.Position = e.Location;
                                            //End Event

                                            ++spawnedCount;
                                            MobSpecificInitialisation(newMob, world, spawnPosition);
                                            world.Server.AddEntity(newMob);

                                            if (spawnedCount >= newMob.MaxSpawnedPerGroup)
                                            {
                                                // This chunk is full - move to the next
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool CanMobTypeSpawnAtLocation(MobType mobType, WorldManager world, int worldX, int worldY, int worldZ)
        {
            Chunk chunk = world.GetChunkFromWorld(worldX, worldZ) as Chunk;
            if (chunk == null || worldY == 0)
                return false;

            BlockBase blockClassCurrent = BlockHelper.Instance.CreateBlockInstance((byte)chunk.GetType(worldX & 0xF, worldY, worldZ & 0xF));
            BlockBase blockClassUp = BlockHelper.Instance.CreateBlockInstance((byte)chunk.GetType(worldX & 0xF, worldY + 1, worldZ & 0xF));          

            if (mobType == MobType.Squid)
            {
                return (blockClassCurrent.IsLiquid && !blockClassCurrent.IsOpaque) // Is Water
                    && !blockClassUp.IsOpaque; // Has either water or air above it
            }

            BlockBase blockClassDown = BlockHelper.Instance.CreateBlockInstance((byte)chunk.GetType(worldX & 0xF, worldY - 1, worldZ & 0xF));

            return blockClassDown.IsOpaque && blockClassDown.IsSolid && // Is solid underneath
                    !blockClassCurrent.IsOpaque && !blockClassCurrent.IsSolid && !blockClassCurrent.IsLiquid && // Is not solid or liquid where spawning 
                    !blockClassUp.IsOpaque && !blockClassUp.IsSolid; // Is not solid 1 block above
        }

        private static void MobSpecificInitialisation(Mob mob, WorldManager world, AbsWorldCoords coords)
        {
            // 1 in 100 chance to spawn a skeleton riding a spider
            if (mob.Type == MobType.Spider && world.Server.Rand.Next(100) == 0)
            {
                LivingEntity skeleton = MobFactory.Instance.CreateMob(world, world.Server, MobType.Skeleton) as LivingEntity;
                skeleton.Position = coords;
                skeleton.Yaw = mob.Yaw;
                
                world.Server.AddEntity(skeleton);
                skeleton.MountEntity(mob);
            }
        }
    }
}
