using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web.Script.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
 

namespace Glitch_Anim_Viewer
{
    public class GlitchCharacter 
    {
        /// <summary>
        ///  returns true if all the animations are loaded
        /// </summary>
        public bool IsLoaded {
            get {
                if (AnimationSheets.Count == 0) // if we havent got any animations registered yet. we havent loaded anything
                    return false;

                return ((from p in AnimationSheets where p.Value.IsLoaded == true select p).ToList().Count == AnimationSheets.Count); // Check that every animation sheet we requested has finished downloading
            }
            set {} // private set 
        }

        private readonly string MethodURL = "http://api.glitch.com/simple/players.getAnimations?player_tsid="; //The url we use to grab animation data
        private Dictionary<string, GlitchAnimationSheet> AnimationSheets; // List of all the animation sheets we have
        private Dictionary<string, GlitchAnimation> Animations; // List of all the animation data we have

        private string Tsid; // the player id we are displaying
        private GraphicsDevice Device; // graphic device to use
        private SpriteBatch Batch; // the spritebatch to use
        private Vector2 Location = Vector2.Zero; // location to draw at

        private string CurrentAnim = null; // the name of the animation we wish to display
        private int CurrentFrameIndex = 0; // the current frame we are displaying
        private int PlaybackRate = 30; // glitch animations run at 30fps 

        private int msLastFrameUpdate = 0; // How Long scince we last updated the animation
        
        /// <summary>
        ///    Glitch Character Object
        /// </summary>
        /// <param name="TSID"> TinySpeck ID of the player </param>
        /// <param name="Dev"> Graphics Device to use during Rendering </param>
        public GlitchCharacter(string TSID, GraphicsDevice Dev) {
            Tsid = TSID;
            AnimationSheets = new Dictionary<string, GlitchAnimationSheet>();
            Animations = new Dictionary<string, GlitchAnimation>();
            Device = Dev;
            Batch = new SpriteBatch(Device);
            LoadAnimations(); // Kick of the animation loading
        }

        /// <summary>
        ///  Request the Animation Data from the Glitch API
        /// </summary>
        public void LoadAnimations() {
            WebClient Client = new WebClient(); // Deals with all the nasty HTTP stuff so we dont have to.
            Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(AnimationData_DownloadStringCompleted); // Set up the callback for when we get the data back
            Client.DownloadStringAsync(new Uri(MethodURL + Tsid)); // Request the animation data from the api. passing in the TinySpeck ID for the player
            Console.WriteLine("Animation Data Request Sent");
        }

        /// <summary>
        ///  Called when the api returns the animation and sheet data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AnimationData_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            
            string raw_data = e.Result; // Typing e.Result annoys me.
            Console.WriteLine("Animation Data Received");

            // Serialize the api response string into an object
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            object response = serializer.DeserializeObject(raw_data);

            Console.WriteLine("Animation Data Deserialized");

            if (response.GetType().Equals(typeof(Dictionary<String, Object>)))
            {
                if (response.GetType().Equals(typeof(Dictionary<String, Object>)))
                {
                    Console.WriteLine("Animation Data Okay!");
                    // Cast to a dictionary
                    Dictionary<String, Object> responseDict = response as Dictionary<String, Object>;

                    // Declare ok object
                    object ok;

                    // Attempt to get the ok value
                    if (responseDict.TryGetValue("ok", out ok))
                    {
                        // If ok is an int
                        if (ok.GetType().Equals(typeof(int)))
                        {
                            // Check if it is 1, which means the request was ok
                            if ((int)ok == 1)
                            {
                                //We got the data. Load the animations
                                On_AnimationDataLoaded(responseDict);
                            }
                            else {
                                // if ok isnt of type int, something went wrong with the GlitchAPI
                                throw new Exception("Glitch API Error: API Response returned an Error");
                            }
                        }
                    }
                    else
                    {
                        // No ok value? Ooops bail.
                        throw new Exception("Glitch API Error: No Ok Value in Response");
                    }
                }
            }
        }
        private void On_AnimationDataLoaded(Dictionary<string, object> responseDict)
        {
            // Get hold of the info about the animation sheets
            Dictionary<string, object> Sheets = responseDict["sheets"] as Dictionary<string, object>;

            foreach (KeyValuePair<string, object> Sheet in Sheets) {
                Dictionary<string, object> Sheetdata = Sheet.Value as Dictionary<string, object>; // this is the sheet data
                Console.WriteLine("SpriteSheet: {0} Url: {1}", Sheet.Key, Sheetdata["url"]);
                

                // convert object[] of frames to int[]
                int[] frames = new int[(Sheetdata["frames"] as object[]).Length + 1];
                int i = 0;
                foreach(int frame_index in (Sheetdata["frames"] as object[])) {
                    frames[i] = frame_index;
                    i++;
                }


                // Create an AnimationSheet from the data. Sheet.Key is the SheetName
                AnimationSheets[Sheet.Key] = new GlitchAnimationSheet(Sheet.Key, 
                    (int)Sheetdata["cols"],
                    (int)Sheetdata["rows"], 
                    (string)Sheetdata["url"],
                    frames,
                    Tsid, Device);
            }

            // Loop over the animation data and load info about the frames
            Dictionary<string, object> AnimationData = responseDict["anims"] as Dictionary<string, object>;
            foreach (KeyValuePair<string, object> anim in AnimationData) {

                // convert frame object[] to int[]
                int[] frame_keys = new int[(anim.Value as object[]).Length];
                int i = 0;
                foreach (int key in anim.Value as object[]) {
                    frame_keys[i] = key;
                    i++;
                }

                // register the animation and all its frames
                Animations.Add(anim.Key, new GlitchAnimation() { 
                    anim_name = anim.Key,
                    frames = frame_keys
                });
            }
        }
        /// <summary>
        ///  Set the currently playing animation name
        /// </summary>
        /// <param name="animname"> animation to play </param>
        public void SetAnimation(string animname) {
            CurrentAnim = animname;
            CurrentFrameIndex = 0; // dont forget to reset the animation
            Console.WriteLine("Animation set to {0}: ", animname);
        }

