using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpriteAnimation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;
using OmegaRace;

namespace CollisionManager
{
    enum GameObjState
    {
        alive,
        dead
    }

    abstract class GameObject : Visitor
    {
        public Sprite_Proxy spriteRef;

        public GameObjType type;
        public bool CollideAvailable;

        public PhysicsObj physicsObj;

        // Speed is m/s 
        // Note the max speed of any object is 120m/s  /////////
        public static float MaxSpeed = 50f;

        public Vector2 objSpeed;

        public float rotation;
        public Vector2 location;

        public OutputQueue outQueue = OutputQueue.Instance;

        private static int globalGameObjId = 0;

        public int id;

        public GameObject()
        {
            id = globalGameObjId++;
            rotation = 0;
            location = new Vector2();
            this.CollideAvailable = true;
        }

        public virtual void Update()
        {
            this.spriteRef.pos = location;
            this.spriteRef.rotation = rotation;
        }

        public void setPhysicsObj(PhysicsObj _physObj)
        {
            physicsObj = _physObj;
        }

        public void pushPhysics(float rot, Vector2 loc)
        {
            rotation = rot;
            location = loc;
        }
    }
}
