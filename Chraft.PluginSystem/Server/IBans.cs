using System;

namespace Chraft.PluginSystem.Server
{
    public interface IBans
    {
        string PlayerName { get; set; }
        string Reason { get; set; }
        DateTime Duration { get; set; }
    }
}