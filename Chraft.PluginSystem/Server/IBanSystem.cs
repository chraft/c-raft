using System;
using System.Collections.Generic;

namespace Chraft.PluginSystem.Server
{
    public interface IBanSystem
    {
        void LoadBansAndWhiteList();
        void LoadWhiteList();
        void AddToBanList(string player, DateTime duration, string reason = "Banned");
        void AddToBanList(string player, string reason = "Banned", string[] tokenDuration = null);
        void AddToIpBans(string ip, string reason = "Banned", string[] tokenDuration = null);
        void AddToIpBans(string ip, DateTime duration, string reason = "Banned");
        void AddToWhiteList(string player);
        void RemoveFromWhiteList(string player);
        bool HasBan(string playerName);
        bool HasIpBan(string ip);
        bool IsOnWhiteList(string player);
        void RemoveFromIpBanList(string ip);
        void RemoveFromBanList(string player);
        List<string> ListWhiteList();
        IBans GetBan(string playerName);
        IIpBans GetIpBan(string ip);

    }
}