using System;
using System.Linq;
using Chraft.Net;
using Chraft.Entity;
using Chraft.Net.Packets;
using Chraft.World;
using Chraft.Utils;
using Chraft.Interfaces;
using Chraft.Commands;

namespace Chraft
{
    public partial class Client : EntityBase, IDisposable
    {
        /// <summary>
        /// Invoked whenever the user sends a command.
        /// </summary>
        public event EventHandler<CommandEventArgs> Command;

        /// <summary>
        /// Invoked prior to a chat message transmission.
        /// </summary>
        public event EventHandler<ChatEventArgs> ChatMessage;

        /// <summary>
        /// Gets or sets the first point of the cuboid selection.
        /// </summary>
        public PointI? Point1 { get; set; }

        /// <summary>
        /// Gets or sets the second point of the cuboid selection.
        /// </summary>
        public PointI? Point2 { get; set; }

        /// <summary>
        /// Gets or sets the start of the cuboid selection.
        /// </summary>
        public PointI? SelectionStart
        {
            get
            {
                if (Point1 == null || Point2 == null)
                    return null;
                return new PointI(Point1.Value.X < Point2.Value.X ? Point1.Value.X : Point2.Value.X,
                    Point1.Value.Y < Point2.Value.Y ? Point1.Value.Y : Point2.Value.Y,
                    Point1.Value.Z < Point2.Value.Z ? Point1.Value.Z : Point2.Value.Z);
            }
        }

        /// <summary>
        /// Gets or sets the end of the cuboid selection.
        /// </summary>
        public PointI? SelectionEnd
        {
            get
            {
                if (Point1 == null || Point2 == null)
                    return null;
                return new PointI(Point1.Value.X > Point2.Value.X ? Point1.Value.X : Point2.Value.X,
                    Point1.Value.Y > Point2.Value.Y ? Point1.Value.Y : Point2.Value.Y,
                    Point1.Value.Z > Point2.Value.Z ? Point1.Value.Z : Point2.Value.Z);
            }
        }

        /// <summary>
        /// Send a chat message from the user.
        /// </summary>
        /// <param name="clean">The pre-cleaned message to be sent.</param>
        public void ExecuteChat(string clean)
        {
            if (IsMuted)
            {
                SendMessage("You have been muted");
                return;
            }

            if ((clean = OnChat(clean)) != null)
            {
                Server.Broadcast(Chat.Format(DisplayName, clean));
                Logger.Log(Logger.LogLevel.Info, "{0}: {1}", DisplayName, clean);
            }
        }

        private string OnChat(string message)
        {
            ChatEventArgs e = new ChatEventArgs
            {
                Cancelled = false,
                Message = message
            };
            if (ChatMessage != null)
                ChatMessage.Invoke(this, e);
            return e.Cancelled ? null : e.Message;
        }

        /// <summary>
        /// Execute a command in the context of the user.
        /// </summary>
        /// <param name="command">The command text, with the slash removed.</param>
        public void ExecuteCommand(string command)
        {
            if (!CanUseCommand(command))
            {
                SendMessage("You do not have permission to use that command");
                return;
            }
            Logger.Log(Logger.LogLevel.Info, DisplayName + " issued server command: " + command);
            Server.Broadcast(DisplayName + " executed command " + command, this);
            CommandProc(command, Chat.Tokenize(command));
        }

        private void CommandProc(string raw, string[] tokens)
        {
            ClientCommand cmd;
            try
            {
                cmd = Server.ClientCommandHandler.Find(tokens[0]) as ClientCommand;
            }
            catch (CommandNotFoundException e)
            {
                SendMessage(ChatColor.Red + e.Message);
                return;
            }
            try
            {
                cmd.Use(this, tokens);
            }
            catch(Exception e)
            {
                SendMessage("There was an error while executing the command.");
                Server.Logger.Log(e);
            }
        }

        private void SetCommand(string[] tokens)
        {
            if (Point2 == null || Point1 == null)
            {
                SendMessage("§cPlease select a cuboid first.");
                return;
            }

            PointI start = SelectionStart.Value;
            PointI end = SelectionEnd.Value;

            ItemStack item = Server.Items[tokens[2]];
            if (ItemStack.IsVoid(item))
            {
                SendMessage("§cUnknown item.");
                return;
            }

            if (item.Type > 255)
            {
                SendMessage("§cInvalid item.");
            }

            for (int x = start.X; x <= end.X; x++)
            {
                for (int y = start.Y; y <= end.Y; y++)
                {
                    for (int z = start.Z; z <= end.Z; z++)
                    {
                        World.SetBlockAndData(x, y, z, (byte)item.Type, (byte)item.Durability);
                    }
                }
            }
        }

