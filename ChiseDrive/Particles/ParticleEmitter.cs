#region File Description
//-----------------------------------------------------------------------------
// ParticleEmitter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Graphics;
using ChiseDrive.Units;
#endregion

namespace ChiseDrive.Particles
{
    /// <summary>
    /// Helper for objects that want to leave particles behind them as they
    /// move around the world. This emitter implementation solves two related
    /// problems:
    /// 
    /// If an object wants to create particles very slowly, less than once per
    /// frame, it can be a pain to keep track of which updates ought to create
    /// a new particle versus which should not.
    /// 
    /// If an object is moving quickly and is creating many particles per frame,
    /// it will look ugly if these particles are all bunched up together. Much
    /// better if they can be spread out along a line between where the object
    /// is now and where it was on the previous frame. This is particularly
    /// important for leaving trails behind fast moving objects such as rockets.
    /// 
    /// This emitter class keeps track of a moving object, remembering its
    /// previous position so it can calculate the velocity of the object. It
    /// works out the perfect locations for creating particles at any frequency
    /// you specify, regardless of whether this is faster or slower than the
    /// game update rate.
    /// </summary>
    public class ParticleEmitter : ILightEmitter
    {
        #region Fields

        ParticleSystem particleSystem;
        float timeBetweenParticles;
        float timeLeftOver;

        PointLight light;
        Color lightColorMin;
        Color lightColorMax;
        float lightRangeMax;
        float lightRangeMin;

        Cooldown activeemittime = new Cooldown(5f);
        float emitspeed;
        IFollow attachment;
        Cue soundeffect;

        Vector3 previousPosition;

        bool loop = false;
        bool active = false;
        bool lightactive = false;

        Cooldown activelighttime;

        float percent = 1f;
        #endregion

        /// <summary>
        /// The percentage of the emit effect to release
        /// </summary>
        public float EmitPercent
        {
            get
            {
                return percent;
            }
            set
            {
                percent = value;
                SetLightRange(value);
            }
        }
        public bool IsDone
        {
            get
            {
                if (!active && !lightactive) return true;
                else return false;
            }
        }
        public List<PointLight> Lights
        {
            get
            {
                List<PointLight> retvalue = new List<PointLight>();
                if (this.lightRangeMax > 0f) retvalue.Add(light);
                return retvalue;
            }
        }

        /// <summary>
        /// Creates a particle emitter.
        /// </summary>
        /// <param name="particleSystem">Reference to the particle system.</param>
        /// <param name="particlesPerSecond">Particles created per second.</param>
        /// <param name="emittime">Ammount of time spent per emit call.</param>
        /// <param name="light">A light effect to show during emittance (nullable)</param>
        /// <param name="soundeffect">The sound effect to play (nullable)</param>
        /// <param name="attachment">What this emitter is attached to.</param>
        public ParticleEmitter(
            IFollow attachment,
            ParticleEmitterSettings settings)
        {
            this.particleSystem = ParticleSystem.TryGetCreateSystem(settings.ParticleSystemName);
            timeBetweenParticles = 1.0f / settings.ParticlesPerSecond;

            if (settings.LightRangeMax == 0f)
            {
                this.light = null;
            }
            else
            {
                this.light = new PointLight(
                    new Vector4(attachment.Position, 1f),
                    settings.LightColorLow);
                this.light.Scope = settings.LightScope;
                this.light.Falloff = settings.LightFalloff;
            }
            lightColorMin = settings.LightColorLow;
            lightColorMax = settings.LightColorHigh;
            
            this.lightRangeMax = settings.LightRangeMax;
            this.lightRangeMin = settings.LightRangeMin;
            SetLightRange(0f);

            this.emitspeed = settings.ParticleExitSpeed;
            this.activeemittime = new Cooldown(Time.FromSeconds(settings.ParticlesPerEmit * timeBetweenParticles));
            this.activelighttime = new Cooldown(settings.LightFrames);

            // TODO: Prepare the sound effect!
            this.soundeffect = null;

            this.attachment = attachment;
            this.previousPosition = attachment.Position;
        }

