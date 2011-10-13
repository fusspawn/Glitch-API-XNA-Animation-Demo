using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Glitch_Anim_Viewer
{
    public enum GamePlayState { 
        Loading = 0, // We are loading the content
        Starting = 2,
        Playing = 3,
        Died = 4,
    }


    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        List<GlitchObstacle> Obstacles;
        SpriteFont DefaultFont;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        PlayerControlledGlitch Character; // Character we wish to display
        ScrollingBackground Background;
        KeyboardState StaleState;
        Texture2D bgTex;
        Texture2D DebugTex;
        int CurrentScrollSpeed = 0;
        int LastSpeedIncrease = 0;
        int Score = 0;
        public static GamePlayState CurrentGameState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 1024;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            CurrentGameState = GamePlayState.Loading;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
             base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            bgTex = Content.Load<Texture2D>("groddle");
            // Kick of the Character Loading
            Obstacles = new List<GlitchObstacle>();
            DefaultFont = Content.Load<SpriteFont>("DefaultFont");
            Character = new PlayerControlledGlitch("PHV2HON5BG82O1J", GraphicsDevice); // I use my TSID here. Change this to Yours
            Background = new ScrollingBackground(bgTex, GraphicsDevice);
            DebugTex = Content.Load<Texture2D>("debugtex");
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
                base.Update(gameTime);

                if (StaleState == null)
                    StaleState = Keyboard.GetState();

                if (CurrentGameState == GamePlayState.Starting)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Space) && !StaleState.IsKeyDown(Keys.Space))
                    {
                        CurrentGameState = GamePlayState.Playing;
                        CurrentScrollSpeed = 150;
                        Character.Character.Location.Y = GraphicsDevice.Viewport.Height - 250;
                        Character.Character.SetAnimation("walk1x");
                        Character.CurrentState = PlayerMoveState.Walking;
                        Character.Direction = 0;
                        Obstacles = new List<GlitchObstacle>();
                        Background.ScreenPos = Vector2.Zero;
                        Score = 0;
                        return; // We have done what ever we need this frame
                    }
                }
                if (CurrentGameState == GamePlayState.Playing)
                {
                    LastSpeedIncrease += gameTime.ElapsedGameTime.Milliseconds;

                    if (LastSpeedIncrease > 1000)
                    {
                        CurrentScrollSpeed += 20;
                        LastSpeedIncrease = 0;
                        Score += 1;
                    }

                    Background.Update(gameTime, CurrentScrollSpeed);
                    Character.Update(gameTime); // Update the Character
                    UpdateGameCollsions(gameTime);
                    UpdateGameObjects(gameTime);
                    return;
                }

                if (CurrentGameState == GamePlayState.Died)
                {
                    
                    if (Keyboard.GetState().IsKeyDown(Keys.Space) && !StaleState.IsKeyDown(Keys.Space))
                    {
                        Character.CurrentState = PlayerMoveState.Dead;
                        CurrentGameState = GamePlayState.Starting;
                        CurrentScrollSpeed = 50;
                    }

                    return;
                }

                StaleState = Keyboard.GetState();
        }
        private int LastSpawnDelay = 0;
        private int SpawnDelay = 3000;
        private void UpdateGameObjects(GameTime gameTime)
        {
            LastSpawnDelay += gameTime.ElapsedGameTime.Milliseconds;
            if (LastSpawnDelay > SpawnDelay) { 
                Obstacles.Add(new GlitchObstacle(Content.Load<Texture2D>("log"), GraphicsDevice));
                LastSpawnDelay = 0;
            }
        }
        private void UpdateGameCollsions(GameTime gameTime)
        {
            GlitchObstacle[] Obs = Obstacles.ToArray();
            foreach (GlitchObstacle ob in Obs) {
                if (ob.BoundingBox.X + ob.ObstacleTex.Width <= 0) //offscreen?
                    Obstacles.Remove(ob); // Remove old Obstacles

                if (ob.BoundingBox.Intersects(Character.Character.FootBoundingBox))
                {// We hit the obstacle
                    CurrentGameState = GamePlayState.Died;
                    Character.Character.SetAnimation("hit1");
                }

                ob.Update(gameTime, CurrentScrollSpeed);
            }
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue); // Clear the screen
            Background.Draw(gameTime); // Draw the background
            Character.Draw(gameTime); // Draw the Character
            spriteBatch.Begin();
            
            foreach (GlitchObstacle Ob in Obstacles) {
                Ob.Draw(gameTime);
            }
            
            if (CurrentGameState == GamePlayState.Loading) {
                spriteBatch.DrawString(DefaultFont, "Loading Character..", new Vector2(
                    -(DefaultFont.MeasureString("Loading Character..").X / 2) - GraphicsDevice.Viewport.Width / 2, 200)
                    ,Color.White);
            }

            if (CurrentGameState == GamePlayState.Starting) {
                spriteBatch.DrawString(DefaultFont, "Press Jump (Space) \n to Start", new Vector2(
                    -(DefaultFont.MeasureString("Press Jump (Space) \n to Start").X / 2) + GraphicsDevice.Viewport.Width / 2, 200)
                    , Color.White);
            }

            if (CurrentGameState == GamePlayState.Died) {
                spriteBatch.DrawString(DefaultFont, "You died!. Press Space to Restart \n Score: " + Score, new Vector2(
                   -(DefaultFont.MeasureString("You died!. Press Space to Restart \n Score: " + Score).X / 2) - GraphicsDevice.Viewport.Width / 2, 200)
                    , Color.White);
            }

            if (CurrentGameState == GamePlayState.Playing) {
                spriteBatch.DrawString(DefaultFont, "Score: " + Score, Vector2.Zero, Color.White);
            }

            spriteBatch.DrawString(DefaultFont, "State: " + CurrentGameState.ToString(), new Vector2(0, 20), Color.White); // Debug
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
