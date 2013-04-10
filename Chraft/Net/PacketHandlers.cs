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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using Chraft.Net.Packets;

namespace Chraft.Net
{
    public class PacketHandlers
    {
        private static PacketHandler[] m_Handlers;

        public static PacketHandler[] Handlers
        {
            get { return m_Handlers; }
        }

        static PacketHandlers()
        {
            m_Handlers = new PacketHandler[0x100];

            Register(PacketType.KeepAlive, 5, 0, ReadKeepAlive);
            Register(PacketType.Handshake, 0, 10, ReadHandshake);
            Register(PacketType.ChatMessage, 0, 3, ReadChatMessage);
            Register(PacketType.UseEntity, 10, 0, ReadUseEntity);
            Register(PacketType.Respawn, 14, 0, ReadRespawn);
            Register(PacketType.Player, 2, 0, ReadPlayer);
            Register(PacketType.PlayerPosition, 34, 0, ReadPlayerPosition);
            Register(PacketType.PlayerRotation, 10, 0, ReadPlayerRotation);
            Register(PacketType.PlayerPositionRotation, 42, 0, ReadPlayerPositionRotation);
            Register(PacketType.PlayerDigging, 12, 0, ReadPlayerDigging);
            Register(PacketType.PlayerBlockPlacement, 0, 13, ReadPlayerBlockPlacement);
            Register(PacketType.HoldingChange, 3, 0, ReadHoldingChange);
            Register(PacketType.Animation, 6, 0, ReadAnimation);
            Register(PacketType.EntityAction, 6, 0, ReadEntityAction);
            Register(PacketType.CloseWindow, 2, 0, ReadCloseWindow);
            Register(PacketType.WindowClick, 0, 10, ReadWindowClick);
            Register(PacketType.CreativeInventoryAction, 0, 5, ReadCreativeInventoryAction);
            Register(PacketType.ServerListPing, 2, 0, ReadServerListPing);
            Register(PacketType.Disconnect, 0, 3, ReadDisconnect);
            Register(PacketType.Transaction, 5 , 0, ReadTransaction);
            Register(PacketType.UpdateSign, 0, 11, ReadUpdateSign);
            Register(PacketType.EnchantItem, 3, 0, ReadEnchantItem);
            Register(PacketType.PlayerAbilities, 4, 0, ReadPlayerAbilities); 
            Register(PacketType.LocaleAndViewDistance, 0, 6, ReadLocaleAndViewDistance);
            Register(PacketType.ClientStatus, 2, 0, ReadClientStatus);
            Register(PacketType.EncryptionKeyResponse, 0, 5, ReadEncryptionResponse);
            Register(PacketType.TabComplete, 0, 3, ReadTabCompletePacket);
        }

        public static void Register(PacketType packetID, int length, int minimumLength, OnPacketReceive onReceive)
        {
            m_Handlers[(byte)packetID] = new PacketHandler(packetID, length, minimumLength, onReceive);
        }

        public static PacketHandler GetHandler(PacketType packetID)
        {
            return m_Handlers[(byte)packetID];
        }

        public static void ReadKeepAlive(Client client, PacketReader reader)
        {
            KeepAlivePacket ka = new KeepAlivePacket();
            ka.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketKeepAlive(client, ka);
        }

        public static void ReadHandshake(Client client, PacketReader reader)
        {
            HandshakePacket hp = new HandshakePacket();
            hp.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketHandshake(client, hp);
        }

        public static void ReadChatMessage(Client client, PacketReader reader)
        {
            ChatMessagePacket cm = new ChatMessagePacket();
            cm.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketChatMessage(client, cm);
        }

        public static void ReadUseEntity(Client client, PacketReader reader)
        {
            UseEntityPacket ue = new UseEntityPacket();
            ue.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketUseEntity(client, ue);
        }

        public static void ReadRespawn(Client client, PacketReader reader)
        {
            RespawnPacket rp = new RespawnPacket();
            rp.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketRespawn(client, rp);
        }

        public static void ReadPlayer(Client client, PacketReader reader)
        {
            PlayerPacket pp = new PlayerPacket();
            pp.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketPlayer(client, pp);
        }

        public static void ReadPlayerPosition(Client client, PacketReader reader)
        {
            PlayerPositionPacket pp = new PlayerPositionPacket();
            pp.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketPlayerPosition(client, pp);
        }

        public static void ReadPlayerRotation(Client client, PacketReader reader)
        {
            PlayerRotationPacket pr = new PlayerRotationPacket();
            pr.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketPlayerRotation(client, pr);
        }

