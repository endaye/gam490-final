using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using CollisionManager;
using SpriteAnimation;
using Box2D.XNA;

namespace OmegaRace
{
    public enum gameState
    {
        lobby,  // Lobby and waiting for 2 players connection
        ready,  // Flashes Ready? until the timer is up
        game,   // The main game mode
        pause,
        winner  // Displays the winner
    };

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        public GraphicsDeviceManager Graphics
        {
            get { return graphics; }
        }


        private static Game1 Game;
        public static Game1 GameInstance
        {
            get { return Game; }
        }

        private static Camera camera;
        public static Camera Camera
        {
            get { return camera; }
        }

        // Screen size
        const int screenWidth = 800;
        const int screenHeight = 500;

        // Network Gamers
        const int maxGamers = 16;
        const int maxLocalGamers = 4;

        // Network Session
        public static NetworkSession networkSession;

        // Singleton I/O queues
        InputQueue inQueue = InputQueue.Instance;
        OutputQueue outQueue = OutputQueue.Instance;

        // Backup, may be useless
        SpriteBatch spriteBatch;
        SpriteFont font;

        // Error message
        string errorMessage;

        // Keyboard and Xbox Controller states
        KeyboardState oldState;
        KeyboardState newState;

        GamePadState oldPadState;
        GamePadState newPadState;

        GamePadState P1oldPadState;
        GamePadState P1newPadState;

        //GamePadState P2oldPadState;
        //GamePadState P2newPadState;

        // For flipping game states
        public static gameState state;


        // Box2D world
        World world;
        public World getWorld()
        {
            return world;
        }

        public Rectangle gameScreenSize;


        // Quick reference for Input 
        Player player1;
        Player player2;
        Player playerCtrl;

        // Max ship speed
        public const int shipSpeed = 200;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";


            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.PreferredBackBufferWidth = screenWidth;

            gameScreenSize = new Rectangle(0, 0, screenWidth, screenHeight);

            state = gameState.lobby;

            world = new World(new Vector2(0, 0), false);

            Game = this;

