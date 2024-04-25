using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Cameras;

namespace ChiseDrive.World
{
    public enum WeatherIntensity
    {
        None,
        Mild,
        Moderate,
        Stormy,
        Catastrophe,
    };

    public interface IWeather : IDisposable
    {
        WeatherIntensity Intensity { get; set; }
        bool Visible { get; set; }
        void Update(Time elapsed);
        void Draw(GraphicsDevice device, Camera camera);
    }
}
