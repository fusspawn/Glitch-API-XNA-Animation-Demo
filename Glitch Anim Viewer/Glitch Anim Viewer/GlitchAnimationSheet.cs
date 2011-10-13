using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Net;
using System.IO;
using Microsoft.Xna.Framework;

namespace Glitch_Anim_Viewer
{
    public class GlitchAnimationSheet
    {
        private int cols, rows = 0; // Number of Columns and Rows in this animation.
        private string Url; // The Url we download the Image from
        private string PlayerTSID; // The TinySpeckID for the player 
        private string SheetName; // The Name of this AnimationSheet
        private GraphicsDevice GDevice; // GraphicsDevice to load the image to

        public int[] Frames; // List of FrameID's this AnimationSheet Contains
        public Dictionary<int, Rectangle> FrameRects; // We Cache the Rectangles for all the Frames
        public Texture2D Image; // The Texture
        public bool IsLoaded = false; // Are we loaded yet?

        /// <summary>
        ///  Contains the Image data for a GlitchAnimation
        /// </summary>
        /// <param name="name"> Sheet Name </param>
        /// <param name="Cols"> Number of Columns in the Sheet </param>
        /// <param name="Rows"> Number of Rows in the Sheet </param>
        /// <param name="ImageUrl"> The Url we need to download </param>
        /// <param name="frames"> List of Frames this AnimationSheet holds </param>
        /// <param name="Tsid"> TinySpeckID of player </param>
        /// <param name="Device">Graphics Device </param>
        public GlitchAnimationSheet(string name, int Cols, int Rows, string ImageUrl, int[] frames, string Tsid, GraphicsDevice Device) {
            cols = Cols;
            rows = Rows;
            Url = ImageUrl;
            PlayerTSID = Tsid;
            SheetName = name;
            GDevice = Device;
            Frames = frames;
            FrameRects = new Dictionary<int, Rectangle>();
            DownloadImage(); // Start the Download
        }

        /// <summary>
        ///  Sets up EventHandlers and Sets the Image Download In Motion
        /// </summary>
        private void DownloadImage()
        {
            Console.WriteLine("Download Image From Url: {0}", Url);
            WebClient Client = new WebClient();
            Client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(Client_DownloadFileCompleted);
            Client.DownloadFileAsync(new Uri(Url), "./downloads/" + PlayerTSID + SheetName + ".png"); // We save the image to the ./download/ folder with the following format.  PlayerTSID followed by the SheetName and finally the .png suffix
        }

        /// <summary>
        ///  Called when the Image is downloaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {

            // Load the Image we Just Downloaded onto the GraphicsCard
            Image = Texture2D.FromStream(GDevice, File.OpenRead("./downloads/" + PlayerTSID + SheetName + ".png")); // Load in the file we just downloaded
            Console.WriteLine("Animation Sprites Loaded: {0}", SheetName);

            // Loop Over all the images and calculate the frame rectangles for each frame
            int frame_index = 0;
            for (int x = 0; x < cols; x++) {
                for (int y = 0; y < rows; y++) {
                    Console.WriteLine("Frame_Index = {0} FrameKey = {1}", frame_index, Frames[frame_index]);

                    // No Point Loading the Same Frame Twice 
                    // TODO: this may not be needed. I think i had a bug here that required this i later fixed
                    if(!FrameRects.ContainsKey(Frames[frame_index])) {
                        Console.WriteLine("Adding Frame: {0} to Anim: {1}", Frames[frame_index], SheetName);
                        FrameRects.Add(
                            Frames[frame_index], 
                            new Rectangle(
                                    x * (Image.Width / cols),
                                    y * (Image.Height / rows),
                                    Image.Width / cols,
                                    Image.Height / rows
                        ));
                    }

                    frame_index++;
                } 
            }

            IsLoaded = true; // Now thats done we are loaded
        }
    }
}
