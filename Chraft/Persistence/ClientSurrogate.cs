using System;
using Chraft.Interfaces;

namespace Chraft.Persistence
{
    [Serializable]
    public sealed class ClientSurrogate
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Yaw { get; set; }
        public double Pitch { get; set; }
        public short Health { get; set; }
        public short Food { get; set; }
        public float FoodSaturation { get; set; }
        public Inventory Inventory { get; set; }
        public byte GameMode { get; set; }
        public string DisplayName { get; set; }
    }
}
