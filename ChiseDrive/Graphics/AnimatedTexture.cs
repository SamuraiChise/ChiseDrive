using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ChiseDrive;

namespace ChiseDrive.Graphics
{
    /// <summary>
    /// Holds a series of textures, and animates them 1 frame per Tick
    /// </summary>
    public class AnimatedTexture
    {
        #region Values
        ArrayList frames = null;
        bool looping = false;
        bool pause = false;
        int currentframe = 0;
        Time framecounter = Time.Zero;
        Time animationrate = Time.FromFPS(30f);
        #endregion

        #region Properties
        public Texture2D Frame
        {
            get
            {
                return (Texture2D)frames[currentframe];
            }
        }
        public bool Looping
        {
            get
            {
                return looping;
            }
            set
            {
                looping = value;
            }
        }
        public bool Pause
        {
            get
            {
                return pause;
            }
            set
            {
                pause = value;
            }
        }
        public Time AnimationRate
        {
            set
            {
                animationrate = value;
            }
        }
        #endregion

        public AnimatedTexture()
        {
            frames = new ArrayList();
            looping = false;
            pause = false;
            currentframe = 0;
        }

        public AnimatedTexture(ContentManager content, String filename)
        {
            frames = new ArrayList();
            looping = false;
            pause = false;
            currentframe = 0;
            frames.Add(content.Load<Texture2D>(filename));
        }

        public AnimatedTexture(ContentManager content, String filename, int numberframes)
        {
            frames = new ArrayList();
            looping = false;
            pause = false;
            currentframe = 0;

            for (int i = 0; i < numberframes; i++)
            {
                frames.Add(content.Load<Texture2D>(filename + i));
            }
        }

        public AnimatedTexture(AnimatedTexture copy)
        {
            frames = copy.frames;
            looping = copy.looping;
            pause = copy.pause;
            currentframe = copy.currentframe;
        }

        public AnimatedTexture(Texture2D texture)
        {
            frames = new ArrayList();
            looping = false;
            pause = false;
            currentframe = 0;
            frames.Add(texture);
        }

        /// <summary>
        /// Non looping animations can end.
        /// </summary>
        /// <returns>True if done.</returns>
        public bool IsDone()
        {
            if (currentframe >= frames.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Starts the animation.  Every call to draw updates the animation count.
        /// </summary>
        /// <param name="asLoop"></param>
        public void Play()
        {
            looping = false;
            currentframe = 0;
            pause = false;
        }

        public void Loop()
        {
            looping = true;
            currentframe = 0;
            pause = false;
        }

        public void Animate(Time frame)
        {
            if (!pause)
            {
                framecounter += frame;
                if (framecounter >= animationrate)
                {
                    currentframe++;
                    framecounter = Time.Zero;
                }

                if (IsDone())
                {
                    // We're at the end
                    if (looping)
                    {
                        // Unless we're looping
                        while (currentframe >= frames.Count)
                        {
                            currentframe -= frames.Count;
                        }
                    }
                    else
                    {
                        // Hold on the last frame if not looping
                        currentframe = frames.Count - 1;
                    }
                }
            }
        }
    }
}
