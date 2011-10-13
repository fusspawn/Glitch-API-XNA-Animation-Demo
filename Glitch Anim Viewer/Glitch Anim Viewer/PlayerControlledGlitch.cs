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


    public class PlayerControlledGlitch
    {
        public int JumpHeight = 100;
        public PlayerMoveState CurrentState;
        public GlitchCharacter Character;

        private KeyboardState StaleKeyboardState;
        private Vector2 JumpStartLocation = Vector2.Zero;
        private int MOVE_UP = -4;
        private int MOVE_DOWN = 4;
        public int Direction = 0;

        public PlayerControlledGlitch(string TSID, GraphicsDevice Device, int GroundHeight = 250) 
        {
            Character = new GlitchCharacter(TSID, Device);
            Character.Location.X = Device.Viewport.Width / 4;
            Character.Location.Y = Device.Viewport.Height - GroundHeight;
            CurrentState = PlayerMoveState.Walking;
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
            Character.Draw(Time);
        }
    }
}
