using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ChiseDrive.Graphics
{
    public struct MaterialSettings
    {
        public string DiffuseName;
        public string NormalName;
        public string SpecularName;
        public string EmittanceName;

        public float SpecularPower;
        public float SpecularIntensity;
        public Color Color;
    }

    public class Material
    {
        public static Material White = new Material(Color.White);

        // References
        Texture2D diffuseMap = null;
        Texture2D normalMap = null;
        Texture2D specularMap = null;
        Texture2D emittanceMap = null;

        public Texture2D Diffuse { get { return diffuseMap; } }
        public Texture2D Normal { get { return normalMap; } }
        public Texture2D Specular { get { return specularMap; } }
        public Texture2D Emittance { get { return emittanceMap; } }

        // Values
        MaterialSettings settings;

        public Material(ContentManager content, string settingsName) 
        {
            settings = content.Load<MaterialSettings>(settingsName);

            if (settings.DiffuseName != "") diffuseMap = content.Load<Texture2D>(settings.DiffuseName);
            if (settings.NormalName != "") normalMap = content.Load<Texture2D>(settings.NormalName);
            if (settings.SpecularName != "") specularMap = content.Load<Texture2D>(settings.SpecularName);
            if (settings.EmittanceName != "") emittanceMap = content.Load<Texture2D>(settings.EmittanceName);
        }

        public Material(Material copy)
        {
            settings = copy.settings;

            diffuseMap = copy.diffuseMap;
            normalMap = copy.normalMap;
            specularMap = copy.specularMap;
            emittanceMap = copy.emittanceMap;
        }

        Material(Color color)
        {
            settings = new MaterialSettings();
            settings.Color = color;
            settings.SpecularIntensity = 1f;
            settings.SpecularPower = 1f;
        }

        /// <summary>
        /// This sets all the values for the effect.  This should only be
        /// called once when the object is created, as copying data to the
        /// graphics card can be an expensive operation.
        /// </summary>
        /// <param name="effect">The effect to apply values to.</param>
        public void SetEffectValues(Effect effect)
        {
            effect.Parameters["SpecularPower"].SetValue(settings.SpecularPower);
            effect.Parameters["SpecularIntensity"].SetValue(settings.SpecularIntensity);
            effect.Parameters["MaterialColor"].SetValue(settings.Color.ToVector4());

            effect.Parameters["DiffuseTexture"].SetValue(diffuseMap);
            effect.Parameters["NormalTexture"].SetValue(normalMap);
            effect.Parameters["SpecularTexture"].SetValue(specularMap);
            effect.Parameters["EmittanceTexture"].SetValue(emittanceMap);

            effect.Parameters["DiffuseTextureEnabled"].SetValue(diffuseMap != null ? true : false);
            effect.Parameters["NormalTextureEnabled"].SetValue(normalMap != null ? true : false);
            effect.Parameters["SpecularTextureEnabled"].SetValue(specularMap != null ? true : false);
            effect.Parameters["EmittanceTextureEnabled"].SetValue(emittanceMap != null ? true : false);
        }
    }
}