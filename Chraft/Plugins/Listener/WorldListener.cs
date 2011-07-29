using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Listener
{
    public class WorldListener : ChraftListener
    {
        public virtual void OnWorldLoaded(WorldLoadEventArgs e) { }
        public virtual void OnWorldUnloaded(WorldUnloadEventArgs e) { }
        public virtual void OnWorldJoined(WorldJoinedEventArgs e) { }
        public virtual void OnWorldLeft(WorldLeftEventArgs e) { }
        public virtual void OnWorldCreated(WorldCreatedEventArgs e) { }
        public virtual void OnWorldDeleted(WorldDeletedEventArgs e) { }
    }
}
