
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chraft.Properties;
using Chraft.Utils.NBT;

namespace Chraft.Utils
{
    public class Schematic
    {
        /// <summary>
        /// Schematic name: file name without extension
        /// </summary>
        public string SchematicName { get; protected set; }

        /// <summary>
        /// Y-size
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// Z-size
        /// </summary>
        public int Length { get; protected set; }

        /// <summary>
        /// X-size
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// Level format - Alpha or Classic
        /// </summary>
        public string Level { get; protected set; }

        public byte[] BlockIds;
        public byte[] BlockMetas;

        /// <summary>
        /// Schematic file constructor
        /// </summary>
        /// <param name="schematicName">Schematic name (without extension)</param>
        public Schematic(string schematicName)
        {
            SchematicName = schematicName;
        }

        public void Reset()
        {
            Height = 0;
            Length = 0;
            Width = 0;
            Level = string.Empty;
            BlockIds = null;
            BlockMetas = null;
        }

        public bool Validate(bool headerOnly = false)
        {
            if (Height == 0 || Length == 0 || Width == 0 || string.IsNullOrEmpty(Level))
                return false;
            if (!headerOnly)
            {
                if (BlockIds == null || BlockMetas == null)
                    return false;
                int blocks = Height*Width*Length;
                if (BlockIds.Length != blocks || BlockMetas.Length != blocks)
                    return false;
            }
            return true;
        }

        public bool LoadFromFile(bool headerOnly = false)
        {
            Reset();
            NBTFile nbtFile = null;
            FileStream stream = null;
            string fileName = Path.Combine(Settings.Default.SchematicsFolder, SchematicName + ".schematic");
            if (!File.Exists(fileName))
                return false;

            try
            {
                stream = new FileStream(fileName, FileMode.Open);
                nbtFile = NBTFile.OpenFile(stream, 1);
                foreach (KeyValuePair<string, NBTTag> sa in nbtFile.Contents)
                {
                    switch (sa.Key)
                    {
                        case "Height":
                            Height = sa.Value.Payload;
                            break;
                        case "Length":
                            Length = sa.Value.Payload;
                            break;
                        case "Width":
                            Width = sa.Value.Payload;
                            break;
                        case "Entities":
                        case "TileEntities":
                            break;
                        case "Materials":
                            Level = sa.Value.Payload;
                            break;
                        case "Blocks":
                            if (!headerOnly)
                                BlockIds = sa.Value.Payload;
                            break;
                        case "Data":
                            if (!headerOnly)
                                BlockMetas = sa.Value.Payload;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading schematic file {0}:", SchematicName);
                Console.WriteLine(ex);
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
                if (nbtFile != null)
                    nbtFile.Dispose();
            }

            if (!Validate(headerOnly))
            {
                Reset();
                return false;
            }
            return true;
        }

        public int ToIndex(int x, int y, int z)
        {
            return Width*(y*Length + z) + x;
        }
    }
}
