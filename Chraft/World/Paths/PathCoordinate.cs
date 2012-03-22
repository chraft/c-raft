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
using System.Diagnostics;
using Chraft.Entity;
using Chraft.Utilities;
using Chraft.Utilities.Coords;

namespace Chraft.World.Paths
{
    public class PathCoordinateDistanceFromStartComparer: Comparer<PathCoordinate>
    {
        public override int Compare(PathCoordinate x, PathCoordinate y)
        {
            Debug.Assert(x != null, "x != null");
            Debug.Assert(y != null, "y != null");
            return x.DistanceFromStart.CompareTo(y.DistanceFromStart);
        }
    }

    public class PathCoordinateDistanceToTargetComparer : Comparer<PathCoordinate>
    {
        public override int Compare(PathCoordinate x, PathCoordinate y)
        {
            Debug.Assert(x != null, "x != null");
            Debug.Assert(y != null, "y != null");
            return x.DistanceToTarget.CompareTo(y.DistanceToTarget);
        }
    }

    public class PathCoordinateTotalPathDistanceComparer : Comparer<PathCoordinate>
    {
        public override int Compare(PathCoordinate x, PathCoordinate y)
        {
            Debug.Assert(x != null, "x != null");
            Debug.Assert(y != null, "y != null");
            return x.TotalPathDistance.CompareTo(y.TotalPathDistance);
        }
    }

    public class PathCoordinate
    {
        public readonly UniversalCoords Coordinate;
        public double DistanceToNext { get; set; }
        
        private double _distanceToTarget;
        public double DistanceToTarget
        {
            get { return _distanceToTarget; }
            set
            {
                _distanceToTarget = value;
                TotalPathDistance = DistanceFromStart + _distanceToTarget;
            }
        }

        private double _distanceFromStart;
        public double DistanceFromStart
        {
            get { return _distanceFromStart; }
            set
            {
                _distanceFromStart = value;
                TotalPathDistance = _distanceFromStart + DistanceToTarget;
            }
        }

        public PathCoordinate PreviousCoordinate { get; set; }
        public double TotalPathDistance { get; set; }
        public bool Checked { get; set; }

        public PathCoordinate(UniversalCoords coords)
        {
            Coordinate = coords;
            DistanceToNext = 0;
            DistanceToTarget = 0;
            DistanceFromStart = 0;
            PreviousCoordinate = null;
        }

        /// <summary>
        /// Returns the distance to another path coordinate
        /// </summary>
        /// <param name="pathCoord"></param>
        /// <returns></returns>
        public double DistanceTo(PathCoordinate pathCoord)
        {
            return Coordinate.DistanceTo(pathCoord.Coordinate);
        }

        public PathCoordinate GetClosest(PathCoordinate pathCoord1, PathCoordinate pathCoord2)
        {
            return (Coordinate.DistanceToSquared(pathCoord1.Coordinate) <= Coordinate.DistanceToSquared(pathCoord2.Coordinate)) ? pathCoord1 : pathCoord2;
        }

        public AbsWorldCoords AsEntityPosition(EntityBase entity)
        {
            return new AbsWorldCoords(Coordinate.WorldX + (entity.Width + 1.0) * 0.5, Coordinate.WorldY, Coordinate.WorldZ + (entity.Width + 1.0) * 0.5);
        }

        internal void SetDistanceFromStartAndTarget(double distanceFromStart, double distanceToTarget)
        {
            this._distanceFromStart = distanceFromStart;
            this.DistanceToTarget = distanceToTarget;
        }
    }
}
