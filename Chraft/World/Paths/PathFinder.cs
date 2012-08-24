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

using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Chraft.Entity;
using Chraft.PluginSystem.Server;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.Utils;
using Chraft.World.Blocks;
using System;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Paths
{
    public class PathFinder
    {
        protected WorldManager World { get; set; }
        private PathCoordinate _firstCoordinate;
        private readonly Dictionary<UniversalCoords, PathCoordinate> _coordinateCache = new Dictionary<UniversalCoords, PathCoordinate>();

        public PathFinder(WorldManager world)
        {
            World = world;
        }
        
        public List<PathCoordinate> CreatePathToEntity(EntityBase entityFrom, EntityBase entityTo, double maxDistance = 24.0, double untilDistanceToTarget = 1.0)
        {
            PathCoordinate start = GetCoordinateFromCacheOrAdd(entityFrom.BlockPosition);
            PathCoordinate end = GetCoordinateFromCacheOrAdd(entityTo.BlockPosition);
            Size size = new Size((int)Math.Floor(entityFrom.Width + 1.0), (int)Math.Floor(entityFrom.Height + 1.0));

            return GeneratePath(start, end, size, maxDistance, untilDistanceToTarget);
        }

        public List<PathCoordinate> CreatePathToCoordinate(EntityBase entityFrom, AbsWorldCoords coordinate, double maxDistance = 24.0, double untilDistanceToTarget = 1.0)
        {
            PathCoordinate start = GetCoordinateFromCacheOrAdd(entityFrom.BlockPosition);
            PathCoordinate end = GetCoordinateFromCacheOrAdd(UniversalCoords.FromAbsWorld(coordinate.X - (entityFrom.Width * 0.5), coordinate.Y, coordinate.Z - (entityFrom.Width * 0.5)));
            Size size = new Size((int)Math.Floor(entityFrom.Width + 1.0), (int)Math.Floor(entityFrom.Height + 1.0));

            return GeneratePath(start, end, size, maxDistance, untilDistanceToTarget);
        }

        private List<PathCoordinate> GeneratePath(PathCoordinate start, PathCoordinate end, Size size, double maxDistance, double untilDistanceToTarget)
        {
            SortedSet<PathCoordinate> sortedPath = new SortedSet<PathCoordinate>(new PathCoordinateDistanceToTargetComparer());
            
            List<PathCoordinate> path = new List<PathCoordinate>();
            _firstCoordinate = start;
            _firstCoordinate.DistanceToTarget = _firstCoordinate.DistanceTo(end);

            PathCoordinate currentCoordinate = start;
            int loopsLeft = 1000;
            while (currentCoordinate != null && currentCoordinate != end && currentCoordinate.DistanceToTarget > untilDistanceToTarget && loopsLeft > 0)
            {
                currentCoordinate.Checked = true;
                loopsLeft--;
                PathCoordinate[] pathOptions = GetPathOptions(currentCoordinate, end, size, maxDistance);

                foreach (var option in pathOptions)
                {
                    double distance = currentCoordinate.DistanceFromStart + currentCoordinate.DistanceTo(option);
                    bool existsInPath = sortedPath.Contains(option);

                    if (!existsInPath || distance < option.DistanceFromStart)
                    {
                        option.PreviousCoordinate = currentCoordinate;

                        if (existsInPath)
                        {
                            sortedPath.Remove(option);
                            option.SetDistanceFromStartAndTarget(distance, option.DistanceTo(end));
                            sortedPath.Add(option);
                        }
                        else
                        {
                            option.SetDistanceFromStartAndTarget(distance, option.DistanceTo(end));
                            sortedPath.Add(option);
                        }
                    }
                }
                sortedPath.Remove(currentCoordinate);
                if (sortedPath.Count > 0)
                    currentCoordinate = sortedPath.Min;
                else
                    break;

                if (currentCoordinate != null && currentCoordinate.PreviousCoordinate != null && currentCoordinate.DistanceToTarget > currentCoordinate.PreviousCoordinate.DistanceToTarget)
                    break;
            }

            if (currentCoordinate == null || currentCoordinate == _firstCoordinate)
                return null;

            // currentCoordinate is destination so walk up previous coordinates and add to list, then reverse
            List<PathCoordinate> result = new List<PathCoordinate> {currentCoordinate};
            while (currentCoordinate.PreviousCoordinate != null)
            {
                result.Add(currentCoordinate.PreviousCoordinate);
                currentCoordinate = currentCoordinate.PreviousCoordinate;
            }

            result.Reverse();
            return result;
        }
        
        /// <summary>
        /// Returns "safe" coordinates 1 block (NSEW) from <paramref name="start"/> where they are not the first point, and the distance to end is less than <paramref name="maxDistance"/>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="size"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        private PathCoordinate[] GetPathOptions(PathCoordinate start, PathCoordinate end, Size size, double maxDistance)
        {
            int height = 0;
            if (CheckOffset(start.Coordinate.Offset(0, 1, 0), size) == 1)
            {
                height = 1;
            }

            List<PathCoordinate> pathOptions = new List<PathCoordinate>();

            PathCoordinate eastPoint = GetSafeCoordinate(start.Coordinate.Offset(0, 0, 1), size, height);
            PathCoordinate northPoint = GetSafeCoordinate(start.Coordinate.Offset(-1, 0, 0), size, height);
            PathCoordinate southPoint = GetSafeCoordinate(start.Coordinate.Offset(1, 0, 0), size, height);
            PathCoordinate westPoint = GetSafeCoordinate(start.Coordinate.Offset(0, 0, -1), size, height);

            if (eastPoint != null && !eastPoint.Checked && eastPoint.DistanceTo(end) < maxDistance)
                pathOptions.Add(eastPoint);

            if (northPoint != null && !northPoint.Checked && northPoint.DistanceTo(end) < maxDistance)
                pathOptions.Add(northPoint);

            if (southPoint != null && !southPoint.Checked && southPoint.DistanceTo(end) < maxDistance)
                pathOptions.Add(southPoint);

            if (westPoint != null && !westPoint.Checked && westPoint.DistanceTo(end) < maxDistance)
                pathOptions.Add(westPoint);

            return pathOptions.ToArray();
        }

        private PathCoordinate GetCoordinateFromCacheOrAdd(UniversalCoords coordinate)
        {
            PathCoordinate result = null;

            if (!_coordinateCache.TryGetValue(coordinate, out result))
            {
                result = new PathCoordinate(coordinate);
                _coordinateCache[coordinate] = result;
            }

            return result;
        }
        
        /// <summary>
        /// Returns an integer indicating if the 
        /// </summary>
        /// <param name="startCoordinate"></param>
        /// <param name="sizeToCheck"></param>
        /// <param name="heightOffset"></param>
        /// <returns></returns>
        private PathCoordinate GetSafeCoordinate(UniversalCoords startCoordinate, Size sizeToCheck, int heightOffset)
        {
            // Given a coordinate, determine if it is safe to move to allowing Y +- 1 and taking into consideration size of space to check
            
            PathCoordinate result = null;

            if (heightOffset > 0 && CheckOffset(startCoordinate.Offset(0, heightOffset, 0), sizeToCheck) == 1)
                startCoordinate = startCoordinate.Offset(0, heightOffset, 0);

            result = GetCoordinateFromCacheOrAdd(startCoordinate);
            
            if (result != null)
            {
                // Check down the Y axis for unsafe blocks
                int checkOffsetResult = 0;
                int loopCount = 0;
                
                while(startCoordinate.WorldY > 0 && (checkOffsetResult = CheckOffset(startCoordinate.Offset(0, -1, 0), sizeToCheck)) == 1)
                {
                    if (++loopCount >= 4)
                    {
                        return null;
                    }
                    
                    startCoordinate = startCoordinate.Offset(0, -1, 0);
                    if (startCoordinate.WorldY > 0)
                    {
                        result = GetCoordinateFromCacheOrAdd(startCoordinate);
                    }
                }
                if (checkOffsetResult == -2)
                {
                    return null; // Lava
                }
            }
            
            return result;
        }
        
        private int CheckOffset(UniversalCoords start, Size size)
        {
            for (int x = start.WorldX; x < start.WorldX + size.Width; x++)
            {
                for (int y = start.WorldY; y < start.WorldY + size.Height; y++)
                {
                    for (int z = start.WorldZ; z < start.WorldZ + size.Width; z++)
                    {
                        StructBlock block = (StructBlock) World.GetBlock(x, y, z);
                        if (block.Type <= 0)
                            continue;

                        BlockBase blockClass = BlockHelper.Instance.CreateBlockInstance(block.Type);

                        if (blockClass is BlockBaseDoor)
                        {
                            if (!((BlockBaseDoor) blockClass).IsOpen(block))
                            {
                                return 0;
                            }
                            continue;
                        }
                        if (blockClass.IsSolid)
                        {
                            return 0;
                        }
                        if (blockClass.IsLiquid)
                        {
                            if (!blockClass.IsOpaque) // Water
                            {
                                return -1;
                            }
                            else // Lava
                            {
                                return -2;
                            }
                        }
                    }
                }
            }
            
            return 1;
        }
    }
}

