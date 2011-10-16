using System;

namespace Glitch_Anim_Viewer
{
#if WINDOWS || XBOX
    static class EntryPoint
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (GlitchRunnerGame game = new GlitchRunnerGame())
            {
                game.Run();
            }
        }
    }
#endif
}