        public static void ReadPlayerPositionRotation(Client client, PacketReader reader)
        {
            PlayerPositionRotationPacket ppr = new PlayerPositionRotationPacket();
            ppr.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketPlayerPositionRotation(client, ppr);
        }

        public static void ReadPlayerDigging(Client client, PacketReader reader)
        {
            PlayerDiggingPacket pd = new PlayerDiggingPacket();
            pd.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketPlayerDigging(client, pd);
        }

        public static void ReadPlayerBlockPlacement(Client client, PacketReader reader)
        {
            PlayerBlockPlacementPacket pb = new PlayerBlockPlacementPacket();
            pb.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketPlayerBlockPlacement(client, pb);
        }

        public static void ReadHoldingChange(Client client, PacketReader reader)
        {
            HoldingChangePacket hc = new HoldingChangePacket();
            hc.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketHoldingChange(client, hc);
        }

        public static void ReadAnimation(Client client, PacketReader reader)
        {
            AnimationPacket ap = new AnimationPacket();
            ap.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketAnimation(client, ap);
        }

        public static void ReadEntityAction(Client client, PacketReader reader)
        {
            EntityActionPacket ea = new EntityActionPacket();
            ea.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketEntityAction(client, ea);
        }

        public static void ReadCloseWindow(Client client, PacketReader reader)
        {
            CloseWindowPacket cw = new CloseWindowPacket();
            cw.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketCloseWindow(client, cw);
        }

        public static void ReadWindowClick(Client client, PacketReader reader)
        {
            WindowClickPacket wc = new WindowClickPacket();
            wc.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketWindowClick(client, wc);
        }

        public static void ReadServerListPing(Client client, PacketReader reader)
        {
            ServerListPingPacket sl = new ServerListPingPacket();
            sl.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketServerListPing(client, sl);
        }

        public static void ReadDisconnect(Client client, PacketReader reader)
        {
            DisconnectPacket dp = new DisconnectPacket();
            dp.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketDisconnect(client, dp);
        }

        public static void ReadCreativeInventoryAction(Client client, PacketReader reader)
        {
            CreativeInventoryActionPacket ci = new CreativeInventoryActionPacket();
            ci.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketCreativeInventoryAction(client, ci);
        }
        public static void ReadTransaction(Client client, PacketReader reader)
        {
            TransactionPacket tp = new TransactionPacket();
            tp.Read(reader);

            if (!reader.Failed)
                Client.HandleTransactionPacket(client, tp);
        }

        public static void ReadUpdateSign(Client client, PacketReader reader)
        {
            UpdateSignPacket us = new UpdateSignPacket();
            us.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketUpdateSign(client, us);
        }

        public static void ReadEnchantItem(Client client, PacketReader reader)
        {
            EnchantItemPacket ei = new EnchantItemPacket();
            ei.Read(reader);

            if(!reader.Failed)
                Client.HandlePacketEnchantItem(client, ei);
        }

        public static void ReadPlayerAbilities(Client client, PacketReader reader)
        {
            PlayerAbilitiesPacket pa = new PlayerAbilitiesPacket();
            pa.Read(reader);

            if (!reader.Failed)
                Client.HandlePacketPlayerActivites(client, pa);
            
        }

        public static void ReadLocaleAndViewDistance(Client client, PacketReader reader)
        {
            ClientSettingsPacket lvd = new ClientSettingsPacket();
            lvd.Read(reader);

            if(!reader.Failed)
                Client.HandlePacketLocaleAndViewDistance(client, lvd);
        }

        public static void ReadClientStatus(Client client, PacketReader reader)
        {
            ClientStatusPacket csp = new ClientStatusPacket();
            csp.Read(reader);

            if(!reader.Failed)
                Client.HandlePacketClientStatus(client, csp);
        }

        public static void ReadEncryptionResponse(Client client, PacketReader reader)
        {
            EncryptionKeyResponse ekr = new EncryptionKeyResponse();
            ekr.Read(reader);

            if(!reader.Failed)
                Client.HandlePacketEncryptionResponse(client, ekr);
        }

        public static void ReadTabCompletePacket(Client client, PacketReader reader)
        {
            TabCompletePacket tcp = new TabCompletePacket();
            tcp.Read(reader);

            if (!reader.Failed)
                Client.HandleTabCompletePacket(client, tcp);
        }
    }
}
