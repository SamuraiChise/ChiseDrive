using System;
using System.Collections.Generic;
using System.Text;

namespace ChiseDrive
{
    public class Cooldown
    {
        Timer cooldown = new Timer(Time.Zero);
        readonly Timer reset = new Timer(Time.Zero);

        public bool IsReady 
        { 
            get 
            {
                return cooldown.IsZero ? true : false; 
            } 
        }

        public Time ResetTime
        {
            get
            {
                return reset.Remaining;
            }
        }
        public Time RemainingTime
        {
            get
            {
                return cooldown.Remaining;
            }
        }

        public bool AlmostReady
        {
            get
            {
                return cooldown.Frames < 3f ? true : false;
            }
        }

        public Cooldown(Time totaltime)
        {
            reset.Set(totaltime);
        }

        public Cooldown(float totalframes)
        {
            reset.Set(Time.FromFrames(totalframes));
        }

        public Cooldown(Cooldown copy)
        {
            this.reset = copy.reset;
            this.cooldown = copy.cooldown;
        }

        public void Trigger()
        {
            cooldown = reset;
        }

        public void Update(Time elapsed)
        {
            cooldown.SubTime(elapsed);
        }

        public bool AutoTrigger(Time elapsed)
        {
            Update(elapsed);
            if (IsReady)
            {
                Trigger();
                return true;
            }
            return false;
        }
    }
}
