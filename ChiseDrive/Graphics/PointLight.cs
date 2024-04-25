using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Graphics
{
    public enum LightScope
    {
        None,
        Global,
        Local
    };

    public class PointLight : IDisposable
    {
        static List<PointLight> GlobalLightList = new List<PointLight>();
        static List<PointLight> MasterLightList = new List<PointLight>();
        static public void ClearAllChanged()
        {
            foreach (PointLight pl in MasterLightList)
            {
                pl.positionChanged = false;
                pl.rangeChanged = false;
                pl.colorChanged = false;
                pl.falloffChanged = false;
                pl.visibilityChanged = false;
            }
            globalLightsChanged = false;
        }
        static public List<PointLight> GlobalLights
        {
            get
            {
                return GlobalLightList;
            }
        }
        static bool globalLightsChanged = false;
        static public bool GlobalLightsChanged { get { return globalLightsChanged; } }

        public LightScope Scope
        {
            set
            {
                scope = value;

                if (value == LightScope.Global)
                {
                    if (!GlobalLightList.Contains(this))
                    {
                        GlobalLightList.Add(this);
                        globalLightsChanged = true;
                    }
                }
                else
                {
                    if (GlobalLightList.Contains(this))
                    {
                        GlobalLightList.Remove(this);
                        globalLightsChanged = true;
                    }
                }
            }
        }
        LightScope scope;

        float range = 30f;
        float falloff = 2f;
        Vector4 position = Vector4.Zero;
        Color color = Color.White;

        bool positionChanged = false;
        bool rangeChanged = false;
        bool colorChanged = false;
        bool falloffChanged = false;
        bool visibilityChanged = false;

        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (scope == LightScope.Global)
                {
                    if (value && !GlobalLightList.Contains(this)) GlobalLightList.Add(this);
                    if (!value && GlobalLightList.Contains(this)) GlobalLightList.Remove(this);
                    globalLightsChanged = true;
                }
                visible = value;
                visibilityChanged = true;
            }
        }
        bool visible = true;

        public PointLight(Vector4 position, Color color)
        {
            this.position = position;
            this.color = color;
            scope = LightScope.Local;
            MasterLightList.Add(this);
        }

        public PointLight(Vector3 position, Color color, float range)
        {
            this.position = new Vector4(position, 1f);
            this.color = color;
            this.range = range;
            scope = LightScope.Local;
            MasterLightList.Add(this);
        }

        public void Dispose()
        {
            Scope = LightScope.Local;
            MasterLightList.Remove(this);
        }

        public Vector4 Position
        {
            set
            {
                position = value;
                positionChanged = true;
            }
            get
            {
                return position;
            }
        }
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                colorChanged = true;
            }
        }
        public float Range
        {
            get
            {
                return range;
            }
            set
            {
                range = value;
                rangeChanged = true;
            }
        }
        public float Falloff
        {
            get
            {
                return falloff;
            }
            set
            {
                falloff = value;
                falloffChanged = true;
            }
        }

        public void UpdateChangedParameters(EffectParameter parameter)
        {
            if (visibilityChanged && !Visible)
            {
                parameter.StructureMembers["Range"].SetValue(0f);
            }
            else if (visibilityChanged && Visible)
            {
                parameter.StructureMembers["Range"].SetValue(range);
            }
            else if (!visibilityChanged && Visible && rangeChanged)
            {
                parameter.StructureMembers["Range"].SetValue(range);
            }

            if (positionChanged) parameter.StructureMembers["Position"].SetValue(position);
            if (falloffChanged) parameter.StructureMembers["Falloff"].SetValue(falloff);
            if (colorChanged) parameter.StructureMembers["Color"].SetValue(color.ToVector4());
        }

        public void UpdateAllParameters(EffectParameter paramater)
        {
            paramater.StructureMembers["Position"].SetValue(position);
            paramater.StructureMembers["Falloff"].SetValue(falloff);
            paramater.StructureMembers["Range"].SetValue(range);
            paramater.StructureMembers["Color"].SetValue(color.ToVector4());
        }
    }
}
