using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Server;
using Chraft.Utilities.Coords;

namespace Chraft.PluginSystem.Net
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
