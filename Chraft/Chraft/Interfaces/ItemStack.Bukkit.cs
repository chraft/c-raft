using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.World;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using org.bukkit;
using org.bukkit.inventory;
using org.bukkit.material;

namespace Chraft.Interfaces
{
    [Serializable]
    public class ItemStackBukkit
    {

        private MaterialData data = null;
        private int _type = 0;
        //for serialization

        public int Amount { get; set; }
        public int Type
        {
            get
            { return _type; }
            set
            {
                _type = value;
                createData((byte)0);
            }
        }
        public int Durability { get; set; }
        public Material MaterialType
        {
            get { return Material.getMaterial(Type); }

        }

        public ItemStackBukkit()
        {

        }

        protected ItemStackBukkit(int type, int amount, short damage)
        {
            ItemStack(type, amount, damage, null);
        }

        public ItemStack ItemStack(int type, int amount, short damage, java.lang.Byte data)
        {
            return new ItemStack(type, amount, damage, data);

        }
        private void createData(Byte data)
        {
            Material mat = Material.getMaterial(Type);
            if (mat == null)
            {
                this.data = new MaterialData(Type, data);
            }
            else
            {
                this.data = mat.getNewData(data);
            }
        }
    }
}