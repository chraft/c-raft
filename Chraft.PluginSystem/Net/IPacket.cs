using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.PluginSystem
{
    public interface IPacket
    {
        void SetShared(ILogger logger, int length);
    }
}