        public void Dispose()
        {
            if (this.light != null)
            {
                this.light.Dispose();
            }

            this.particleSystem = null;
            this.light = null;
            this.attachment = null;
            this.activeemittime = null;
        }

        public void EmitOnce()
        {
            activeemittime.Trigger();
            activelighttime.Trigger();
            active = true;
            if (light != null) lightactive = true;
            previousPosition = attachment.Position;

            if (this.light != null && this.lightRangeMax > 0f)
            {
                light.Visible = true;
            }
        }

        public void EmitLoop()
        {
            loop = true;
            EmitOnce();
        }

        public void StopLoop()
        {
            loop = false;
        }

        public void Reset()
        {
            StopLoop();
            active = false;
            particleSystem.Update(Time.FromSeconds(120f));
        }

        void SetLightRange(float percent)
        {
            if (this.light != null)
                this.light.Range = ((lightRangeMax - lightRangeMin) * percent) + lightRangeMin;
        }
        /// <summary>
        /// Updates the emitter, creating the appropriate number of particles
        /// in the appropriate positions.
        /// </summary>
        public void Update(Time elapsed)
        {
            if (active)
            {
                if (loop && activeemittime.IsReady) activeemittime.Trigger();

                Vector3 newPosition = attachment.Position;

                if (this.light != null && lightactive)
                {
                    if (activelighttime.IsReady)
                    {
                        if (loop) activelighttime.Trigger();
                        else
                        {
                            this.light.Visible = lightactive = false;
                            SetLightRange(0f);
                        }
                    }
                    else // Update the light
                    {
                        float totaltime = activelighttime.ResetTime;
                        float currentime = activelighttime.RemainingTime;
                        float percent = currentime / totaltime;

                        // Increase percent the first half of the light span
                        if (percent < 0.5f) percent *= 2f;
                        // Decrease percent the second half of the light span
                        else percent = (1f - percent) * 2f;

                        SetLightRange(percent);
                        this.light.Position = new Vector4(newPosition, 1f);
                        this.light.Color = Color.Lerp(lightColorMin, lightColorMax, Helper.Randomf);
                    }
                }

                if (activeemittime.IsReady)
                {
                    if (!lightactive) active = false;
                }
                else
                {
                    // Work out how much time has passed since the previous update.
                    float elapsedTime = elapsed.Seconds;

                    if (elapsedTime > 0f && percent > 0f)
                    {
                        // If we had any time left over that we didn't use during the
                        // previous update, add that to the current elapsed time.
                        float timeToSpend = timeLeftOver + elapsedTime;
                        timeToSpend *= percent;

                        // Counter for looping over the time interval.
                        float currentTime = -timeLeftOver;

                        Vector3 deltaPrime = previousPosition - newPosition;//newPosition - previousPosition;

                        // Create particles as long as we have a big enough time interval.
                        while (timeToSpend > timeBetweenParticles)
                        {
                            currentTime += timeBetweenParticles;
                            timeToSpend -= timeBetweenParticles;

                            // Work out the optimal position for this particle. This will produce
                            // evenly spaced particles regardless of the object speed, particle
                            // creation frequency, or game update rate.
                            float mu = currentTime / (elapsedTime * percent);

                            Vector3 delta = deltaPrime * mu;// (attachment.Velocity * mu);
                            Vector3 emitkick = Vector3.Zero;

                            Vector3 position;

                            if (emitspeed != 0f)
                            {
                                Vector3 emitdir = attachment.RotationPosition.Up;
                                Helper.Normalize(ref emitdir);

                                emitkick = emitdir * emitspeed;
                                position = newPosition + emitkick + delta;
                            }
                            else
                            {
                                position = newPosition + delta;
                            }

                            // We used last update's velocity to determine how to spread
                            // our particles out, however, we're using this update's
                            // velocity to figure out what the exit velocity is going to be!
                            particleSystem.AddParticle(position, attachment.Velocity);
                        }

                        // Store any time we didn't use, so it can be part of the next update.
                        timeLeftOver = timeToSpend;
                    }
                }

                previousPosition = newPosition;
            }

            activeemittime.Update(elapsed);
            activelighttime.Update(elapsed);
        }
    }
}
