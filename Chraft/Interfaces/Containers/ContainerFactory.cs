using System;
using System.Collections.Concurrent;
using Chraft.Entity;
using Chraft.World;

namespace Chraft.Interfaces.Containers
{
    public static class ContainerFactory
    {
        private static ConcurrentDictionary<string, PersistentContainer> _containerInstances = new ConcurrentDictionary<string, PersistentContainer>();

        public static PersistentContainer Instance(WorldManager world, UniversalCoords coords)
        {
            string id = String.Format("{0}-{1},{2},{3}", world.Name, coords.WorldX, coords.WorldY, coords.WorldZ);
            PersistentContainer container;
            Chunk chunk = world.GetChunk(coords, false, false);
            if (chunk == null)
                return null; 
            BlockData.Blocks block = chunk.GetType(coords);
            if (!_containerInstances.ContainsKey(id))
            {
                switch (block)
                {
                    case BlockData.Blocks.Furnace:
                    case BlockData.Blocks.Burning_Furnace:
                        container = new FurnaceContainer();
                        container.Initialize(world, coords);
                        (container as FurnaceContainer).StartBurning();
                        break;
                    case BlockData.Blocks.Dispenser:
                        container = new DispenserContainer();
                        container.Initialize(world, coords);
                        break;
                    case BlockData.Blocks.Chest:
                        // Double chest?
                        if (IsDoubleChest(chunk, coords))
                        {
                            UniversalCoords[] doubleChestCoords = GetDoubleChestCoords(world, coords);
                            if (doubleChestCoords == null)
                                return null;
                            string firstId = String.Format("{0}-{1},{2},{3}", world.Name, doubleChestCoords[0].WorldX, doubleChestCoords[0].WorldY, doubleChestCoords[0].WorldZ);
                            string secondId = String.Format("{0}-{1},{2},{3}", world.Name, doubleChestCoords[1].WorldX, doubleChestCoords[1].WorldY, doubleChestCoords[1].WorldZ);
                            if (_containerInstances.ContainsKey(firstId))
                            {
                                _containerInstances.TryGetValue(firstId, out container);
                                return container;
                            }
                            if (_containerInstances.ContainsKey(secondId))
                            {
                                _containerInstances.TryGetValue(secondId, out container);
                                return container;
                            }

                            container = new LargeChestContainer(doubleChestCoords[1]);
                            container.Initialize(world, doubleChestCoords[0]);
                        }
                        else
                        {
                            container = new SmallChestContainer();
                            container.Initialize(world, coords);
                        }
                        break;
                    default:
                        return null;
                }
                _containerInstances.TryAdd(id, container);
            }
            else
            {
                _containerInstances.TryGetValue(id, out container);
            }
            return container;
        }

        public static void Open(Player player, UniversalCoords coords)
        {
            PersistentContainer container = Instance(player.World, coords);
            if (container == null)
                return;

            Chunk chunk = player.World.GetChunk(coords, false, false);
            if (chunk == null)
                return;
            BlockData.Blocks block = chunk.GetType(coords);
            switch (block)
            {
                case BlockData.Blocks.Furnace:
                case BlockData.Blocks.Burning_Furnace:
                    player.CurrentInterface = new FurnaceInterface(player.World, coords);
                    break;
                case BlockData.Blocks.Dispenser:
                    player.CurrentInterface = new DispenserInterface(player.World, coords);
                    break;
                case BlockData.Blocks.Chest:
                    // Double chest?
                    if (IsDoubleChest(chunk, coords))
                    {
                        UniversalCoords[] doubleChestCoords = GetDoubleChestCoords(player.World, coords);
                        if (doubleChestCoords == null)
                            return;
                        if (container.Coords == doubleChestCoords[0])
                            player.CurrentInterface = new LargeChestInterface(player.World, doubleChestCoords[0], doubleChestCoords[1]);
                        else
                            player.CurrentInterface = new LargeChestInterface(player.World, doubleChestCoords[1], doubleChestCoords[0]);
                    }
                    else
                    {
                        player.CurrentInterface = new SmallChestInterface(player.World, coords);
                    }
                    break;
                default:
                    return;
            }
            player.CurrentInterface.Associate(player);
            container.AddInterface((PersistentContainerInterface)player.CurrentInterface);
            player.CurrentInterface.Open();
        }

        public static void Close(PersistentContainerInterface containerInterface, UniversalCoords coords)
        {
            PersistentContainer container = Instance(containerInterface.World, coords);
            if (container == null)
                return;
            container.RemoveInterface(containerInterface);
            if (container.IsUnused())
            {
                PersistentContainer unused;
                string id = GetContainerId(containerInterface.World, coords);
                _containerInstances.TryRemove(id, out unused);
            }
        }

        public static void Destroy(WorldManager world, UniversalCoords coords)
        {
            PersistentContainer container = Instance(world, coords);
            if (container == null)
                return;
            string id = GetContainerId(world, coords);
            container.Destroy();
            _containerInstances.TryRemove(id, out container);
        }

        public static bool IsDoubleChest(Chunk chunk, UniversalCoords coords)
        {
            if (chunk == null)
                return false;
            return chunk.IsNSEWTo(coords,(byte) BlockData.Blocks.Chest);
        }

        public static UniversalCoords[] GetDoubleChestCoords(WorldManager world, UniversalCoords coords)
        {
            Chunk chunk = world.GetChunk(coords, false, false);
            if (chunk == null || !IsDoubleChest(chunk, coords))
                return null;
            // Is this chest the "North or East", or the "South or West"
            BlockData.Blocks[] nsewBlocks = new BlockData.Blocks[4];
            UniversalCoords[] nsewBlockPositions = new UniversalCoords[4];
            int nsewCount = 0;
            byte? blockId;
            chunk.ForNSEW(coords, uc =>
            {
                blockId = world.GetBlockId(uc) ?? 0;
                nsewBlocks[nsewCount] = (BlockData.Blocks)blockId;
                nsewBlockPositions[nsewCount] = uc;
                nsewCount++;
            });
            UniversalCoords firstCoords;
            UniversalCoords secondCoords;

            if ((byte)nsewBlocks[0] == (byte)BlockData.Blocks.Chest) // North
            {
                firstCoords = nsewBlockPositions[0];
                secondCoords = coords;
            }
            else if ((byte)nsewBlocks[2] == (byte)BlockData.Blocks.Chest) // East
            {
                firstCoords = nsewBlockPositions[2];
                secondCoords = coords;
            }
            else if ((byte)nsewBlocks[1] == (byte)BlockData.Blocks.Chest) // South
            {
                firstCoords = coords;
                secondCoords = nsewBlockPositions[1];
            }
            else// if ((byte)nsewBlocks[3] == (byte)BlockData.Blocks.Chest) // West
            {
                firstCoords = coords;
                secondCoords = nsewBlockPositions[3];
            }
            return new UniversalCoords[] { firstCoords, secondCoords };
        }

        public static string GetContainerId(WorldManager world, UniversalCoords coords)
        {
            return String.Format("{0}-{1},{2},{3}", world.Name, coords.WorldX, coords.WorldY, coords.WorldZ);
        }
    }
}
