using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utils;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.World;

namespace Chraft.Entity.Mobs
{
    public class Sheep: Mob
    {
        public override string Name
        {
            get { return "Sheep"; }
        }

        static ProportionValue<WoolColor>[] _woolColor = new[]{
                ProportionValue.Create(0.8184, WoolColor.White), // 81.84% chance for White
                ProportionValue.Create(0.05, WoolColor.Silver),  // 5% chance for light gray
                ProportionValue.Create(0.05, WoolColor.Gray),    // 5% chance for gray
                ProportionValue.Create(0.05, WoolColor.Black),   // 5% chance for black
                ProportionValue.Create(0.03, WoolColor.Brown),   // 3% chance for brown
                ProportionValue.Create(0.0016, WoolColor.Pink),  // 0.16% chance for pink
            };

        internal Sheep(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Sheep, data)
        {
            this.Data.Sheared = false;
            this.Data.WoolColor = _woolColor.ChooseByRandom();
        }

        protected WoolColor DyeColorToWoolColor(MetaData.Dyes dyeColor)
        {
            switch (dyeColor)
            {
                case MetaData.Dyes.InkSack:
                    return WoolColor.Black;
                case MetaData.Dyes.LapisLazuli:
                    return WoolColor.Blue;
                case MetaData.Dyes.CocoBeans:
                    return WoolColor.Brown;
                case MetaData.Dyes.CactusGreen:
                    return WoolColor.Green;
                case MetaData.Dyes.Cyan:
                    return WoolColor.Cyan;
                case MetaData.Dyes.Gray:
                    return WoolColor.Gray;
                case MetaData.Dyes.LightBlue:
                    return WoolColor.LightBlue;
                case MetaData.Dyes.Lime:
                    return WoolColor.Lime;
                case MetaData.Dyes.Magenta:
                    return WoolColor.Magenta;
                case MetaData.Dyes.Orange:
                    return WoolColor.Orange;
                case MetaData.Dyes.Pink:
                    return WoolColor.Pink;
                case MetaData.Dyes.Purple:
                    return WoolColor.Purple;
                case MetaData.Dyes.RoseRed:
                    return WoolColor.Red;
                case MetaData.Dyes.LightGray:
                    return WoolColor.Silver;
                case MetaData.Dyes.BoneMeal:
                    return WoolColor.White;
                case MetaData.Dyes.DandelionYellow:
                    return WoolColor.Yellow;
            }
            return WoolColor.White;
        }

        protected override void DoInteraction(Client client, Interfaces.ItemStack item)
        {
            base.DoInteraction(client, item);

            if (client != null && !Chraft.Interfaces.ItemStack.IsVoid(item))
            {
                if (item.Type == (short)Chraft.World.BlockData.Items.Shears && !Data.Sheared)
                {
                    // Drop wool when sheared
                    sbyte count = (sbyte)Server.Rand.Next(2, 4);
                    if (count > 0)
                        Server.DropItem(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), new Interfaces.ItemStack((short)Chraft.World.BlockData.Blocks.Wool, count, (short)Data.WoolColor));
                    Data.Sheared = true;

                    SendMetadataUpdate();
                }
                else if (item.Type == (short)Chraft.World.BlockData.Items.Ink_Sack)
                {
                    // Set the wool colour of this Sheep based on the item.Durability
                    // Safety check. Values of 16 and higher (color do not exist) may crash the client v1.8.1 and below
                    if (item.Durability >= 0 && item.Durability <= 15)
                    {
                        //this.Data.WoolColor = (WoolColor)Enum.ToObject(typeof(WoolColor), (15 - item.Durability));
                        Data.WoolColor = DyeColorToWoolColor((MetaData.Dyes)Enum.ToObject(typeof(MetaData.Dyes), item.Durability));
                        SendMetadataUpdate();
                    }
                }
            }
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            if (!this.Data.Sheared)
                Server.DropItem(World, UniversalCoords.FromAbsWorld(this.Position.X, this.Position.Y, this.Position.Z), new Interfaces.ItemStack((short)Chraft.World.BlockData.Blocks.Wool, 1, (short)this.Data.WoolColor));
        }
    }
}