        private void Pos2Command(string[] tokens)
        {
            Point2 = new PointI((int)Position.X, (int)Position.Y, (int)Position.Z);
            SendMessage("§7First position set.");
        }

        private void Pos1Command(string[] tokens)
        {
            Point1 = new PointI((int)Position.X, (int)Position.Y, (int)Position.Z);
            SendMessage("§7Second position set.");
        }

        private void GiveCommand(string[] tokens)
        {
            if (tokens.Length < 3)
            {
                SendMessage("§cPlease specify an item and target.");
                return;
            }

            ItemStack item = Server.Items[tokens[2]];
            if (ItemStack.IsVoid(item))
            {
                SendMessage("§cUnknown item.");
                return;
            }

            sbyte count = -1;
            if (tokens.Length > 3)
                sbyte.TryParse(tokens[3], out count);

            foreach (Client c in Server.GetClients(tokens[1]))
                c.Inventory.AddItem(item.Type, count < 0 ? item.Count : count, item.Durability);
            SendMessage("§7Item given.");
        }

        private void ItemCommand(string[] tokens)
        {
            if (tokens.Length < 2)
            {
                SendMessage("§cPlease specify an item.");
                return;
            }

            ItemStack item = Server.Items[tokens[1]];
            if (ItemStack.IsVoid(item))
            {
                SendMessage("§cUnknown item.");
                return;
            }

            sbyte count = -1;
            if (tokens.Length > 1)
                sbyte.TryParse(tokens[2], out count);

            Inventory.AddItem(item.Type, count < 0 ? item.Count : count, item.Durability);
            SendMessage("§7Item given.");
        }

        private void TpCommand(string[] tokens)
        {
            if (tokens.Length < 2)
            {
                SendMessage("§cPlease specify a target.");
                return;
            }
            Client[] targets = Server.GetClients(tokens[1]).ToArray();
            if (targets.Length < 1)
            {
                SendMessage("§cUnknown player.");
                return;
            }
            World = targets[0].World;
            TeleportTo(targets[0].Position.X, targets[0].Position.Y, targets[0].Position.Z);
        }

        private void TphereCommand(string[] tokens)
        {
            if (tokens.Length < 2)
            {
                SendMessage("§cPlease specify a target.");
                return;
            }
            Client[] targets = Server.GetClients(tokens[1]).ToArray();
            if (targets.Length < 1)
            {
                SendMessage("§cUnknown payer.");
                return;
            }
            foreach (Client c in targets)
            {
                c.World = World;
                c.TeleportTo(Position.X, Position.Y, Position.Z);
            }
        }

        private void StopCommand(string[] tokens)
        {
            Server.Stop();
        }

        private void SpawnCommand(string[] tokens)
        {
            TeleportTo(World.Spawn.X, World.Spawn.Y, World.Spawn.Z);
        }

        private void ListCommand(string[] tokens)
        {
            SendMessage("Online Players: " + Server.Clients.Count);
            foreach (Client c in Server.Clients.Values)
                SendMessage(c.EntityId + " : " + c.DisplayName);
        }

        private void TimeCommand(string[] tokens)
        {
            int newTime = -1;
            if (tokens.Length < 2)
            {
                SendMessage("You must specify an explicit time, day, or night.");
                return;
            }
            if (int.TryParse(tokens[1], out newTime) && newTime >= 0 && newTime <= 24000)
            {
                World.Time = newTime;
            }
            else if (tokens[1].ToLower() == "day")
            {
                World.Time = 0;
            }
            else if (tokens[1].ToLower() == "night")
            {
                World.Time = 12000;
            }
            else
            {
                SendMessage("You must specify a time value between 0 and 24000");
                return;
            }
            Server.Broadcast(new TimeUpdatePacket { Time = World.Time });
        }

        private void MuteCommand(string[] tokens)
        {
            if (tokens.Length < 2)
            {
                SendMessage("You must specify a player to mute");
                return;
            }

            Client[] client = Server.GetClients(tokens[1]).ToArray();
            if (client.Length < 1)
            {
                SendMessage("Unknown Player");
                return;
            }
            bool clientMuted = client[0].IsMuted;
            client[0].IsMuted = !clientMuted;
            client[0].SendMessage(clientMuted ? "You have been unmuted" : "You have been muted");
            SendMessage(clientMuted ? tokens[1] + " has been unmuted" : tokens[1] + " has been muted");
        }
    }
}
