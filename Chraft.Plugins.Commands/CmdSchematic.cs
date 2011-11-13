#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.IO;
using System.Linq;
using Chraft.Commands;
using Chraft.Net;
using Chraft.Properties;
using Chraft.Utils;
using Chraft.World;

namespace Chraft.Plugins.Commands
{
    public class CmdSchematic : IClientCommand
    {
        public CmdSchematic(IPlugin plugin)
        {
            Iplugin = plugin;
        }

        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            if (tokens.Length < 1)
            {
                Help(client);
                return;
            }

            switch (tokens[0].Trim().ToLower())
            {
                case "list":
                    uint page = 1;
                    if (tokens.Length >= 2 && !UInt32.TryParse(tokens[1], out page))
                        page = 1;
                    List(client, page);
                    return;
                case "place":
                    if (tokens.Length < 2)
                        break;
                    Place(client, tokens);
                    return;
                case "info":
                    if (tokens.Length < 2)
                        break;
                    Info(client, tokens[1]);
                    return;
                    return;
                default:
                    break;
            }
            Help(client);
        }

        protected void List(Client client, uint pageNumber)
        {
            int maxPerPage = 9;
            // 10 max
            if (!Directory.Exists(Settings.Default.SchematicsFolder))
            {
                client.SendMessage("Schematics not found");
                return;
            }
            string[] files = Directory.GetFiles(Settings.Default.SchematicsFolder, "*.schematic");

            if (files.Length == 0)
            {
                client.SendMessage("Schematics not found");
                return;
            }

            int totalPages = (int)((double)files.Length/maxPerPage);
            if (files.Length % maxPerPage != 0)
                totalPages += 1;
            if (pageNumber < 1 || pageNumber > totalPages)
            {
                client.SendMessage("Please specify the page number between 1 and " + totalPages);
                return;
            }
            
            client.SendMessage(string.Format("Schematics [{0}/{1}]:", pageNumber, totalPages));
            int startIndex = (int)(maxPerPage * (pageNumber - 1));
            int lastIndex = (pageNumber == totalPages ? files.Length : (int)(maxPerPage * pageNumber));
            for (int i = startIndex; i < lastIndex; i++)
            {
                string schematicName = files[i].Replace(".schematic", "").Replace(Settings.Default.SchematicsFolder + Path.DirectorySeparatorChar, "");
                client.SendMessage(string.Format("{0}: {1}", (i + 1), schematicName));
            }
        }

        protected void Place(Client client, string[] tokens)
        {
            string schematicName = tokens[1];
            Schematic schematic = new Schematic(schematicName);
            if (!schematic.LoadFromFile())
            {
                client.SendMessage("Can not load schematic file");
                return;
            }

            bool rotateByX = false;
            bool rotateByZ = false;
            bool rotateByXZ = false;

            if (tokens.Length >= 3)
            {
                string rotation = tokens[2].Trim().ToLower();
                if (rotation == "x")
                    rotateByX = true;
                else if (rotation == "z")
                    rotateByZ = true;
                else if (rotation == "xz")
                    rotateByXZ = true;
            }

            UniversalCoords coords = UniversalCoords.FromAbsWorld(client.Owner.Position);
            int width = ((rotateByX || rotateByXZ) ? -1*schematic.Width : schematic.Width);
            int length = ((rotateByZ || rotateByXZ) ? -1*schematic.Length : schematic.Length);

            if (!RequiredChunksExist(client.Owner.World, coords, width, schematic.Height, length))
            {
                client.SendMessage("The schematic is too big - required chunks are not loaded/created yet");
                return;
            }

            for (int dx = 0; dx < schematic.Width; dx++)
                for (int dy = 0; dy < schematic.Height; dy++)
                    for (int dz = 0; dz < schematic.Length; dz++)
                    {
                        int x = coords.WorldX + ((rotateByX || rotateByXZ) ? -dx : dx);
                        int y = coords.WorldY + dy;
                        int z = coords.WorldZ + ((rotateByZ || rotateByXZ) ? -dz : dz);
                        client.Owner.World.SetBlockAndData(x, y, z, schematic.BlockIds[schematic.ToIndex(dx, dy, dz)], schematic.BlockMetas[schematic.ToIndex(dx, dy, dz)]);
                    }
            client.SendMessage(string.Format("Schematic {0} ({1} blocks) has been loaded", schematic.SchematicName, schematic.Width * schematic.Height * schematic.Length));
        }

        protected bool RequiredChunksExist(WorldManager world, UniversalCoords startingPoint, int xSize, int ySize, int zSize)
        {
            if (startingPoint.WorldY + ySize > 127)
                return false;
            UniversalCoords endPoint = UniversalCoords.FromWorld(startingPoint.WorldX + xSize, startingPoint.WorldY, startingPoint.WorldZ + zSize);
            if (startingPoint.ChunkX == endPoint.ChunkX && startingPoint.ChunkZ == endPoint.ChunkZ)
                return true;
            for (int x = startingPoint.ChunkX; x <= endPoint.ChunkX; x++)
                for (int z = startingPoint.ChunkZ; z <= endPoint.ChunkZ; z++)
                    if (world.GetChunkFromChunkSync(x, z) == null)
                        return false;
            return true;
        }

        protected void Info(Client client, string schematicName)
        {
            Schematic schematic = new Schematic(schematicName);
            if (!schematic.LoadFromFile(true))
            {
                client.SendMessage("Can not load schematic file");
                return;
            }
            client.SendMessage(string.Format("Width(X) x Height(Y) x Length(Z): {0} x {1} x {2} ({3} blocks)", schematic.Width, schematic.Height, schematic.Length, (schematic.Width*schematic.Height*schematic.Length)));
        }

        public void Help(Client client)
        {
            client.SendMessage("/schematic list [pageNumber] -");
            client.SendMessage("    display a list of available schematics");
            client.SendMessage("/schematic place <schematic name> [x|z|xz] -");
            client.SendMessage("    place the specified schematic at current position");
            client.SendMessage("    and rotate it by X, Z or X & Z axis (optional)");
            client.SendMessage("/schematic info <schematic name> -");
            client.SendMessage("    display the info about specified schematic");
        }

        public string Name
        {
            get { return "schematic"; }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Build; }
        }

        public string Permission
        {
            get { return "chraft.schematic"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}
