using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Glitch_Anim_Viewer.Particles
{
    public class Particle
    {
        public Vector2 Location;
        public Vector2 Offset;
        public Texture2D Texture;
        public SpriteBatch Batch;
        public float Rotation = 0;


        public Rectangle BoundingBox {
            get {
                return new Rectangle((int)(Location + Offset).X,
                    (int)(Location + Offset).Y,
                    Texture.Width,
                    Texture.Height);
            }
        }

        public Particle(Vector2 Loc, Texture2D Tex, GraphicsDevice Dev) {
            Location = Loc;
            Texture = Tex;
            Offset = Vector2.Zero;
            Batch = new SpriteBatch(Dev);
            UpAmount = new Random().Next(0, 10);
        }

        public virtual void Draw(GameTime Time)
        {
            if ((Offset + Location).X + Texture.Width > 0)
            {
                Batch.Begin();
                Batch.Draw(Texture, BoundingBox, null, Color.White, Rotation, Vector2.Zero + new Vector2(BoundingBox.Width / 2, BoundingBox.Height / 2), SpriteEffects.None, 1f);
                Batch.End();
            }
        } 


        float UpAmount = 10f;
        Random R = new Random();
        public virtual void Update(GameTime Time) 
        {
            Offset.X -= (float)Time.ElapsedGameTime.TotalSeconds * GlitchRunnerGame.CurrentScrollSpeed;
            Offset.Y += .33f;
            Offset.Y -= UpAmount;
            UpAmount -= .33f;
            Rotation += (float)R.NextDouble();
        }
    }
}
