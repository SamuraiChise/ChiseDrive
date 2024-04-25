using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Graphics
{
    public abstract class Drawable2D
    {
        static List<Drawable2D> DrawList = new List<Drawable2D>();
        static public void DrawAll(SpriteBatch spritebatch)
        {
            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.SaveState);
            foreach (Drawable2D sprite in DrawList)
            {
                sprite.Draw(spritebatch);
            }
            spritebatch.End();
        }

        /// <summary>
        /// Set to false when stopping drawing
        /// </summary>
        public virtual bool Visible
        {
            get
            {
                return DrawList.Contains(this);
            }
            set
            {
                if (value && !Visible) DrawList.Add(this);
                if (!value && Visible) DrawList.Remove(this);
            }
        }

        /// <summary>
        /// Drawcolor (default White)
        /// </summary>
        public virtual Color Color
        {
            set
            {
                drawcolor = value;
            }
            get
            {
                return drawcolor;
            }
        }

        public virtual float Layer
        {
            set
            {
                layer = value;
            }
            get
            {
                return layer;
            }
        }

        /// <summary>
        /// The centerpoint for the sprite. (default 0,0)
        /// </summary>
        public virtual Vector2 Center
        {
            set
            {
                position = value;
            }
            get
            {
                return position;
            }
        }

        /// <summary>
        /// Sets the X/Y scale (default 1,1)
        /// </summary>
        public virtual Vector2 Scale
        {
            set
            {
                scale = value;
            }
        }

        public bool Fading
        {
            get
            {
                return fadedest == NoFade ? false : true;
            }
        }

        public float Fade
        {
            set
            {
                fade = value;
                Helper.Clamp(ref fade, 0f, 1f);
                drawcolor.A = (byte)(fade * 255f);
            }
        }

        public const float FastFade = 10f;
        public const float MediumFade = 30f;
        public const float SlowFade = 60f;

        public const float BottomLayer = 1f;
        public const float MiddleLayer = 0.5f;
        public const float TopLayer = 0f;

        protected Color drawcolor = Color.White;
        protected Vector2 position = Vector2.Zero;
        protected Vector2 scale = Vector2.One;
        protected float fade = 1f;
        protected float layer = BottomLayer;

        const float NoFade = -1f;
        float fadedest = NoFade;
        float fadespeed = 0f;

        /// <summary>
        /// Fades the Sprite out.
        /// </summary>
        /// <param name="fadetime">Time to Fade</param>
        public virtual void FadeOut(Time fadetime)
        {
            if (fadetime == Time.Zero) this.fadespeed = 1f;
            else this.fadespeed = 1f / fadetime;
            fadedest = 0f;
        }

        /// <summary>
        /// Fades the Sprite in.
        /// </summary>
        /// <param name="fadespeed">Time to Fade</param>
        public virtual void FadeIn(Time fadetime)
        {
            if (fadetime == Time.Zero) this.fadespeed = 1f;
            else this.fadespeed = 1f / fadetime;
            fadedest = 1f;
            Visible = true;
        }

        public virtual void Update(Time elapsed)
        {
            if (fadedest != NoFade)
            {
                Fade = Helper.EaseTo(fade, fadedest, fadespeed, elapsed);
                if (fade == fadedest) fadedest = NoFade;
                if (fade == 0f) Visible = false;
            }
        }

        public abstract void Draw(SpriteBatch spritebatch);
    }
}