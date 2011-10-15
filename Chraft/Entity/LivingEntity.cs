using System;

namespace Chraft.Entity
{
    public abstract class LivingEntity : EntityBase
    {
        public override short MaxHealth
        {
            get
            {
                return 20;
            }
        }
    
        public LivingEntity(Server server, int entityId)
         : base(server, entityId)
        {
            this.Health = 20;
        }
    }
}

