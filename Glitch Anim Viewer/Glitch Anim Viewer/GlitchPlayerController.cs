using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Glitch_Anim_Viewer
{
    public enum PlayerMoveState {
        Walking = 0, 
        Jumping = 1,
        Dead = 2,
    }


    public class GlitchPlayerController
    {
        public int JumpHeight = 100;
        public PlayerMoveState CurrentState;
        public GlitchCharacter Character;

        private GraphicsDevice Device;
        private KeyboardState StaleKeyboardState;
        private Vector2 JumpStartLocation = Vector2.Zero;
        private SpriteBatch ShadowBatch;
        private Texture2D ShadowTex;
        private int MOVE_UP = -4;
        private int MOVE_DOWN = 4;
        public int Direction = 0;


        public GlitchPlayerController(string TSID, GraphicsDevice _Device, int GroundHeight = 250) 
        {
            Device = _Device;
            Character = new GlitchCharacter(TSID, _Device);
            Character.Location.X = _Device.Viewport.Width / 4;
            Character.Location.Y = _Device.Viewport.Height - GroundHeight;
            CurrentState = PlayerMoveState.Walking;
            ShadowBatch = new SpriteBatch(_Device);
            ShadowTex = GlitchRunnerGame.ContentManager.Load<Texture2D>("CharShadow");
        }
        public void Update(GameTime Time) {
            Character.Update(Time);
            DoInput();
            DoJumpingLogic();
            Character.Location.Y += Direction;
        }
        private void DoJumpingLogic()
        {
            if (CurrentState != PlayerMoveState.Jumping)
                return;

            if (JumpStartLocation.Y - Character.Location.Y > JumpHeight) {
                Direction = MOVE_DOWN;
            }

            if (Character.Location.Y > JumpStartLocation.Y)
            {
                Character.Location.Y = JumpStartLocation.Y;
                CurrentState = PlayerMoveState.Walking;
                Character.SetAnimation("walk2x");
                Direction = 0;
                GlitchRunnerGame.ParticleManager.SpawnSmokeParticles(new Vector2(Character.FootBoundingBox.X + (Character.FootBoundingBox.Width / 2) 
                    , Character.FootBoundingBox.Y + Character.FootBoundingBox.Height), 4, 25);
            }
        }
        private void DoInput()
        {
            if (StaleKeyboardState == null)
                StaleKeyboardState = Keyboard.GetState();

            if (Keyboard.GetState().IsKeyDown(Keys.Space) &&
                !StaleKeyboardState.IsKeyDown(Keys.Space) &&
                CurrentState == PlayerMoveState.Walking) {
                    CurrentState = PlayerMoveState.Jumping;
                    JumpStartLocation = Character.Location;
                    Direction = MOVE_UP;
                    Character.SetAnimation("jumpOver_test_sequence");
            }

            StaleKeyboardState = Keyboard.GetState();
        }
        public void Draw(GameTime Time) {
            if (CurrentState == PlayerMoveState.Walking || CurrentState == PlayerMoveState.Dead) {
                ShadowBatch.Begin();
                ShadowBatch.Draw(ShadowTex, Character.FootBoundingBox, Color.Black);
                ShadowBatch.End();
            }
            Character.Draw(Time);
        }
    }
}
