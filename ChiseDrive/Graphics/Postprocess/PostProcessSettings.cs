#region File Description
//-----------------------------------------------------------------------------
// BloomSettings.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace ChiseDrive.Graphics
{
    /// <summary>
    /// Class holds all the settings used to tweak the bloom effect.
    /// </summary>
    public class PostProcessSettings
    {
        #region Fields


        // Name of a preset bloom setting, for display to the user.
        public readonly string Name;


        // Controls how bright a pixel needs to be before it will bloom.
        // Zero makes everything bloom equally, while higher values select
        // only brighter colors. Somewhere between 0.25 and 0.5 is good.
        public readonly float BloomThreshold;


        // Controls how much blurring is applied to the bloom image.
        // The typical range is from 1 up to 10 or so.
        public readonly float BlurAmount;


        // Controls the amount of the bloom and base images that
        // will be mixed into the final scene. Range 0 to 1.
        public readonly float BloomIntensity;
        public readonly float BaseIntensity;


        // Independently control the color saturation of the bloom and
        // base images. Zero is totally desaturated, 1.0 leaves saturation
        // unchanged, while higher values increase the saturation level.
        public readonly float BloomSaturation;
        public readonly float BaseSaturation;

        public readonly float EdgeWidth;
        public readonly float EdgeIntensity;

        public bool EdgeDetect { get { return EdgeIntensity > 0 && EdgeWidth > 0; } }
        #endregion


        /// <summary>
        /// Constructs a new bloom settings descriptor.
        /// </summary>
        public PostProcessSettings(
            string name, float bloomThreshold, float blurAmount,
            float bloomIntensity, float baseIntensity,
            float bloomSaturation, float baseSaturation,
            float edgeWidth, float edgeIntensity)
        {
            Name = name;
            BloomThreshold = bloomThreshold;
            BlurAmount = blurAmount;
            BloomIntensity = bloomIntensity;
            BaseIntensity = baseIntensity;
            BloomSaturation = bloomSaturation;
            BaseSaturation = baseSaturation;
            EdgeWidth = edgeWidth;
            EdgeIntensity = edgeIntensity;
        }


        /// <summary>
        /// Table of preset bloom settings, used by the sample program.
        /// </summary>
        public static PostProcessSettings[] PresetSettings =
        {
            //                      Name           Thresh  Blur Bloom  Base  BloomSat BaseSat EdgeWid EdgeInt
            new PostProcessSettings("Default",     0.25f,  4,   1.25f, 1,    1,       1,      0,      0),
            new PostProcessSettings("Soft",        0,      3,   1,     1,    1,       1,      0,      0),
            new PostProcessSettings("Desaturated", 0.5f,   8,   2,     1,    0,       1,      0,      0),
            new PostProcessSettings("Saturated",   0.25f,  4,   2,     1,    2,       0,      0,      0),
            new PostProcessSettings("Blurry",      0,      2,   1,     0.1f, 1,       1,      0,      0),
            new PostProcessSettings("Subtle",      0.5f,   2,   1,     1,    1,       1,      0,      0),
            new PostProcessSettings("EdgeDetect",  0,      0f,  0f,    1f,   1,       1,      1,      1),
        };
    }
}
