using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.Plugins;
using Chraft.Utils;

namespace Chraft.Commands.Debug
{
    public class DbgPos : IClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            if (tokens.Length == 0)
            {
                client.SendMessage(String.Format("§7Your position: X={0:0.00},Y={1:0.00},Z={2:0.00}, Yaw={3:0.00}, Pitch={4:0.00}", client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z, client.Owner.Yaw, client.Owner.Pitch));
            }
            else if (tokens[0] == "yaw")
            {
                Vector3 z1 = client.Owner.Position.ToVector() + Vector3.ZAxis;
                Vector3 posToZ1 = (client.Owner.Position.ToVector() - z1);

                client.SendMessage(String.Format("§7Player.Position.Yaw {0:0.00}, vector computed yaw (SignedAngle) {1:0.00}", client.Owner.Yaw % 360, Vector3.ZAxis.SignedAngle(Vector3.ZAxis.Yaw(client.Owner.Yaw.ToRadians()), Vector3.ZAxis.Yaw(client.Owner.Yaw.ToRadians()).Yaw(90.0.ToRadians())).ToDegrees()));
                client.SendMessage(String.Format("§7Normalised facing Yaw: " + new Vector3(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z).Normalize().Yaw(client.Owner.Yaw % 360).ToString()));
            }
        }

        public void Help(Client client)
        {

        }

        public string Name
        {
            get { return "dbgpos"; }
        }

        public string Shortcut
        {
            get { return String.Empty; }
        }

        public CommandType Type
        {
            get { return CommandType.Information; }
        }

        public string Permission
        {
            get { return "chraft.debug"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}
