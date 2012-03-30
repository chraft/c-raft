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

using Chraft.PluginSystem;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Listener;

namespace Chraft.Plugins.SamplePlugin
{
    class SamplePluginEntitiyListener : IEntityListener
    {

        private readonly IPlugin _plugin;

        public void OnDeath(EntityDeathEventArgs e)
        {
         
        }

        public void OnSpawn(EntitySpawnEventArgs e)
        {
            if (e.EventCanceled) return;
            _plugin.Server.Broadcast(e.Entity.GetType() + " Spawned");
        }

        public void OnMove(EntityMoveEventArgs e)
        {
        
        }

        public void OnDamaged(EntityDamageEventArgs e)
        {
        
        }

        public void OnAttack(EntityAttackEventArgs e)
        {
        
        }

        public SamplePluginEntitiyListener(IPlugin plugin)
        {
            _plugin = plugin;
        }
    }
}
