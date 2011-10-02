using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Listener
{
    public class BlockListener : ChraftListener
    {
        public virtual void OnDestroy(BlockDestroyEventArgs e) { }
        public virtual void OnPlace(BlockPlaceEventArgs e) { }
        public virtual void OnTouch(BlockTouchEventArgs e) { }
    }
}
