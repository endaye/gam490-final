//-----------------------------------------------------------------------------
// PeerToPeerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using System.Diagnostics;

namespace PeerToPeer
{
    // Sample showing how to implement a simple multiplayer
    // network session, using a peer-to-peer network topology.
    public class PeerToPeerGame : Microsoft.Xna.Framework.Game
    {
        const int screenWidth = 1067;
        const int screenHeight = 600;
        const int maxGamers = 16;
        const int maxLocalGamers = 4;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        KeyboardState currentKeyboardState;
        GamePadState currentGamePadState;

        NetworkSession networkSession;
// Bang 6
//        PacketReader packetReader = new PacketReader();

        InputQueue inQueue = new InputQueue();
        OutputQueue outQueue = new OutputQueue();

        string errorMessage;

        public PeerToPeerGame()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;

            Content.RootDirectory = "Content";

            Components.Add(new GamerServicesComponent(this));
        }

        // Load your content.
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Font");
        }

        // Allows the game to run logic.
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            if (networkSession == null)
            {
                // If we are not in a network session, update the
                // menu screen that will let us create or join one.
                UpdateMenuScreen();
            }
            else
            {
                // If we are in a network session, update it.
                UpdateNetworkSession();
            }

            base.Update(gameTime);
        }

        // Menu screen provides options to create or join network sessions.
        void UpdateMenuScreen()
        {
            if (IsActive)
            {
                if (Gamer.SignedInGamers.Count == 0)
                {
                    // If there are no profiles signed in, we cannot proceed.
                    // Show the Guide so the user can sign in.
                    Guide.ShowSignIn(maxLocalGamers, false);
                }
                else if (IsPressed(Keys.A, Buttons.A))
                {
                    // Create a new session?
                    CreateSession();
                }
                else if (IsPressed(Keys.B, Buttons.B))
                {
                    // Join an existing session?
                    JoinSession();
                }
            }
        }

        // Starts hosting a new network session.
        void CreateSession()
        {
            DrawMessage("Creating session...");
            try
            {
                networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, maxLocalGamers, maxGamers);
                HookSessionEvents();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        // Joins an existing network session.
        void JoinSession()
        {
            DrawMessage("Joining session...");

            try
            {
                // Search for sessions.
                using (AvailableNetworkSessionCollection availableSessions =
                            NetworkSession.Find(NetworkSessionType.SystemLink,
                                                maxLocalGamers, null))
                {
                    if (availableSessions.Count == 0)
                    {
                        errorMessage = "No network sessions found.";
                        return;
                    }

                    // Join the first session we found.
                    networkSession = NetworkSession.Join(availableSessions[0]);

                    HookSessionEvents();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        // After creating or joining a network session, we must subscribe to
        // some events so we will be notified when the session changes state.
        void HookSessionEvents()
        {
            networkSession.GamerJoined += GamerJoinedEventHandler;
            networkSession.SessionEnded += SessionEndedEventHandler;
        }

        // This event handler will be called whenever a new gamer joins the session.
        // We use it to allocate a Bird object, and associate it with the new gamer.
        void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            int gamerIndex = networkSession.AllGamers.IndexOf(e.Gamer);

            e.Gamer.Tag = new Bird(gamerIndex, Content, screenWidth, screenHeight);

            // Register the new player:
            Debug.WriteLine("--> Gamer join: {0} \n", gamerIndex);
            // Magic stuff here: needed for queue
            InputQueue.pBird[gamerIndex] = e.Gamer.Tag as Bird;


            Bird thisBird = e.Gamer.Tag as Bird;
            if (e.Gamer.IsHost)
            {
                thisBird.TextureCurrent = thisBird.TextureGreen;
            }
            else
            {
                thisBird.TextureCurrent = thisBird.TextureRed;
            }
        }

        // Event handler notifies us when the network session has ended.
        void SessionEndedEventHandler(object sender, NetworkSessionEndedEventArgs e)
        {
            errorMessage = e.EndReason.ToString();

            networkSession.Dispose();
            networkSession = null;
        }

        // Updates the state of the network session, moving the Birds
        // around and synchronizing their state over the network.
        void UpdateNetworkSession()
        {
            LocalNetworkGamer localGamer = null;

            // Update our locally controlled Birds, and send their
            // latest position data to everyone in the session.
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                localGamer = gamer;
                UpdateLocalGamer(gamer);
            }

            // Push data to the network
            outQueue.pushToNetwork(localGamer);

            // Get data from the network
            inQueue.pullFromNetwork(localGamer);

// bang 6
          //  while (localGamer.IsDataAvailable)
          //  {
          //      NetworkGamer sender;
//
             //   // Read a single packet from the network.
            //    localGamer.ReceiveData(packetReader, out sender);
//
            //    // Write the tank state into a network packet.
           //     Bird_Data qBird;
           //     QueueHdr qH;
//
            //    qH.inSeqNum = packetReader.ReadInt32();
            //    qH.outSeqNum = packetReader.ReadInt32();
            //    qH.type = (Queue_type)packetReader.ReadInt32();
//
            //    qBird.type = (Bird_Type)packetReader.ReadInt32();
           //     qBird.x = packetReader.ReadSingle();
           //     qBird.y = packetReader.ReadSingle();
           //     qBird.gamerIndex = packetReader.ReadInt32();
//
          //      Bird_inQueue.add(qBird, qH.outSeqNum);
          //  }

            // Process the input Queue to push to game
            inQueue.process();

            // Pump the underlying session object.
            networkSession.Update();

            // Make sure the session has not ended.
            if (networkSession == null)
                return;
        }

        // Helper for updating a locally controlled gamer.
        void UpdateLocalGamer(LocalNetworkGamer gamer)
        {
            // Look up what Birds is associated with this local player.
            Bird localBird = gamer.Tag as Bird;

            // Update the Bird.
            ReadBirdInputs(localBird, gamer.SignedInGamer.PlayerIndex);
        }

        // This is called when the game should draw itself.
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (networkSession == null)
            {
                // If we are not in a network session, draw the
                // menu screen that will let us create or join one.
                DrawMenuScreen();
            }
            else
            {
                // If we are in a network session, draw it.
                DrawNetworkSession();
            }

            base.Draw(gameTime);
        }

        // Draws the startup screen used to create and join network sessions.
        void DrawMenuScreen()
        {
            string message = string.Empty;

            if (!string.IsNullOrEmpty(errorMessage))
                message += "Error:\n" + errorMessage.Replace(". ", ".\n") + "\n\n";

            message += "A = create session\n" +
                       "B = join session";

            spriteBatch.Begin();

            spriteBatch.DrawString(font, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(font, message, new Vector2(160, 160), Color.White);

            spriteBatch.End();
        }

        // Draws the state of an active network session.
        void DrawNetworkSession()
        {
            spriteBatch.Begin();

            // For each person in the session...
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                // Look up the Bird object belonging to this network gamer.
                Bird Bird = gamer.Tag as Bird;

                // Draw the Bird.
                Bird.Draw(spriteBatch);

                // Draw a gamertag label.
                string label = gamer.Gamertag;
                Color labelColor = Color.Black;
                Vector2 labelOffset = new Vector2(100, 150);

                if (gamer.IsHost)
                    label += " (host)";

                // Flash the gamertag to yellow when the player is talking.
                if (gamer.IsTalking)
                    labelColor = Color.Yellow;

                spriteBatch.DrawString(font, label, Bird.Position, labelColor, 0,
                                       labelOffset, 0.6f, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }

        // Helper draws notification messages before calling blocking network methods.
        void DrawMessage(string message)
        {
            if (!BeginDraw())
                return;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.DrawString(font, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(font, message, new Vector2(160, 160), Color.White);

            spriteBatch.End();

            EndDraw();
        }

        // Handles input.
        private void HandleInput()
        {
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (IsActive && IsPressed(Keys.Escape, Buttons.Back))
            {
                Exit();
            }
        }

        // Checks if the specified button is pressed on either keyboard or gamepad.
        bool IsPressed(Keys key, Buttons button)
        {
            return (currentKeyboardState.IsKeyDown(key) || currentGamePadState.IsButtonDown(button));
        }

        // Reads input data from keyboard and gamepad, and stores
        // it into the specified Bird object.
        void ReadBirdInputs(Bird Bird, PlayerIndex playerIndex)
        {
            // Read the gamepad.
            GamePadState gamePad = GamePad.GetState(playerIndex);

            Vector2 BirdInput = gamePad.ThumbSticks.Left;

            // Read the keyboard.
            KeyboardState keyboard = Keyboard.GetState(playerIndex);

            if (keyboard.IsKeyDown(Keys.Left))
                BirdInput.X = -1;
            else if (keyboard.IsKeyDown(Keys.Right))
                BirdInput.X = 1;

            if (keyboard.IsKeyDown(Keys.Up))
                BirdInput.Y = 1;
            else if (keyboard.IsKeyDown(Keys.Down))
                BirdInput.Y = -1;

            // Normalize the input vectors.
            if (BirdInput.Length() > 1)
                BirdInput.Normalize();

            // Store these input values into the Bird object.
            Bird_Data BirdData = Bird.CreateBirdData(BirdInput.X, BirdInput.Y);
// BANG 6
            //BirdData.type = Bird_Type.BIRD_POS;
            //BirdData.x = BirdInput.X;
            //BirdData.y = BirdInput.Y;
            //BirdData.gamerIndex = Bird.gamerIndex;

            // Reason it's output, local is controlling it
            // so your are outputing stimulus to the system.
            // ONLY way you can input data, it's coming into the inputQueue from network
            // This data will be propagated to the local machine (inBirdQueue) and transmitted to network
// BaNG 6
            //Bird_outQueue.add(BirdData);
            Bird.insertOutQueue(BirdData);
        }
    }

    // The main entry point for the application.
    static class Program
    {
        static void Main()
        {
            using (PeerToPeerGame game = new PeerToPeerGame())
            {
                game.Run();
            }
        }
    }
}
