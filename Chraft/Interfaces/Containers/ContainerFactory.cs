using System;
using System.Collections.Concurrent;
using System.IO;
using Chraft.Entity;
using Chraft.Properties;
using Chraft.World;

namespace Chraft.Interfaces.Containers
{
    public static class ContainerFactory
    {
        public static PersistentContainer Instance(WorldManager world, UniversalCoords coords)
        {
            PersistentContainer container;
            Chunk chunk = world.GetChunk(coords, false, false);
            if (chunk == null)
                return null; 
            BlockData.Blocks block = chunk.GetType(coords);
            if (!chunk.Containers.ContainsKey(coords.BlockPackedCoords))
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
                            chunk.Containers.TryRemove(doubleChestCoords[0].BlockPackedCoords, out container);
                            chunk.Containers.TryRemove(doubleChestCoords[1].BlockPackedCoords, out container);

                            container = new LargeChestContainer(doubleChestCoords[1]);
                            container.Initialize(world, doubleChestCoords[0]);
                            chunk.Containers.TryAdd(doubleChestCoords[0].BlockPackedCoords, container);
                            chunk.Containers.TryAdd(doubleChestCoords[1].BlockPackedCoords, container);
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
                chunk.Containers.TryAdd(coords.BlockPackedCoords, container);
            }
            else
            {
                chunk.Containers.TryGetValue(coords.BlockPackedCoords, out container);
            }
            return container;
        }

        public static void LoadContainersFromDisk(Chunk chunk)
        {
            string containerPath = Path.Combine(chunk.World.Folder, Settings.Default.ContainersFolder, "x" + chunk.Coords.ChunkX + "z" + chunk.Coords.ChunkZ);
            if (!Directory.Exists(containerPath))
                return;
            
            string[] files = Directory.GetFiles(containerPath);

            PersistentContainer container;
            UniversalCoords containerCoords;
            BlockData.Blocks containerType;
            string id;
            int x, y, z, startPos, endPos;
            string coordStr;
            foreach (var file in files)
            {
                coordStr = file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                startPos = coordStr.IndexOf("x") + 1;
                endPos = coordStr.IndexOf("y");
                if (!Int32.TryParse(coordStr.Substring(startPos, endPos - startPos), out x))
                    continue;
                startPos = endPos + 1;
                endPos = coordStr.IndexOf("z");
                if (!Int32.TryParse(coordStr.Substring(startPos, endPos - startPos), out y))
                    continue;
                startPos = endPos + 1;
                endPos = coordStr.IndexOf(".");
                if (!Int32.TryParse(coordStr.Substring(startPos, endPos - startPos), out z))
                    continue;
                containerCoords = UniversalCoords.FromWorld(x, y, z);
                containerType = chunk.GetType(containerCoords);
                switch (containerType)
                {
                    case BlockData.Blocks.Dispenser:
                        container = new DispenserContainer();
                        container.Initialize(chunk.World, containerCoords);
                        break;
                    case BlockData.Blocks.Furnace:
                    case BlockData.Blocks.Burning_Furnace:
                        container = new FurnaceContainer();
                        container.Initialize(chunk.World, containerCoords);
                        (container as FurnaceContainer).StartBurning();
                        break;
                    default:
                        continue;
                }
                chunk.Containers.TryAdd(containerCoords.BlockPackedCoords, container);
            }
        }

        public static void UnloadContainers(Chunk chunk)
        {
            foreach (var container in chunk.Containers)
            {
                container.Value.Save();
                if (container.Value is FurnaceContainer)
                    (container.Value as FurnaceContainer).Unload();
            }
            chunk.Containers.Clear();
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
                    if (container is LargeChestContainer)
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
            Chunk chunk = container.World.GetChunk(coords, false, false);
            if (chunk == null)
                return;
            PersistentContainer unused;
            if (container is LargeChestContainer && container.IsUnused())
            {
                chunk.Containers.TryRemove(container.Coords.BlockPackedCoords, out unused);
                chunk.Containers.TryRemove((container as LargeChestContainer).SecondCoords.BlockPackedCoords, out unused);
            } else if (container is SmallChestContainer && container.IsUnused())
                chunk.Containers.TryRemove(container.Coords.BlockPackedCoords, out unused);
        }

        public static void Destroy(WorldManager world, UniversalCoords coords)
        {
            PersistentContainer container = Instance(world, coords);
            if (container == null)
                return;
            Chunk chunk = world.GetChunk(coords, false, false);
            if (chunk == null)
                return;
            PersistentContainer unused;
            container.Destroy();
            chunk.Containers.TryRemove(container.Coords.BlockPackedCoords, out unused);
            if (container is LargeChestContainer)
                chunk.Containers.TryRemove((container as LargeChestContainer).SecondCoords.BlockPackedCoords, out unused);
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
    }
}
