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
namespace Chraft.Net
{
    public enum PacketType : byte
    {
        KeepAlive = 0x00,                   // c <-> s
        LoginRequest = 0x01,                //   <->
        Handshake = 0x02,                   //   <->
        ChatMessage = 0x03,                 //   <->
        TimeUpdate = 0x04,                  //   <--
        EntityEquipment = 0x05,             //   <->
        SpawnPosition = 0x06,               //   <--
        UseEntity = 0x07,                   //   -->
        UpdateHealth = 0x08,                //   <--
        Respawn = 0x09,                     //   <->
        Player = 0x0A,                      //   -->
        PlayerPosition = 0x0B,              //   -->
        PlayerRotation = 0x0C,              //   -->
        PlayerPositionRotation = 0x0D,      //   -->
        PlayerDigging = 0x0E,               //   -->
        PlayerBlockPlacement = 0x0F,        //   -->
        HoldingChange = 0x10,               //   <->
        UseBed = 0x11,                      //   <->
        Animation = 0x12,                   //   <->
        EntityAction = 0x13,                //   <--
        NamedEntitySpawn = 0x14,            //   <--
        CollectItem = 0x16,                 //   <--
        AddObjectVehicle = 0x17,            //   <--
        MobSpawn = 0x18,                    //   <--
        EntityPainting = 0x19,              //   <--
        ExperienceOrb = 0x1A,               //   <--
        EntityVelocity = 0x1C,              //   <--
        DestroyEntity = 0x1D,               //   <--
        Entity = 0x1E,                      //   <--
        EntityRelativeMove = 0x1F,          //   <--
        EntityLook = 0x20,                  //   <--
        EntityLookAndRelativeMove = 0x21,   //   <--
        EntityTeleport = 0x22,              //   <--
        EntityHeadLook = 0x23,              //   <--
        EntityStatus = 0x26,                //   <--
        AttachEntity = 0x27,                //   <--
        EntityMetadata = 0x28,              //   <--
        EntityEffect = 0x29,                //   <->
        RemoveEntityEffect = 0x2A,          //   <->
        Experience = 0x2B,                  //   <--
        MapChunk = 0x33,                    //   <--
        MultiBlockChange = 0x34,            //   <--
        BlockChange = 0x35,                 //   <--
        BlockAction = 0x36,                 //   <--
        MapChunkBulk = 0x38,                //   <--
        Explosion = 0x3C,                   //   <--
        SoundOrParticleEffect = 0x3D,       //   <--
        Particle = 0x3F,                    //   <--
        NewInvalidState = 0x46,             //   ???
        GlobalEntityPacket = 0x47,          //   <--
        OpenWindow = 0x64,                  //   <--
        CloseWindow = 0x65,                 //   <--
        WindowClick = 0x66,                 //   -->
        SetSlot = 0x67,                     //   <--
        WindowItems = 0x68,                 //   <--
        UpdateWindowProperty = 0x69,        //   <--
        Transaction = 0x6A,                 //   <->
        CreativeInventoryAction = 0x6B,     //   <--
        EnchantItem = 0x6C,                 //   -->
        UpdateSign = 0x82,                  //   <->
        ItemData = 0x83,                    //   -->
        UpdateTileEntity = 0x84,            //   <--
        IncrementStatistic = 0xC8,          //   ???
        PlayerListItem = 0xC9,              //   <--
        PlayerAbilities = 0xCA,             //   <-->
        TabComplete = 0xCB,                 //   <-->
        LocaleAndViewDistance = 0xCC,       //   <--
        ClientStatus = 0xCD,                //   <--
        ScoreBoardObjective = 0xCE,         //   <--
        UpdateScore = 0xCF,                 //   <--
        DisplayScorboard = 0xD0,            //   <--
        Teams = 0xD1,                       //   <--
        PluginMessage = 0xFA,               //   <-->
        EncryptionKeyResponse = 0xFC,       //   <--
        EncryptionKeyRequest = 0xFD,        //   -->
        ServerListPing = 0xFE,              //   -->
        Disconnect = 0xFF                   //   <->
    }
}
