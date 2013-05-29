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

namespace Chraft.Utilities.Misc
{
    public static class Experience
    {
        public static short GetLevel(short experience)
        {
            short level = 0;
            if (experience <= 16)
                return level;

            short toTheNext = ExpToNextLevel(level);
            while (experience >= toTheNext)
            {
                level++;
                experience -= toTheNext;
                toTheNext = ExpToNextLevel(level);
            }

            return level;
        }

        public static short GetExperience(short level)
        {
            if (level <= 0)
                return 0;

            if (level >= 1 && level <= 15)
                return (short)System.Math.Min(short.MaxValue, 17 * level);

            if (level >= 16 && level <= 30)
                return (short)System.Math.Min(short.MaxValue, 1.5 * (level * level) - (29.5 * level) + 360);

            // Levels 31+
            return (short)System.Math.Min(short.MaxValue, 3.5 * (level * level) - (151.5 * level) + 2220);
        }

        public static short ExpToNextLevel(short currentLevel)
        {
            // TODO: Proper handling of negative level
            if (currentLevel < 0)
                return 17;

            if (currentLevel >= 0 && currentLevel <= 14)
                return 17;

            if (currentLevel >= 15 && currentLevel <= 29)
                return (short)System.Math.Min(short.MaxValue, 3 * currentLevel - 28);

            // Levels 30+
            return (short)System.Math.Min(short.MaxValue, 7 * currentLevel - 148);
        }
    }
}
