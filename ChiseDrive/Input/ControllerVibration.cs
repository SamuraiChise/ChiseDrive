using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChiseDrive.Input
{
    public class ControllerVibration
    {
        struct VibrationNode
        {
            public float high;
            public float low;
            public Time length;
        };

        bool pause = false;
        bool pauseProcessed = false;
        public bool Pause
        {
            set
            {
                pause = value;
                if (pause)
                {
                    pauseProcessed = GamePad.SetVibration(index, 0f, 0f);
                }
            }
        }
        public static bool NoVibration { get; set; }

        List<VibrationNode> nodes;
        PlayerIndex index;

        static ControllerVibration[] ControllerVibrations = new ControllerVibration[4];

        public static ControllerVibration FetchController(PlayerIndex index)
        {
            if (ControllerVibrations[(int)index] == null)
            {
                ControllerVibrations[(int)index] = new ControllerVibration(index);
            }

            return ControllerVibrations[(int)index];
        }

        ControllerVibration(PlayerIndex index)
        {
            nodes = new List<VibrationNode>();
            this.index = index;
            NoVibration = false;
        }

        static public void ResetAll()
        {
            foreach (ControllerVibration cv in ControllerVibrations)
            {
                if (cv != null) cv.Reset();
            }
        }

        static public void PauseAll()
        {
            foreach (ControllerVibration cv in ControllerVibrations)
            {
                if (cv != null) cv.Pause = true;
            }
        }

        static public void ResumeAll()
        {
            foreach (ControllerVibration cv in ControllerVibrations)
            {
                if (cv != null) cv.Pause = false;
            }
        }

        public void Reset()
        {
            nodes.Clear();
            pause = false;
        }

        public void AddVibrationEvent(float high, float low, Time length)
        {
            if (NoVibration) return;
            VibrationNode newnode = new VibrationNode();
            newnode.high = high;
            newnode.low = low;
            Helper.Clamp(ref newnode.high, 0f, 1f);
            Helper.Clamp(ref newnode.low, 0f, 1f);
            newnode.length = length;
            nodes.Add(newnode);
        }

        static public void UpdateAll(Time elapsed)
        {
            foreach (ControllerVibration cv in ControllerVibrations)
            {
                if (cv != null) cv.Update(elapsed);
            }
        }

        public void Update(Time elapsed)
        {
            if (pause)
            {
                if (!pauseProcessed)
                    pauseProcessed = GamePad.SetVibration(index, 0f, 0f);
                return;
            }
            float high = 0f;
            float low = 0f;
            float MaxTime = Time.FromSeconds(1f);

            for (int i = 0; i < nodes.Count; i++)
            {
                VibrationNode temp = nodes[i];
                temp.length -= elapsed;
                if (temp.length == Time.Zero)
                {
                    temp.high = 0f;
                    temp.low = 0f;
                }
                if (temp.length > MaxTime)
                {
                    temp.length = MaxTime;
                }
                nodes[i] = temp;

                if (nodes[i].high > high) high = nodes[i].high;
                if (nodes[i].low > low) low = nodes[i].low;
            }

            GamePad.SetVibration(index, low, high);

            nodes.RemoveAll(delegate(VibrationNode vn)
            {
                if (vn.length <= Time.Zero) return true;
                return false;
            });
        }
    }
}