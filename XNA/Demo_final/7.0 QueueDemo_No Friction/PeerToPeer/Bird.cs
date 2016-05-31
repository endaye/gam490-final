//-----------------------------------------------------------------------------
// Bird.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PeerToPeer
{
    public enum Bird_Type
    {
        BIRD_POS,
    }

    public struct Bird_Data
    {
        public Bird_Type type;
        public float x;
        public float y;
        public int gamerIndex;
    }

    // Each player controls a Bird, which they can drive around the screen.
    // This class implements the logic for moving and drawing the Bird, and
    // responds to input that is passed in from outside. The Bird class does
    // not implement any networking functionality, however: that is all
    // handled by the main game class.
    public class Bird
    {
        const float BirdSpeed = 1.0f;
        const float BirdFriction = 0.9f;

        // The current position and rotation of the Bird.
        public Vector2 Position;
        //public Vector2 Velocity;
        
        // which player?
        public int gamerIndex;

        // Textures used to draw the Bird
       public Texture2D TextureCurrent;
       public Texture2D TextureGreen;
       public Texture2D TextureRed;

        Vector2 screenSize;

        // Constructs a new Bird instance.
        public Bird(int _gamerIndex, ContentManager content,
                    int screenWidth, int screenHeight)
        {

            this.gamerIndex = _gamerIndex;

            // Use the gamer index to compute a starting position, so each player
            // starts in a different place as opposed to all on top of each other.
            Position.X = screenWidth / 4 + (gamerIndex % 5) * screenWidth / 8;
            Position.Y = screenHeight / 4 + (gamerIndex / 5) * screenHeight / 5;

            TextureGreen = content.Load<Texture2D>("GreenBird");
            TextureRed = content.Load<Texture2D>("RedBird");

            screenSize = new Vector2(screenWidth, screenHeight);
        }

        // Moves the Bird in response to the current input settings.
        public void Update(Bird_Data BirdData)
        {
            Vector2 BirdForward;

            if (BirdData.type == Bird_Type.BIRD_POS)
            {
                BirdForward = new Vector2(BirdData.x, -BirdData.y);
            }
            else
            {
                BirdForward = Vector2.Zero;
            }

            BirdForward *= 3.0f;
            // Update the position and velocity.
            Position += BirdForward;
      

            // Clamp so the Bird cannot drive off the edge of the screen.
            Position = Vector2.Clamp(Position, Vector2.Zero, screenSize);
        }

        // Draws the Bird        
        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(TextureCurrent.Width / 2, TextureCurrent.Height / 2);

            spriteBatch.Draw(   this.TextureCurrent, 
                            Position,
                            null,
                            Color.White,
                            0.0f,
                            origin,
                            1,
                            SpriteEffects.None,
                            0);
        }

        public Bird_Data CreateBirdData(float x, float y)
        {
            Bird_Data Data;
            Data.type = Bird_Type.BIRD_POS;
            Data.x = x;
            Data.y = y;
            Data.gamerIndex = this.gamerIndex;

            return Data;
        }

        public static void insertOutQueue(Bird_Data d)
        {
            QueueHdr qH;
            qH.type = Queue_type.QUEUE_BIRD;
            qH.outSeqNum = OutputQueue.seqNumGlobal;
            qH.inSeqNum = -1;
            qH.data = d;

            OutputQueue.seqNumGlobal++;

            // add the to input Queue
            OutputQueue.outQ.Enqueue(qH);
        }

        public static void insertInQueue(Bird_Data d, int outSeqNum)
        {
            QueueHdr qH;
            qH.type = Queue_type.QUEUE_BIRD;
            qH.inSeqNum = InputQueue.seqNumGlobal;
            qH.outSeqNum = outSeqNum;
            qH.data = d;

            InputQueue.seqNumGlobal++;

            // add the to input Queue
            InputQueue.inQ.Enqueue(qH);
        }
    }
}
