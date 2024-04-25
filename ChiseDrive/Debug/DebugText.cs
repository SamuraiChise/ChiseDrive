using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Debug
{
    public class DebugText
    {
        enum Stage
        {
            Write,
            Draw,
        };

        static List<DebugText> Text = new List<DebugText>();
        static SpriteFont font = null;
        static SpriteBatch sprites = null;

        static bool active = false;
        static public bool Active
        {
            set
            {
                active = value;
            }
        }
        static public bool Initialized
        {
            get
            {
                if (sprites == null || font == null) return false;
                return true;
            }
        }

        static Stage currentstage = Stage.Write;

        String text;
        Timer lifespan;

        public static void Initialize(SpriteFont font, SpriteBatch sprites)
        {
            DebugText.font = font;
            DebugText.sprites = sprites;
        }

        DebugText(String text, Time lifespan)
        {
            this.text = text;
            this.lifespan.Set(lifespan);
            Text.Add(this);
        }

        /// <summary>
        /// Opens the string buffers to accept new Write arguments.
        /// </summary>
        public static void OpenWrite()
        {
            currentstage = Stage.Write;
        }

        /// <summary>
        /// Closes writing until OpenWrite occurs.
        /// </summary>
        public static void CloseWrite()
        {
            currentstage = Stage.Draw;
        }

        public static void Update(Time elapsed)
        {
            if (sprites == null) return;

            foreach (DebugText t in Text)
            {
                t.lifespan.SubTime(elapsed);
            }
        }

        public static void Draw()
        {
            if (sprites == null) return;

            Vector2 debugpos = new Vector2();
            Vector2 shadowoffset = new Vector2(1f, 1f);
            debugpos.X = 100f;
            debugpos.Y = 50f;
            sprites.Begin();

            foreach (DebugText t in Text)
            {
                Vector2 shadowpos = debugpos + shadowoffset;
                sprites.DrawString(font, t.text, shadowpos, Color.Black);
                sprites.DrawString(font, t.text, debugpos, Color.Turquoise);
                debugpos.Y += font.MeasureString(t.text).Y - 3.5f;
            }

            Text.RemoveAll(delegate(DebugText t)
            {
                return t.lifespan.IsZero ? true : false;
            });

            sprites.End();
        }

        /// <summary>
        /// Use this when you want to write a message for one frame.
        /// </summary>
        /// <param name="text">The message to write.</param>
        public static void Write(String text)
        {
#if Debug
            if (sprites == null || font == null) throw new Exception("Call DebugText.Initialize before using!");
            if (active && currentstage == Stage.Write)
            {
                new DebugText(text, Time.Zero);
            }
#endif
        }

        /// <summary>
        /// Use this when you want to write a message to linger several frames.
        /// </summary>
        /// <param name="text">The message to write.</param>
        /// <param name="lifespan">The time it will display.</param>
        public static void Write(String text, Time lifespan)
        {
            if (sprites == null || font == null) throw new Exception("Call DebugText.Initialize before using!");
            if (active && currentstage == Stage.Write)
            {
                new DebugText(text, lifespan);
            }
        }
    }
}