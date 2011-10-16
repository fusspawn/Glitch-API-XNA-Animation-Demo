using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Glitch_Anim_Viewer
{
    public class GlitchScrollingBackground
    {
        public Texture2D bgTex;
        public Vector2 ScreenPos, Origin = Vector2.Zero;
        private GraphicsDevice Device;
        private SpriteBatch Batch;

        public GlitchScrollingBackground(Texture2D Texture, GraphicsDevice Dev) {
            bgTex = Texture;
            Device = Dev;
            ScreenPos = new Vector2(0, 0);
            Origin = new Vector2(0, 0);
            Batch = new SpriteBatch(Device);
        }

        public void Update(GameTime Time, int ScrollSpeed = 100) {
            ScreenPos.X -= (float)Time.ElapsedGameTime.TotalSeconds * ScrollSpeed; // This just seems good
        }

        public void Draw(GameTime Time) {
            Batch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone);
            Batch.Draw(bgTex, Vector2.Zero , new Rectangle((int)-ScreenPos.X, (int)-ScreenPos.Y, Device.Viewport.Width, Device.Viewport.Height), Color.White, 0, Origin, 1f, SpriteEffects.None, 0f);
            Batch.End();
        }
    }
}
