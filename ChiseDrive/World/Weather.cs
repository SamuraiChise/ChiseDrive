using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.World
{
    public class Weather : IWeather
    {
        Wind wind;
        public Wind Wind
        {
            get
            {
                return wind;
            }
            set
            {
                wind = value;
            }
        }

        public WeatherIntensity Intensity 
        {
            get { return WeatherIntensity.None; }
            set { }
        }
        public bool Visible
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public Weather(Wind wind)
        {
            Wind = wind;
        }

        public void Dispose()
        {
            wind = null;
        }

        public void Update(Time elapsed)
        {
            Wind.Update(elapsed);
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, Cameras.Camera camera)
        {
        }
    }
}