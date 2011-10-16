using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Glitch_Anim_Viewer.Particles
{
    public class ParticleManager
    {
        List<Particle> Particles;
        GraphicsDevice Device;

        public ParticleManager(GraphicsDevice Dev) {
            Device = Dev;
            Particles = new List<Particle>();
        }

        public void SpawnSmokeParticles(Vector2 Location, int Count, int SpreadMax) {
            Random R = new Random();
            for (int i = 0; i < Count; i++) {
                Particles.Add(new Particle(Location + new Vector2(
                    R.Next(-SpreadMax, SpreadMax), R.Next(-SpreadMax, SpreadMax)),
                    GlitchRunnerGame.ContentManager.Load<Texture2D>("groddlegroundpart"), Device));
            }
        }

        public void Update(GameTime Time) {
            Particle[] Parts = Particles.ToArray();
            foreach (Particle Part in Parts) {
                Part.Update(Time);
                if (Part.BoundingBox.Right < 0)
                    Particles.Remove(Part);
            }
        }

        public void Draw(GameTime Time) {
            foreach (Particle toDraw in Particles)
                toDraw.Draw(Time);
        }
    }
}
