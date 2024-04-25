using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#if !Xbox
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endif
using ChiseDrive;
using ChiseDrive.Input;

namespace ChiseDrive.Menu
{
    public sealed class MenuComponent
    {
        public enum State
        {
            Open,
            Idle,
            Highlight,
            Select,
            Close,
            Disable,
            Enable,
        };
        const int NumStates = 7;

        public readonly string TypeName = "Unknown";

        public Rectangle Bounds { get; set; }
        public List<MenuKeyframe>[] Keyframes;
        public List<IMenuAsset> Assets;

        State currentState;

        List<MenuKeyframe> CurrentKeyList
        {
            get
            {
                return Keyframes[(int)currentState];
            }
        }

        MenuKeyframe current = MenuKeyframe.None;
        MenuKeyframe start = MenuKeyframe.None;
        MenuKeyframe finish = MenuKeyframe.None;
        Time elapsed = Time.Zero;
        int step;

        public MenuComponent()
        {
            Keyframes = new List<MenuKeyframe>[NumStates];
            for (int i = 0; i < NumStates; i++)
            {
                Keyframes[i] = new List<MenuKeyframe>();
            }

            TypeName = this.ToString();
        }

        public void ResolveAssets(ContentManager content) { }

        MenuKeyframe FindCurrentKey()
        {
            if (CurrentKeyList != null && CurrentKeyList.Count > step)
            {
                return CurrentKeyList[step];
            }
            return MenuKeyframe.None;
        }
        MenuKeyframe FindTerminationKey()
        {
            if (CurrentKeyList != null && CurrentKeyList.Count > 0)
            {
                return CurrentKeyList[CurrentKeyList.Count - 1];
            }
            // No sequence
            return MenuKeyframe.None;
        }

        void StartAnimation(MenuKeyframe start, MenuKeyframe finish)
        {
            this.start = start;
            this.finish = finish;
            this.elapsed = 0f;
        }

        public void SetState(State next)
        {
            currentState = next;

            // Always start with the first keyframe
            // from a series
            step = 0;

            // Our destination keyframe is the current one
            // (it can be null)
            MenuKeyframe firstkey = FindCurrentKey();

            if (next == State.Open)
            {
                if (firstkey == MenuKeyframe.None) throw new Exception("All menu items at least need one keyframe in the open list.");

                // When opening, make sure
                // everything starts with a keyframe
                current = firstkey;
            }

            // We're animating away from our 
            // current frame when changing state
            StartAnimation(current, firstkey);
        }

        public bool IsFinished(State test)
        {
            // Not even in that state
            if (currentState != test) return false;

            MenuKeyframe terminationKey = FindTerminationKey();

            if (terminationKey == MenuKeyframe.None)
            {
                // We're at the condition if it doesn't exist
                return true;
            }

            if (terminationKey == current)
            {
                // We're at the last key
                return true;
            }

            return false;
        }

        public MenuCommand CheckInput(Instruction value)
        {
            return MenuCommand.Empty;
        }

        public void Update(Time elapsed)
        {
            if (start != MenuKeyframe.None && finish != MenuKeyframe.None)
            {
                // Going from one to the next
                float percent = this.elapsed / finish.Length;
                if (percent > 1f) percent = 1f;
                current = MenuKeyframe.Lerp(start, finish, percent);

                if (percent == 1f)
                {
                    finish = MenuKeyframe.None;
                    start = MenuKeyframe.None;
                }
            }
            else if (start != MenuKeyframe.None)
            {
                // Snapping to start
                current = start;
                start = MenuKeyframe.None;
            }
            else if (finish != MenuKeyframe.None)
            {
                // Snapping to end
                current = finish;
                finish = MenuKeyframe.None;
            }
            else // start && finish are both null
            {
                // Next Keyframe
                step++;
                MenuKeyframe next = FindCurrentKey();

                if (next != MenuKeyframe.None)
                {
                    StartAnimation(current, next);
                }
                else // This animation chain is done
                {
                    switch (currentState)
                    {
                        case State.Idle: SetState(State.Idle); break; // Loop
                        case State.Highlight: SetState(State.Highlight); break; // Loop
                        case State.Open: SetState(State.Idle); break;
                        default: break; // All done!
                    }
                }
            }

            this.elapsed += elapsed;
        }

        public void Draw(SpriteBatch batch)
        {
            for (int i = 0; i < Assets.Count; i++)
            {
                Assets[i].Draw(batch, current);
            }
        }
    }
}