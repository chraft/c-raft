using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Chraft.Net;

namespace Chraft.Commands
{
    public class CmdStop : ServerCommand, ClientCommand
    {
        public ServerCommandHandler ServerCommandHandler { get; set; }
        public ClientCommandHandler ClientCommandHandler { get; set; }
        public string Name
        {
            get { return "stop"; }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public string Permission
        {
            get { return "chraft.stop"; }
        }

        public void Use(Server server, string[] tokens)
        {
            server.Broadcast("The server is shutting down.");
            server.Logger.Log(Logger.LogLevel.Info, "The server is shutting down.");
            foreach (var client in server.GetClients())
            {
                client.Kick("Server is shutting down");
            }
            Thread.Sleep(5000);
            server.Stop();
            Thread.Sleep(10);
            Console.WriteLine("Press Enter to exit.");
        }

        public void Help(Server server)
        {
            server.Logger.Log(Logger.LogLevel.Info, "Shuts down the server.");
        }
        public void Use(Client client, string[] tokens)
        {
            client.Owner.Server.Broadcast("The server is shutting down.");
            client.Owner.Server.Logger.Log(Logger.LogLevel.Info, "The server is shutting down.");
            foreach (var cl in client.Owner.Server.GetClients())
            {
                cl.Kick("Server is shutting down");
            }
            Thread.Sleep(5000);
            client.Owner.Server.Stop();
            Thread.Sleep(10);
            Console.WriteLine("Press Enter to exit.");
        }

        public void Help(Client client)
        {
            client.SendMessage("Shuts down the server.");
        }
    }
}
