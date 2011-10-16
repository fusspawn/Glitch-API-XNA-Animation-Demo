using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Glitch_Anim_Viewer
{
    public class GlitchObstacle
    {
        public Vector2 ScreenLoc = Vector2.Zero;
        public int XOffset = 0;
        public Texture2D ObstacleTex;
        public SpriteBatch SpriteBatch;
        public GraphicsDevice Device;
        public Color[] PerPixelCollisionData;

        public Rectangle BoundingBox {
            get {
                return new Rectangle(
                    (int)ScreenLoc.X,
                    (int)ScreenLoc.Y,
                    ObstacleTex.Width,
                    ObstacleTex.Height
                );
            }
        }

        public GlitchObstacle(Texture2D Tex, GraphicsDevice Dev) {
            ObstacleTex = Tex;
            Device = Dev;
            SpriteBatch = new SpriteBatch(Dev);
            ScreenLoc = new Vector2(
                Device.Viewport.Width + ObstacleTex.Width,
                Device.Viewport.Height - 155
            );
            PerPixelCollisionData = new Color[ObstacleTex.Width * ObstacleTex.Height];
            ObstacleTex.GetData<Color>(PerPixelCollisionData);
        }

        public void Update(GameTime Time, int CurrentScrollSpeed) {
            ScreenLoc.X -= (float)Time.ElapsedGameTime.TotalSeconds * CurrentScrollSpeed;  
        }
        
        public void Draw(GameTime Time) {
            SpriteBatch.Begin();
            SpriteBatch.Draw(ObstacleTex, BoundingBox, Color.White);
            SpriteBatch.End();
        }
    }
}
