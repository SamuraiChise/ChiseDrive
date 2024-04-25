using System;
using System.Collections.Generic;
using System.Text;

namespace ChiseDrive
{
    class OperatorError
    {
        #region Fields
        float value;        // Whatever this operator is modifying
        float driftvalue;

        float acceleration; // The rate of change

        float dampen;       // The percent that the operator errors and must correct for
        int oscillations;   // The maximum number of occilations before clamping
        float speed;        // The maximum rate of change

        float drift;        // The drift ammount
        Cooldown driftchange;

        float echo;         // Echo of big changes
        float echoreduce;
        float echospeed;
        Cooldown echorepeat;

        float maxdampen;
        int maxoscillations;
        float maxspeed;
        float minimumspeed; // The minimum-maximum speed
        float minimumaccel; // The minimum-maximum accel
        #endregion

        #region Properties
        public float CurrentValue
        {
            get
            {
                return value;
            }
        }
        #endregion

        public OperatorError(float initialvalue, float acceleration, float speed, int oscillations, float dampen, float drift, float echo)
        {
            Reset(initialvalue);
            this.minimumaccel = this.acceleration = acceleration;
            this.minimumspeed = this.maxspeed = speed;
            this.maxoscillations = oscillations;
            this.maxdampen = dampen;

            if (drift != 0f)
            {
                driftchange = new Cooldown(drift);
                this.drift = acceleration * 0.01f;
            }
            else
            {
                drift = 0f;
            }

            if (echo != 0f)
            {
                echorepeat = new Cooldown(echo);
            }

            if (acceleration <= 0f) throw new SystemException("Acceleration values must be positive.");
            if (oscillations < 0f) throw new SystemException("Occilations are not allowed to be negative.");
            if (dampen < 0f) throw new SystemException("Errorpercent is not allowed to be negative.");
            if (speed < 0f) throw new SystemException("Speed values must be positive.");
        }

        public void Reset(float value)
        {
            this.value = value;
            this.driftvalue = value;
            this.oscillations = 0;
            this.dampen = 1f;
            this.speed = 0f;
            this.echo = 0f;
            this.echoreduce = 0f;
            this.echospeed = 0f;
        }

        public void SetSpeed(float value, float destination, float increment)
        {
            float distance = destination - value;
            float newmax = Math.Abs(distance / increment);

            if (newmax > this.minimumspeed) this.maxspeed = newmax;
            else this.maxspeed = this.minimumspeed;

            float newaccel = newmax * 0.1f;

            if (newaccel > this.minimumaccel) this.acceleration = newaccel;
            else this.acceleration = this.minimumaccel;
        }

        bool CheckDestination(float destination)
        {
            if (value == destination)
            {
                this.oscillations = 0;
                this.dampen = 1f;
                this.speed = 0f;
                return true;
            }
            else
            {
                this.echoreduce = 0f;
                this.echospeed = 0f;
                return false;
            }
        }

        void IncreaseOscilations(float destination)
        {
            oscillations++;

            // At the final oscillation, just stop
            if (oscillations >= maxoscillations)
            {
                value = destination;
                dampen = 1f;
            }
            else if (oscillations > 0)
            {
                dampen = maxdampen / oscillations;
            }
        }

        public float MoveTo(float destination, Time elapsed)
        {
            if (CheckDestination(destination))
            {
                if (driftchange != null)
                {
                    driftvalue += drift * elapsed;

                    if (driftchange.AutoTrigger(elapsed))
                    {
                        drift = -drift;
                    }
                }
                if (echorepeat != null && echorepeat.ResetTime != Time.Zero)
                {
                    echorepeat.Update(elapsed);
                    if (echorepeat.IsReady)
                    {
                        echoreduce *= 0.5f;
                        if (Math.Abs(echoreduce) < maxspeed)
                        {
                            echoreduce = 0f;
                        }

                        if (echoreduce > 0f)
                        {
                            echospeed += acceleration;
                            if (echospeed > maxspeed) echospeed = maxspeed;
                        }
                        else if (echoreduce < 0f)
                        {
                            echospeed -= acceleration;
                            if (echospeed < -maxspeed) echospeed = -maxspeed;
                        }
                        else if (echospeed > 0)
                        {
                            echospeed -= acceleration;
                            if (echospeed < 0) echospeed = 0;
                        }
                        else if (echospeed < 0)
                        {
                            echospeed += acceleration;
                            if (echospeed > 0) echospeed = 0;
                        }
                        else
                        {
                            // Swap for the next echo
                            echoreduce = -echo;
                            echo = 0f;
                            echorepeat.Trigger();
                        }

                        driftvalue += echospeed;
                    }
                }
                return driftvalue;
            }
            else//Not at our destination yet
            {
                // Calculate change in speed
                if (value < destination)
                {
                    speed += acceleration*elapsed;
                    if (speed > maxspeed) speed = maxspeed;
                }
                else if (value > destination)
                {
                    speed -= acceleration*elapsed;
                    if (speed < -maxspeed) speed = -maxspeed;
                }

                // Apply speed
                float oldvalue = value;
                float delta = speed * elapsed;
                delta *= dampen;
                value += delta;

                // Check to see if we've oscilated
                if (oldvalue < destination && value > destination)
                {
                    IncreaseOscilations(destination);
                }
                else if (oldvalue > destination && value < destination)
                {
                    IncreaseOscilations(destination);
                }

                driftvalue = value;
                echo += speed;

                Helper.Clamp(ref echo, -acceleration * 10f, acceleration * 10f);

                if (value == destination)
                {
                    echoreduce = echo;
                    if (echorepeat != null) echorepeat.Trigger();
                }

                return value;
            }
        }
    }
}