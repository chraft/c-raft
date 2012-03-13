using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utilities;

namespace Chraft.PluginSystem
{
    public interface IClient
    {
        UniversalCoords? Point1 { get; set; }

        UniversalCoords? Point2 { get; set; }

        UniversalCoords? SelectionStart { get; }

        UniversalCoords? SelectionEnd { get; }

        string Username { get; }

        int Ping { get; }
        bool OnGround { get; }
        double Stance { get; }

        IPlayer GetOwner();
        IServer GetServer();
        bool CheckUsername(string username);

        void Kick(string reason);

        void MarkToDispose();

        void SendMessage(string message);

        void SendInitialTime(bool async = true);

        ILogger GetLogger();
    }
}
