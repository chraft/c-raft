using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Properties;

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
			if (0 == Rand.Next(Settings.Default.WeatherChangeFrequency))
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
