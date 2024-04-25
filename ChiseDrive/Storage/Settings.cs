using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using ChiseDrive.Storage;

namespace ChiseDrive.Storage
{
    public class Settings : GameFile
    {
        // Debug/Tuning Settings
        public bool AutoStart = false;
        public bool SimulateTrialMode = false;

        public short PreferredWidth = 1280;
        public short PreferredHeight = 720;

        public short PreferredMultiSample = 1;

        public float MusicVolume = 1f;

        public float NearCamera = 0.01f;
        public float FarCamera = 400000f;

        bool vibration = true;
        public virtual bool Vibration { get { return vibration; } set { vibration = value; Input.ControllerVibration.NoVibration = !value; } }

        public Settings(String title)
            : base(title, "Settings")
        {
        }

        protected override void Read() { }
        protected override void Write() { }
    }
}