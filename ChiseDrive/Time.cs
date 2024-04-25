using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive
{
    public struct Time
    {
        static string[] TimerStrings = new string[60 * 10];
        public static void InitializeStrings()
        {
            for (int i = 0; i < TimerStrings.Length; i++)
            {
                TimerStrings[i] = String.Format("{0:00}:{1:00}", i/60, i%60);
            }
        }

        public static float TargetFPS = 60f;
        public static float TargetFPM = 3600f;//TargetFPSx60
        public static readonly Time Zero = new Time(0);

        float elapsed;

        public long Ticks { get { return (long)(Seconds * 10000000f); } }

        Time(int frames) { elapsed = (float)frames; }

        public static Time FromMinutes(float t)
        {
            Time retval;
            retval.elapsed = t * TargetFPM;
            return retval;
        }

        public static Time FromSeconds(float t)
        {
            Time retval;
            retval.elapsed = t * TargetFPS;
            return retval;
        }

        public static Time FromFrames(float t)
        {
            Time retval;
            retval.elapsed = t;
            return retval;
        }

        public static Time FromGameTime(GameTime gameTime)
        {
            Time retval;
            retval.elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * TargetFPS;
            return retval;
        }

        public static Time FromFPS(float fps)
        {
            Time retval;
            retval.elapsed = TargetFPS / fps;
            return retval;
        }

        // ACCESSORS **********************************************************

        public float Seconds { get { return elapsed / TargetFPS; } }
        public float Frames { get { return elapsed; } }

        /// <summary>
        /// Returns true if the time is equal to zero.
        /// </summary>
        public bool IsZero { get { return elapsed == 0f ? true : false; } }

        // OPERATORS **********************************************************

        public static implicit operator float(Time t)
        {
            return t.elapsed;
        }

        public static implicit operator Time(float f)
        {
            Time time = new Time();
            time.elapsed = f;
            return time;
        }

        // MEMBERS ************************************************************

        public float Percent(Time time)
        {
            return time.elapsed / elapsed;
        }

        public override string ToString()
        {
            int minutes = (int)elapsed / (int)TargetFPM;
            int seconds = ((int)elapsed - minutes * (int)TargetFPM) / (int)TargetFPS;
            int frames = (int)elapsed - minutes * (int)TargetFPM - seconds * (int)TargetFPS;

            return String.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, frames);
        }

        public string ToSeconds()
        {
            int minutes = (int)elapsed / (int)TargetFPM;
            int seconds = ((int)elapsed - minutes * (int)TargetFPM) / (int)TargetFPS;
            //return String.Format("{0:00}:{1:00}", minutes, seconds);

            return TimerStrings[minutes * 60 + seconds];
        }
    }

    /// <summary>
    /// A structure based on the Time structure.  Must be positive or zero.
    /// </summary>
    public struct Timer
    {
        Time remaining;

        public void Set(Time t)
        {
            remaining = t;
            if (remaining < 0f) remaining = 0f;
        }

        public void Set(float f)
        {
            if (f < 0f) f = 0f;
            remaining = Time.FromFrames(f);
        }

        public Timer(Time t)
        {
            remaining = t;
            if (remaining < 0f) remaining = 0f;
        }
        
        public static Timer FromSeconds(float t)
        {
            Timer retval;
            retval.remaining = Time.FromSeconds(t);

            if (retval.remaining < 0f) retval.remaining = 0f;
            return retval;
        }

        public static Timer FromFrames(float t)
        {
            Timer retval;
            retval.remaining = Time.FromFrames(t);

            if (retval.remaining < 0f) retval.remaining = 0f;
            return retval;
        }

        public float Percent(Time time)
        {
            return remaining.Percent(time);
        }

        public Time Remaining { get { return remaining; } set { remaining = value; } }
        public float Seconds { get { return remaining.Seconds; } }
        public float Frames { get { return remaining.Frames; } }
        public bool IsZero { get { return remaining.IsZero; } }

        public override string ToString()
        {
            return remaining.ToString();
        }

        public string ToSeconds()
        {
            return remaining.ToSeconds();
        }

        public void AddTime(Time elapsed)
        {
            remaining += elapsed;
        }

        public void SubTime(Time elapsed)
        {
            float f = remaining.Frames - elapsed.Frames;
            if (f < 0f) f = 0f;
            remaining = Time.FromFrames(f);
        }

        public static bool operator >(Timer lhs, Timer rhs)
        {
            return lhs.Remaining > rhs.Remaining;
        }

        public static bool operator <(Timer lhs, Timer rhs)
        {
            return lhs.Remaining < rhs.Remaining;
        }

        public static Timer operator /(Timer lhs, Timer rhs)
        {
            Timer timer = new Timer();
            timer.remaining = lhs.remaining.Frames / rhs.remaining.Frames;
            return timer;
        }
    }
}
