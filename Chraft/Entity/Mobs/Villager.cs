using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.Utilities.Misc;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Villager : Animal
    {
        public override string Name
        {
            get { return "Villager"; }
        }

        public override short MaxHealth { get { return 20; } }

        internal Villager(WorldManager world, int entityId, MobType type, MetaData data)
            : base(world, entityId, type, data)
        {
            MaxExp = 0;
            MinExp = 0;
        }

        protected override void DoInteraction(IClient iClient, IItemInventory item)
        {
            //TODO: Add trading with villagers
        }
    }
}
