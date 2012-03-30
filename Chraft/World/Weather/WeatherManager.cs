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
using Chraft.Utilities.Config;

namespace Chraft.World.Weather
{
	public class WeatherManager
	{
		private Random Rand = new Random();

		public WorldManager World { get; private set; }
		public WeatherState Weather { get; private set; }

		internal WeatherManager(WorldManager world)
		{
			World = world;
			World.Server.Pulse += new EventHandler(Server_Pulse);
		}

		private void Server_Pulse(object sender, EventArgs e)
		{
			if (0 == Rand.Next(ChraftConfig.WeatherChangeFrequency))
				RandomizeWeather();
			UpdateChunks();
		}

		private void UpdateChunks()
		{
			foreach (Chunk c in World.GetChunks())
			{
				c.SetWeather(Weather);
			}
		}

		public void RandomizeWeather()
		{
			Weather = (WeatherState)Rand.Next(Enum.GetValues(typeof(WeatherState)).Length);
		}
	}
}
