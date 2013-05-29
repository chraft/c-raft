using System;

namespace Chraft.PluginSystem.Server
{
    public interface IIpBans
    {
        string Ip { get; set; }
        string Reason { get; set; }
        DateTime Duration { get; set; }
    }
}