        /// <summary>
        /// Helper function to pick a random animation name
        /// </summary>
        /// <returns>a random animation from those loaded </returns>
        public string PickRandomAnimation() {
            Random r = new Random();
            int anim_index = r.Next(0, Animations.Count() - 1);
            return Animations.Keys.ElementAt(anim_index);
        }

        /// <summary>
        ///  Some Linq Goodness to find out which sheet contains the frame we need
        /// </summary>
        /// <param name="FrameID"> FrameId we are after </param>
        /// <returns> the AnimationSheet Containing the frame </returns>
        public GlitchAnimationSheet FindSheetWithFrame(int FrameID) {
            return (from sheet in AnimationSheets.Values where sheet.Frames.Contains(FrameID) == true select sheet).Single();
        }
        
        /// <summary>
        ///  Update the animation
        /// </summary>
        /// <param name="Time"> Time info </param>
        public void Update(GameTime Time) {
            if (CurrentAnim == null) // if we dont have an animation selected theres no point doing anything
                return;

            msLastFrameUpdate += Time.ElapsedGameTime.Milliseconds; // update the time scince the last update
            
            if (msLastFrameUpdate > (1000 / PlaybackRate)) { // if we have shown the current frame for long enough
                
                CurrentFrameIndex++; // Update the frame we are showing

                if (CurrentFrameIndex >= Animations[CurrentAnim].frames.Count() - 1) { // if the animation is finished. lets just pick another one at random
                    CurrentFrameIndex = 0;
                    SetAnimation(PickRandomAnimation());
                }

                msLastFrameUpdate = 0; // Reset how long we have shown the current frame
            }
        }

        /// <summary>
        ///  Render the Character
        /// </summary>
        /// <param name="Time"> Time info </param>
        public void Draw(GameTime Time) {
            if (!IsLoaded) { // if we havent loaded yet we cant draw. 
                return; 
            }

            if (CurrentAnim == null) { // we are loaded but havent got an animation yet. lets pick one.
                SetAnimation(PickRandomAnimation());
            }

            
            GlitchAnimation anim = Animations[CurrentAnim]; // Get the animation we are currently playing

            // Error checking
            if (CurrentFrameIndex >= anim.frames.Count())
            {
                Console.WriteLine("Trying to play invalid frame");
                return;
            }

            // Find the sheet that holds the animation we want
            GlitchAnimationSheet Sheet = FindSheetWithFrame(anim.frames[CurrentFrameIndex]);

            // Build the DisplayRect
            Rectangle ImageRect = new Rectangle((int)Location.X, (int)Location.Y, 
                Sheet.FrameRects[anim.frames[CurrentFrameIndex]].Width,
               Sheet.FrameRects[anim.frames[CurrentFrameIndex]].Height);

            Batch.Begin();
            Batch.Draw(Sheet.Image, ImageRect, Sheet.FrameRects[anim.frames[CurrentFrameIndex]], Color.White); // Draw the current frame to the screen. we get the frame from the sheet.
            Batch.End();
        }
    }
}
