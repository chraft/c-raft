
namespace Chraft.Net
{
    public delegate void OnPacketReceive(Client client, PacketReader pvSrc);
    public class PacketHandler
    {
        private int _Length;
        private int _MinimumLength;
        private OnPacketReceive _OnReceive;
        private PacketType _PacketId;

        public PacketType PacketId
        {
            get { return _PacketId; }
        }

        public int Length
        {
            get { return _Length; }
        }

        public int MinimumLength
        {
            get { return _MinimumLength; }
        }

        public OnPacketReceive OnReceive
        {
            get { return _OnReceive; }
        }

        public PacketHandler(PacketType type, int length, int minimumLength, OnPacketReceive onReceive)
        {
            _PacketId = type;
            _Length = length;
            _MinimumLength = minimumLength;
            _OnReceive = onReceive;
        }
    }
}