using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        PickupSpawn = 0x15,                 //   <->
        CollectItem = 0x16,                 //   <--
        AddObjectVehicle = 0x17,            //   <--
        MobSpawn = 0x18,                    //   <--
        EntityPainting = 0x19,              //   <--
        UnknownA = 0x1B,                    //   ???
        EntityVelocity = 0x1C,              //   <--
        DestroyEntity = 0x1D,               //   <--
        Entity = 0x1E,                      //   <--
        EntityRelativeMove = 0x1F,          //   <--
        EntityLook = 0x20,                  //   <--
        EntityLookAndRelativeMove = 0x21,   //   <--
        EntityTeleport = 0x22,              //   <--
        EntityStatus = 0x26,                //   <--
        AttachEntity = 0x27,                //   <--
        EntityMetadata = 0x28,              //   <--
        PreChunk = 0x32,                    //   <--
        MapChunk = 0x33,                    //   <--
        MultiBlockChange = 0x34,            //   <--
        BlockChange = 0x35,                 //   <--
        PlayNoteBlock = 0x36,               //   <--
        Explosion = 0x3C,                   //   <--
        OpenWindow = 0x64,                  //   <--
        CloseWindow = 0x65,                 //   <--
        WindowClick = 0x66,                 //   -->
        SetSlot = 0x67,                     //   <--
        WindowItems = 0x68,                 //   <--
        UpdateProgressBar = 0x69,           //   <--
        Transaction = 0x6A,                 //   <->
        UpdateSign = 0x82,                  //   <->
        MapData = 0x83,                     //   -->
        Disconnect = 0xFF                   //   <->
	}
}
