
﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using System.Diagnostics;
using CollisionManager;

namespace OmegaRace
{

    public enum QueueType
    {
        SHIP_RS,            // remote to server Ship action state
        PHYSICS_SR,         // server to remote Ship pos & rot
        COL_EVENT_SR,       // server to remote collision event
    }

    public class PhysicsBuffer
    {
        public int id;
        public float rot;
        public Vector2 pos;
    }

    abstract public class Message
    {
        public int inSeqNum;
        public int outSeqNum;
        public QueueType type;

        abstract public void execute();

        abstract public QueueType getQueueType();
    }

    public class Ship_RS : Message
    {
        public PlayerID playerId;
        public float rotation;
        public float impulse;
        public bool missle;
        public bool bomb;

        public Ship_RS(PlayerID _id, float _rot, float _imp, bool _missle, bool _bomb)
        {
           playerId = _id;
           rotation = _rot;
           impulse = _imp;
           missle = _missle;
           bomb = _bomb;
        }

        public override QueueType getQueueType()
        {
            return QueueType.SHIP_RS;
        }

        public override void execute()
        {
            Player player = PlayerManager.Instance().getPlayer(this.playerId);
            if (rotation != 0.0f)
            {
                player.playerShip.physicsObj.body.Rotation += this.rotation;
            }
            if (impulse > 0.0f)
            {
                Vector2 direction = new Vector2((float)(Math.Cos(player.playerShip.physicsObj.body.GetAngle())), (float)(Math.Sin(player.playerShip.physicsObj.body.GetAngle())));
                direction.Normalize();
                float MaxSpeed = GameObject.MaxSpeed;
                direction *= MaxSpeed;
                player.playerShip.physicsObj.body.ApplyLinearImpulse(direction, player.playerShip.physicsObj.body.GetWorldCenter());
                Vector2 velocity = player.playerShip.physicsObj.body.GetLinearVelocity();
                if (velocity.Length() > MaxSpeed)
                    player.playerShip.physicsObj.body.SetLinearVelocity((MaxSpeed / velocity.Length() * velocity));
            }
            if (missle)
            {
                player.createMissile();
            }
            if (bomb)
            {
                GameObjManager.Instance().createBomb(player.id);
            }
        }
    }

    class Physics_SR : Message
    {
        //How may physics triggered
        public int count;
        public PhysicsBuffer[] pBuffer;
        public static Physics_SR pBufferGlobal = null;

        public Physics_SR(Physics_SR msg)
        {
            this.count = msg.count;
            this.pBuffer = msg.pBuffer;
        }

        public Physics_SR(ref PhysicsBuffer[] physicsBuffer)
        {
            this.count = physicsBuffer.Length;
            this.pBuffer = physicsBuffer;
        }

        public override QueueType getQueueType()
        {
            return QueueType.PHYSICS_SR;
        }

        public override void execute()
        {
            pBufferGlobal = this;
        }

    }

    class Col_Event_SR : Message
    {
        //Data in message
        public int GameObjA_ID;
        public int GameObjB_ID;
        public Vector2 ColPos;

        public Col_Event_SR(Col_Event_SR msg)
        {
            this.GameObjA_ID = msg.GameObjA_ID;
            this.GameObjB_ID = msg.GameObjB_ID;
            this.ColPos = msg.ColPos;
        }

        public Col_Event_SR(int GameObjA_ID, int GameObjB_ID, Vector2 ColPos)
        {
            this.GameObjA_ID = GameObjA_ID;
            this.GameObjB_ID = GameObjB_ID;
            this.ColPos = ColPos;
        }

        override public QueueType getQueueType()
        {
            return QueueType.COL_EVENT_SR;
        }

        override public void execute()
        {
            GameObject A = GameObjManager.Instance().findGameObj(this.GameObjA_ID).gameObj;
            GameObject B = GameObjManager.Instance().findGameObj(this.GameObjB_ID).gameObj;
            Vector2 Cpos = this.ColPos;

            if (A != null && B != null)
            {
                if (A.type < B.type)
                {
                    A.Accept(B, Cpos);
                }
                else
                {
                    B.Accept(A, Cpos);
                }
            }
        }
    }

