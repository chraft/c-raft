using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Listener
{
    public class EntityListener : IChraftListener
    {
        public virtual void OnDeath(EntityDeathEventArgs e) { }
        public virtual void OnSpawn(EntitySpawnEventArgs e) { }
        public virtual void OnMove(EntityMoveEventArgs e) { }
        public virtual void OnDamaged(EntityDamageEventArgs e) { }
        public virtual void OnAttack(EntityAttackEventArgs e) { }
    }
}
