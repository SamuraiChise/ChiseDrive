#region File Description
//-----------------------------------------------------------------------------
// InstancedModel.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace ChiseDrive.Graphics.InstancedModel
{
    /// <summary>
    /// Enum describes the various possible techniques
    /// that can be chosen to implement instancing.
    /// </summary>
    public enum InstancingTechnique
    {
#if XBOX360
        VFetchInstancing,
#else
        HardwareInstancing,
        ShaderInstancing,
#endif
        NoInstancing,
        NoInstancingOrStateBatching
    }


    /// <summary>
    /// Custom model class can efficiently draw many copies of itself,
    /// using various different GPU instancing techniques.
    /// </summary>
    public class InstancedModel
    {
        #region Fields


        // Internally our custom model is made up from a list of model parts.
        // Most of the interesting code lives in the InstancedModelPart class.
        List<InstancedModelPart> modelParts = new List<InstancedModelPart>();


        // Keep track of what graphics device we are using.
        GraphicsDevice graphicsDevice;


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor reads instanced model data from our custom XNB format.
        /// </summary>
        internal InstancedModel(ContentReader input)
        {
            // Look up our graphics device.
            graphicsDevice = GetGraphicsDevice(input);

            // Load the model data.
            int partCount = input.ReadInt32();

            for (int i = 0; i < partCount; i++)
            {
                modelParts.Add(new InstancedModelPart(input, graphicsDevice));
            }

            // Choose the best available instancing technique.
            InstancingTechnique technique = 0;

            while (!IsTechniqueSupported(technique))
                technique++;

            SetInstancingTechnique(technique);
        }


        /// <summary>
        /// Helper uses the IGraphicsDeviceService interface to find the GraphicsDevice.
        /// </summary>
        static GraphicsDevice GetGraphicsDevice(ContentReader input)
        {
            IServiceProvider serviceProvider = input.ContentManager.ServiceProvider;

            IGraphicsDeviceService deviceService =
                (IGraphicsDeviceService)serviceProvider.GetService(
                                            typeof(IGraphicsDeviceService));

            return deviceService.GraphicsDevice;
        }


        #endregion

        #region Technique Selection


        /// <summary>
        /// Gets the current instancing technique.
        /// </summary>
        public InstancingTechnique InstancingTechnique
        {
            get { return instancingTechnique; }
        }

        InstancingTechnique instancingTechnique;


        /// <summary>
        /// Changes which instancing technique we are using.
        /// </summary>
        public void SetInstancingTechnique(InstancingTechnique technique)
        {
            instancingTechnique = technique;

            foreach (InstancedModelPart modelPart in modelParts)
            {
                modelPart.Initialize(technique);
            }
        }


        /// <summary>
        /// Checks whether the specified instancing technique
        /// is supported by the current graphics device.
        /// </summary>
        public bool IsTechniqueSupported(InstancingTechnique technique)
        {
#if !XBOX360
            // Hardware instancing is only supported on pixel shader 3.0 devices.
            if (technique == InstancingTechnique.HardwareInstancing)
            {
                return graphicsDevice.GraphicsDeviceCapabilities
                                     .PixelShaderVersion.Major >= 3;
            }
#endif

            // Otherwise, everything is good.
            return true;
        }


        #endregion


        /// <summary>
        /// Draws a batch of instanced models.
        /// </summary>
        public void DrawInstances(Matrix[] instanceTransforms,
                                  Matrix view, Matrix projection)
        {
            if (instanceTransforms.Length == 0)
                return;

            foreach (InstancedModelPart modelPart in modelParts)
            {
                modelPart.Draw(instancingTechnique, instanceTransforms,
                               view, projection);
            }
        }
    }
}
