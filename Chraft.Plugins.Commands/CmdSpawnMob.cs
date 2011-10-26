using System;
using Chraft.Commands;
using Chraft.Entity;
using Chraft.Net;
using Chraft.World;

namespace Chraft.Plugins.Commands
{
    public class CmdSpawnMob : IClientCommand
    {
        public CmdSpawnMob(IPlugin plugin)
        {
            Iplugin = plugin;
        }
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            MobType type = MobType.Sheep;
            int amount = 1;
            bool validMob = false;

            if (tokens.Length > 1)
                Int32.TryParse(tokens[1], out amount);

            if (tokens.Length > 0)
            {
                int mobId;
                Int32.TryParse(tokens[0], out mobId);
                string mobName = Enum.GetName(typeof(MobType), mobId);
                if (mobId == 0)
                {
                    if (mobId.ToString() != tokens[0])
                    {
                        Enum.TryParse(tokens[0], true, out type);
                        validMob = true;
                    }
                }
                else if (!string.IsNullOrEmpty(mobName))
                {
                    type = (MobType)Enum.Parse(typeof(MobType), mobName);
                    validMob = true;
                }
            }
            else
                validMob = true;

            if (amount < 1 || !validMob)
            {
                Help(client);
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                client.Owner.World.SpawnMob(UniversalCoords.FromAbsWorld(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z), type);
            }
        }

        public void Help(Client client)
        {
            client.SendMessage("/spawnmob <Mob> [Amount] - Spawns a mob at your position.");
        }

        public string Name
        {
            get { return "spawnmob"; }
            set { }
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
            get { return "chraft.spawnmob"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}