            // added this line for login Live accout
            Components.Add(new GamerServicesComponent(this));
        }

        #endregion

        #region Initalization

        // Allows the game to perform any initialization it needs to before starting to run.
        // This is where it can query for any required services and load any non-graphic
        // related content.  Calling base.Initialize will enumerate through any components
        // and initialize them as well.
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here


            camera = new Camera(GraphicsDevice.Viewport, Vector2.Zero);

            state = gameState.game;

            base.Initialize();
        }


        // LoadContent will be called once per game and is the place to load
        // all of your content.
        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            world = new World(new Vector2(0, 0), true);

            myContactListener myContactListener = new myContactListener();

            world.ContactListener = myContactListener;


            Data.Instance().createData();

            state = gameState.lobby;

            player1 = PlayerManager.Instance().getPlayer(PlayerID.one);
            player2 = PlayerManager.Instance().getPlayer(PlayerID.two);

            // For screen font
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("SpriteFont1");


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            // Useless here
        }

        #endregion

        #region Update

        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            HandleInput();

            GraphicsDevice.Clear(Color.Black);

            if (networkSession == null)
            {
                // If we are not in a network session, update the
                // menu screen that will let us create or join one.
                UpdateMenuScreen();
            }
            else
            {
                // If we are in a network session, update it.
                UpdateNetworkSession(gameTime);
            }

            base.Update(gameTime);
        }

        #region Menu

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
                state = gameState.game;
            }
        }

        // Starts hosting a new network session.
        void CreateSession()
        {
            DrawMessage("Creating session...");
            try
            {
                networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, maxLocalGamers, maxGamers);
                playerCtrl = player1;
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
                    playerCtrl = player2;
                    HookSessionEvents();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        void HookSessionEvents()
        {
            Debug.WriteLine("NetworkSession.MaxPreviousGamers: {0}", NetworkSession.MaxPreviousGamers);
            networkSession.GamerJoined += GamerJoinedEventHandler;
            networkSession.SessionEnded += SessionEndedEventHandler;

        }

        // This event handler will be called whenever a new gamer joins the session.
        // We use it to allocate a Tank object, and associate it with the new gamer.
        void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            int gamerIndex = networkSession.AllGamers.IndexOf(e.Gamer);

            // Register the new player:
            Debug.WriteLine("--> Gamer join: {0} \n", gamerIndex);

        }

        // Event handler notifies us when the network session has ended.
        void SessionEndedEventHandler(object sender, NetworkSessionEndedEventArgs e)
        {
            errorMessage = e.EndReason.ToString();

            networkSession.Dispose();
            networkSession = null;
        }

        #endregion

        #region Game Part

        // Updates the state of the network session, moving the tanks
        // around and synchronizing their state over the network.
        void UpdateNetworkSession(GameTime gameTime)
        {
            // Read inputs for locally controlled tanks, and send them to the server.
            LocalNetworkGamer localGamer = networkSession.LocalGamers.ToArray<LocalNetworkGamer>()[0];

            // Get data from the network, process the input Queue to push to game
            inQueue.process(localGamer);
            
            // update server
            if (state == gameState.game && networkSession.IsHost)
            {
                UpdateServer(gameTime);
            }

            checkInput();

            // copy data from PhysicsMan to GameObjManager
            PhysicsMan.Instance().Update();

            UpdateLocalGamer(localGamer, gameTime);

            // Pump the underlying session object.
            networkSession.Update();

            // Make sure the session has not ended.
            if (networkSession == null)
                return;

            // Push data to the network
            outQueue.pushToNetwork(localGamer);

            //Debug.WriteLine("P1.pos[{0}, {1}] rot[{2}],  P2.pos [{3}, {4}] rot [{5}]",
            //    player1.playerShip.location.X,
            //    player1.playerShip.location.Y,
            //    player1.playerShip.rotation,
            //    player2.playerShip.location.X,
            //    player2.playerShip.location.Y,
            //    player2.playerShip.rotation);
        }

        void UpdateLocalGamer(LocalNetworkGamer gamer, GameTime gameTime)
        {

            if (state == gameState.game)
            {
                ScoreManager.Instance().Update();

                // copy game objects to sprites objects
                GameObjManager.Instance().Update(world);

                Timer.Process(gameTime);
            }
            Game1.Camera.Update(gameTime);
        }

        void UpdateServer(GameTime gameTime)
        {
            if (state == gameState.game)
            {
                world.Step((float)gameTime.ElapsedGameTime.TotalSeconds, 5, 8);
                PhysicsMan.Instance().pushToBuffer();
            }
        }

        #endregion

        #endregion

        #region Draw

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            // GraphicsDevice.Clear(Color.CornflowerBlue);

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

            spriteBatch.DrawString(font, message, new Vector2(161, 161), Color.Azure);
            spriteBatch.DrawString(font, message, new Vector2(160, 160), Color.White);

            spriteBatch.End();
        }

        // Draws the state of an active network session.
        void DrawNetworkSession()
        {
            if (state == gameState.game)
            {
                SpriteBatchManager.Instance().process();
            }
        }

        // Helper draws notification messages before calling blocking network methods.
        void DrawMessage(string message)
        {
            if (!BeginDraw())
                return;

            //GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.DrawString(font, message, new Vector2(161, 161), Color.Azure);
            spriteBatch.DrawString(font, message, new Vector2(160, 160), Color.White);

            spriteBatch.End();

            EndDraw();
        }

        #endregion

        #region Input

        private void checkInput()
        {
            //newState = Keyboard.GetState();
            P1newPadState = GamePad.GetState(PlayerIndex.One);
            //P2newPadState = GamePad.GetState(PlayerIndex.Two);
            //newPadState = P1newPadState;

            /*
            // Read the gamepad.
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);

            Vector2 ShipInput = gamePad.ThumbSticks.Left;

            // Normalize the input vectors.
            if (ShipInput.Length() > 1)
                ShipInput.Normalize();

            player1.playerShip.physicsObj.body.Position += ShipInput;
            */

            #region Ship moving

            float rot = 0.0f;
            float imp = 0.0f;
            int missle = -1;
            int bomb = -1;


            if (oldState.IsKeyDown(Keys.D) || P1oldPadState.IsButtonDown(Buttons.DPadRight))
            {
                rot += 0.1f;
            }

            if (oldState.IsKeyDown(Keys.A) || P1oldPadState.IsButtonDown(Buttons.DPadLeft))
            {
                rot -= 0.1f;
            }

            if (oldState.IsKeyDown(Keys.W) || P1oldPadState.IsButtonDown(Buttons.DPadUp) || P1newPadState.ThumbSticks.Left.Y > 0.3f)
            {
                imp = 0.1f;
            }

            if ((oldState.IsKeyDown(Keys.X) && newState.IsKeyUp(Keys.X)) || (P1oldPadState.IsButtonDown(Buttons.A) && P1newPadState.IsButtonUp(Buttons.A)))
            {
                //if (playerCtrl.state == PlayerState.alive && playerCtrl.missileAvailable())
                //{
                //    //playerCtrl.createMissile();
                //    playerCtrl.launchMissle();
                //}
                missle = 1;
            }

            if (oldState.IsKeyDown(Keys.C) && newState.IsKeyUp(Keys.C) || (P1oldPadState.IsButtonDown(Buttons.B) && P1newPadState.IsButtonUp(Buttons.B)))
            {
                //if (player1.state == PlayerState.alive && BombManager.Instance().bombAvailable(PlayerID.one))
                //    GameObjManager.Instance().createBomb(PlayerID.one);
                bomb = -1;
            }

            #endregion

            #region Useless input
            /*
           

            if (oldState.IsKeyDown(Keys.Right) || P2oldPadState.IsButtonDown(Buttons.DPadRight))
            {
                player2.playerShip.physicsObj.body.Rotation += 0.1f;

            }

            if (oldState.IsKeyDown(Keys.Left) || P2oldPadState.IsButtonDown(Buttons.DPadLeft))
            {
                player2.playerShip.physicsObj.body.Rotation -= 0.1f;
            }


            if (oldState.IsKeyDown(Keys.Up) || P2oldPadState.IsButtonDown(Buttons.DPadUp))
            {
                Ship Player2Ship = player2.playerShip;

                Vector2 direction = new Vector2((float)(Math.Cos(Player2Ship.physicsObj.body.GetAngle())), (float)(Math.Sin(Player2Ship.physicsObj.body.GetAngle())));

                direction.Normalize();

                direction *= shipSpeed;

                Player2Ship.physicsObj.body.ApplyLinearImpulse(direction, Player2Ship.physicsObj.body.GetWorldCenter());

            }

            if ((oldState.IsKeyDown(Keys.OemQuestion) && newState.IsKeyUp(Keys.OemQuestion)) || (P2oldPadState.IsButtonDown(Buttons.A) && P2newPadState.IsButtonUp(Buttons.A)))
            {
                if (player2.state == PlayerState.alive && player2.missileAvailable())
                {
                    player2.createMissile();
                }
            }

            if (oldState.IsKeyDown(Keys.OemPeriod) && newState.IsKeyUp(Keys.OemPeriod) || (P2oldPadState.IsButtonDown(Buttons.B) && P2newPadState.IsButtonUp(Buttons.B)))
            {
                if (player2.state == PlayerState.alive && BombManager.Instance().bombAvailable(PlayerID.two))
                    GameObjManager.Instance().createBomb(PlayerID.two);
            }


            else { }
            */
            #endregion

            P1oldPadState = P1newPadState;
            //P2oldPadState = P2newPadState;
            oldState = newState;

            if (rot != 0.0f || imp != 0.0f || missle > -1 || bomb > -1)
            {
                Ship_RS qShipRS = new Ship_RS(playerCtrl.id, rot, imp, missle, bomb);
                outQueue.add(qShipRS);
            }
        }

        /// Handles input.
        private void HandleInput()
        {
            newState = Keyboard.GetState();
            P1newPadState = GamePad.GetState(PlayerIndex.One);
            //P2newPadState = GamePad.GetState(PlayerIndex.Two);
            newPadState = P1newPadState;

            // Check for exit.
            if (IsActive && IsPressed(Keys.Escape, Buttons.Back))
            {
                Exit();
            }
        }

        // Checks if the specified button is pressed on either keyboard or gamepad.
        bool IsPressed(Keys key, Buttons button)
        {
            return (newState.IsKeyDown(key) || newPadState.IsButtonDown(button));
        }

        #endregion

        #region Other

        public void GameOver()
        {
            state = gameState.winner;


            resetData();
        }

        private void clearData()
        {
            TextureManager.Instance().clear();
            ImageManager.Instance().clear();
            SpriteBatchManager.Instance().clear();
            SpriteProxyManager.Instance().clear();
            DisplayManager.Instance().clear();
            AnimManager.Instance().clear();
            GameObjManager.Instance().clear();
            Timer.Clear();
            PlayerManager.Instance().clear();
            BombManager.Instance().clear();
        }

        public void resetData()
        {
            clearData();

            LoadContent();

            ScoreManager.Instance().createData();

            state = gameState.game;
        }

        #endregion
    }
}
