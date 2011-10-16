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
        public Dictionary<int, Color[]> PerPixelCollisionInfo; // Contains info about the images for PerPixelCollisions
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
            PerPixelCollisionInfo = new Dictionary<int, Color[]>();
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
        ///  Retrieve the PerPixelCollisionData for given FrameID
        /// </summary>
        /// <param name="FrameID"> FrameID we are after</param>
        /// <returns> Image data for frame in a Color[] </returns>
        private Color[] GetFramePerPixelCollisionData(int FrameID) {
            return PerPixelCollisionInfo[FrameID];
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
                    // No Point Loading the Same Frame Twice 
                    // TODO: this may not be needed. I think i had a bug here that required this i later fixed
                    if(!FrameRects.ContainsKey(Frames[frame_index])) {
                        FrameRects.Add(
                            Frames[frame_index], 
                            new Rectangle(
                                    x * (Image.Width / cols),
                                    y * (Image.Height / rows),
                                    Image.Width / cols,
                                    Image.Height / rows
                        )); // Calculate where in the sheet this frame lives
                    }
                    frame_index++;
                } 
            }

            foreach (KeyValuePair<int, Rectangle> Rect in FrameRects) {
                var FrameBB = Rect.Value;
                Color[] Storage = new Color[Convert.ToInt32(FrameBB.Width * FrameBB.Height)];
                int Length = Storage.Length;
                int FrameSize = FrameBB.Width * FrameBB.Height;
                int ArrayIndex = FrameBB.X * FrameBB.Height;
                int ImageSize = Image.Width * Image.Height;

                if (ImageSize - ArrayIndex < FrameSize)
                    throw new InvalidDataException("Unable to copy PhysicsPixelData: StoreLength: " + Length + " Actual: " + FrameSize);

                if (Length != FrameSize)
                    throw new InvalidDataException("Unable to copy PhysicsPixelData: StoreLength: " + Length + " Actual: " + FrameSize);
                try
                {
                    //Get the pixel colors for this sheet ready for collisioncode
                    Image.GetData<Color>(0,
                        FrameBB,
                        Storage,
                        FrameBB.X * FrameBB.Y,
                        FrameBB.Width * FrameBB.Height);
                }
                catch (Exception E) {
                    Console.WriteLine(E.Message);
                    continue;
                }

                PerPixelCollisionInfo.Add(
                    Rect.Key,
                    Storage);

                Console.WriteLine("Populated it");
            }

            IsLoaded = true; // Now thats done we are loaded
        }
    }
}
