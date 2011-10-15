using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Net;
using Chraft.World;

namespace Chraft.Commands
{
    public class CmdSpawnMob : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            MobType type;
            int amount = 1;
            if(tokens.Length > 1)amount = parseint(tokens[1]);
            try
            {
                type = (MobType) Enum.Parse(typeof(MobType), tokens[0], true);
            }
            catch (Exception e) { client.Logger.Log(e); type = MobType.Sheep; }
            for (int i = 0; i < amount; i++)
            {
                client.Owner.World.SpawnMob(UniversalCoords.FromAbsWorld(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z), type);
            }
        }
        private int parseint(string s)
        {
            int r;
            try
            {
                r = int.Parse(s);
            }
            catch { r = 1; }
            return r;
        }
        public void Help(Client client)
        {
            client.SendMessage("/spawnmob <Mob> [Amount] - Spawns a mob at your position.");
        }

        public string Name
        {
            get { return "spawnmob"; }
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
    }
}
