using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity.Mobs
{
    public class Sheep: Mob
    {
        public override string Name
        {
            get { return "Sheep"; }
        }

        public override short AttackStrength
        {
            get
            {
                return 0;
            }
        }

        internal Sheep(Chraft.World.WorldManager world, int entityId, Chraft.Net.MetaData data = null)
            : base(world, entityId, MobType.Sheep, data)
        {
        }

        protected Chraft.Net.MetaData.Dyes WoolColorToDyeColor()
        {
            switch (this.Data.WoolColor)
            {
                case WoolColor.Black:
                    return Chraft.Net.MetaData.Dyes.InkSack;
                case WoolColor.Blue:
                    return Chraft.Net.MetaData.Dyes.LapisLazuli;
                case WoolColor.Brown:
                    return Chraft.Net.MetaData.Dyes.CocoBeans;
                case WoolColor.Green:
                    return Chraft.Net.MetaData.Dyes.CactusGreen;
                case WoolColor.Cyan:
                    return Chraft.Net.MetaData.Dyes.Cyan;
                case WoolColor.Gray:
                    return Chraft.Net.MetaData.Dyes.Gray;
                case WoolColor.LightBlue:
                    return Chraft.Net.MetaData.Dyes.LightBlue;
                case WoolColor.Lime:
                    return Chraft.Net.MetaData.Dyes.Lime;
                case WoolColor.Magenta:
                    return Chraft.Net.MetaData.Dyes.Magenta;
                case WoolColor.Orange:
                    return Chraft.Net.MetaData.Dyes.Orange;
                case WoolColor.Pink:
                    return Chraft.Net.MetaData.Dyes.Pink;
                case WoolColor.Purple:
                    return Chraft.Net.MetaData.Dyes.Purple;
                case WoolColor.Red:
                    return Chraft.Net.MetaData.Dyes.RoseRed;
                case WoolColor.Silver:
                    return Chraft.Net.MetaData.Dyes.LightGray;
                case WoolColor.White:
                    return Chraft.Net.MetaData.Dyes.BoneMeal;
                case WoolColor.Yellow:
                    return Chraft.Net.MetaData.Dyes.DandelionYellow;
            }
            return Chraft.Net.MetaData.Dyes.BoneMeal;
        }

        protected override void DoInteraction(Client client, Interfaces.ItemStack item)
        {
            base.DoInteraction(client, item);

            if (client != null && !Chraft.Interfaces.ItemStack.IsVoid(item))
            {
                if (item.Type == (short)Chraft.World.BlockData.Items.Shears)
                {
                    // Drop wool when sheared
                    sbyte count = (sbyte)Server.Rand.Next(2, 4);
                    if (count > 0)
                        Server.DropItem(World, (int)this.Position.X, (int)this.Position.Y, (int)this.Position.Z, new Interfaces.ItemStack((short)Chraft.World.BlockData.Blocks.Wool, count, (short)this.WoolColorToDyeColor()));
                }
                else if (item.Type == (short)Chraft.World.BlockData.Items.Ink_Sack)
                {
                    // Set the wool colour of this Sheep based on the item.Durability
                    // TODO: (Chraft.Net.MetaData.Dyes)item.Durability
                }
            }
        }

        protected override void DoDeath()
        {
            Server.DropItem(World, (int)this.Position.X, (int)this.Position.Y, (int)this.Position.Z, new Interfaces.ItemStack((short)Chraft.World.BlockData.Blocks.Wool, 1, (short)this.WoolColorToDyeColor()));
        }
    }
}
