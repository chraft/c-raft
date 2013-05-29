using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.PluginSystem.Item
{
    public interface IItemConsumable
    {
        void StartConsuming();
        void FinishConsuming();
    }
}
