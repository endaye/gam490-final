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
        const float BirdSpeed = 0.3f;
        const float BirdFriction = 0.9f;

        // The current position and rotation of the Bird.
        public Vector2 Position;
        public Vector2 Velocity;
        
        // which player?
        public int gamerIndex;

        // Textures used to draw the Bird.
        Texture2D BirdTexture;

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

            BirdTexture = content.Load<Texture2D>("GreenBird");

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

            Velocity += BirdForward * BirdSpeed;

            // Update the position and velocity.
            Position += Velocity;
            Velocity *= BirdFriction;

            // Clamp so the Bird cannot drive off the edge of the screen.
            Position = Vector2.Clamp(Position, Vector2.Zero, screenSize);
        }

        // Draws the Bird        
        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(BirdTexture.Width / 2, BirdTexture.Height / 2);

            spriteBatch.Draw(BirdTexture,
                            Position,
                            null,
                            Color.White,
                            0.0f,
                            origin,
                            1,
                            SpriteEffects.None,
                            0);
        }
    }
}
