using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utils;

namespace Chraft.Commands.Debug
{
    public class DbgPos : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            if (tokens.Length == 1)
            {
                client.SendMessage(String.Format("§7Your position: X={0:0.00},Y={1:0.00},Z={2:0.00}, Yaw={3:0.00}, Pitch={4:0.00}", client.Position.X, client.Position.Y, client.Position.Z, client.Position.Yaw, client.Position.Pitch));
            }
            else if (tokens[1] == "yaw")
            {
                Vector3 z1 = client.Position.Vector + Vector3.ZAxis;
                Vector3 posToZ1 = (client.Position.Vector - z1);

                client.SendMessage(String.Format("§7Player.Position.Yaw {0:0.00}, vector computed yaw (SignedAngle) {1:0.00}", client.Position.Yaw % 360, Vector3.ZAxis.SignedAngle(Vector3.ZAxis.Yaw(client.Position.Yaw.ToRadians()), Vector3.ZAxis.Yaw(client.Position.Yaw.ToRadians()).Yaw(90.0.ToRadians())).ToDegrees()));
                client.SendMessage(String.Format("§7Normalised facing Yaw: " + client.Position.Vector.Normalize().Yaw(client.Position.Yaw % 360).ToString()));
            }
        }

        public void Help(Client client)
        {
            throw new NotImplementedException();
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
    }
}
