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
                var mob = MobFactory.CreateMob(client.Owner.World, client.Server.AllocateEntity(), type, null);
                mob.Position = client.Owner.Position;
                
                //Event
                Chraft.Plugins.Events.Args.EntitySpawnEventArgs e = new Chraft.Plugins.Events.Args.EntitySpawnEventArgs(mob, mob.Position);
                client.Server.PluginManager.CallEvent(Plugins.Events.Event.EntitySpawn, e);
                if (e.EventCanceled)
                    continue;
                mob.Position = e.Location;
                //End Event
                
                client.Server.AddEntity(mob);
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