    //public class Col_Event_SR : Message
    //{
    //    public int id;
    //    public ColEventType colEvent;

    //    public override QueueType getQueueType()
    //    {
    //        return QueueType.COL_EVENT_SR;
    //    }

    //    public override void execute()
    //    {
    //        //pBufferGlobal = this;
    //    }
    //}

    //enum Queue_type
    //{
    //    QUEUE_COL_EVENT,
    //    QUEUE_PHYSICS_BUFFER,
    //    QUEUE_SHIP_IMPULSE,
    //    QUEUE_SHIP_ROT,
    //    QUEUE_SHIP_MISSILE
    //}

    //abstract class Message
    //{
    //    abstract public void execute();
    //    abstract public QueueType getQueueType();
    //}

    


    //abstract class Ship_Message : Message
    //{
    //    //ship ID
    //    public PlayerID id;

    //    public Ship_Message(PlayerID id)
    //    {
    //        this.id = id;
    //    }

    //    public Ship_Message(Player p)
    //    {
    //        this.id = p.id;
    //    }

    //}




    //class Ship_Impulse_Message : Ship_Message
    //{
    //    //impulse
    //    public Vector2 impulse;

    //    public Ship_Impulse_Message(Ship_Impulse_Message msg)
    //        : base(msg.id)
    //    {
    //        this.impulse = msg.impulse;
    //    }

    //    public Ship_Impulse_Message(Player p, Vector2 Impulse)
    //        : base(p)
    //    {
    //        this.impulse.X = Impulse.X;
    //        this.impulse.Y = Impulse.Y;
    //    }

    //    public override Queue_type getQueueType()
    //    {
    //        return Queue_type.QUEUE_SHIP_IMPULSE;
    //    }

    //    public override void execute()
    //    {
    //        Player player = PlayerManager.Instance().getPlayer(this.id);
    //        player.playerShip.physicsObj.body.ApplyLinearImpulse(this.impulse, player.playerShip.physicsObj.body.GetWorldCenter());
    //    }


    //}

    //class Ship_Rot_Message : Ship_Message
    //{
    //    //rot
    //    float rot;

    //    public Ship_Rot_Message(Ship_Rot_Message msg)
    //        : base(msg.id)
    //    {
    //        this.rot = msg.rot;
    //    }

    //    public Ship_Rot_Message(Player p, float rotation)
    //        : base(p)
    //    {
    //        this.rot = rotation;
    //    }

    //    public override Queue_type getQueueType()
    //    {
    //        return Queue_type.QUEUE_SHIP_ROT;
    //    }

    //    public override void execute()
    //    {
    //        Player player = PlayerManager.Instance().getPlayer(this.id);
    //        player.playerShip.physicsObj.body.Rotation += this.rot;
    //    }

    //}

    //class Ship_Missile_Message : Ship_Message
    //{

    //    public Ship_Missile_Message(Ship_Missile_Message msg)
    //        : base(msg.id)
    //    {

    //    }

    //    public Ship_Missile_Message(Player p)
    //        : base(p.id)
    //    {

    //    }

    //    public override Queue_type getQueueType()
    //    {
    //        return Queue_type.QUEUE_SHIP_MISSILE;
    //    }

    //    public override void execute()
    //    {
    //        Player player = PlayerManager.Instance().getPlayer(this.id);
    //        player.createMissile();
    //    }


    //}

    //class Ship_Bomb_Message : Ship_Message
    //{

    //    public Ship_Bomb_Message(Ship_Bomb_Message msg)
    //        : base(msg.id)
    //    {

    //    }

    //    public Ship_Bomb_Message(Player p)
    //        : base(p.id)
    //    {

    //    }

    //    public override Queue_type getQueueType()
    //    {
    //        return Queue_type.QUEUE_SHIP_BOMB;
    //    }

    //    public override void execute()
    //    {
    //        Player player = PlayerManager.Instance().getPlayer(this.id);
    //        GameObjManager.Instance().createBomb(player.id);
    //    }
    //}

}