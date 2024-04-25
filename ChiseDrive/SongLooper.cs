using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using ChiseDrive;

namespace ChiseDrive
{
    public class SongLooper : GameComponent
    {
        Cue song;
        string cuestring;

        SoundBank sounds;
        AudioCategory category;

        float RealVolume;
        float VolumeTarget;

        ChiseDriveGame Game;

        public SongLooper(ChiseDriveGame game, SoundBank sounds)
            : base(game)
        {
            Game = game;
            this.sounds = sounds;
            this.category = Game.AudioEngine.GetCategory("Music");

            RealVolume = 0f;
            SetVolumeTarget(1f);
        }

        public void SetVolumeTarget(float target)
        {
            VolumeTarget = target;

            Helper.Clamp(ref VolumeTarget, 0f, Game.Settings.MusicVolume);
        }

        public void PlaySong(string name)
        {
            if (Game.Settings.MusicVolume == 0f)
                return;

            if (cuestring != name)
            {
                cuestring = name;
                if (song != null && song.IsPlaying)
                {
                    song.Stop(AudioStopOptions.AsAuthored);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (VolumeTarget != RealVolume)
            {
                Helper.EaseTo(ref RealVolume, VolumeTarget, 0.05f, Time.FromGameTime(gameTime));
                category.SetVolume(RealVolume);
            }

            if (cuestring != null && (song == null || !song.IsPlaying))
            {
                song = sounds.GetCue(cuestring);
                song.Play();
            }
            base.Update(gameTime);
        }
    }
}