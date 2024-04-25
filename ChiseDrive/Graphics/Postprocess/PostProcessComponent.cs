#region File Description
//-----------------------------------------------------------------------------
// BloomComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace ChiseDrive.Graphics
{
    public class PostProcessComponent : IDisposable
    {
        #region Fields

        Effect bloomExtractEffect;
        Effect bloomCombineEffect;
        Effect gaussianBlurEffect;
        Effect edgeDetectEffect;

        ResolveTexture2D resolveTarget;
        RenderTarget2D renderTarget1;
        RenderTarget2D renderTarget2;

        RenderTarget2D sceneRenderTarget;
        RenderTarget2D normalDepthRenderTarget;

        // Choose what display settings the bloom should use.
        public PostProcessSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        PostProcessSettings settings = PostProcessSettings.PresetSettings[5];


        // Optionally displays one of the intermediate buffers used
        // by the bloom postprocess, so you can see exactly what is
        // being drawn into each rendertarget.
        public enum IntermediateBuffer
        {
            PreBloom,
            BlurredHorizontally,
            BlurredBothWays,
            FinalResult,
        }

        public IntermediateBuffer ShowBuffer
        {
            get { return showBuffer; }
            set { showBuffer = value; }
        }

        IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;


        #endregion

        #region Initialization

        ChiseDriveGame game;

        public PostProcessComponent(ChiseDriveGame game)
        {
            this.game = game;

            bloomExtractEffect = game.Content.Load<Effect>("Effects/BloomExtract");
            bloomCombineEffect = game.Content.Load<Effect>("Effects/BloomCombine");
            gaussianBlurEffect = game.Content.Load<Effect>("Effects/GaussianBlur");
            edgeDetectEffect = game.Content.Load<Effect>("Effects/Postprocess");

            RebuildBuffers();
        }

        public void RebuildBuffers()
        {
            // Look up the resolution and format of our main backbuffer.
            PresentationParameters pp = game.GraphicsDevice.PresentationParameters;

            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;

            // Create a texture for reading back the backbuffer contents.
            resolveTarget = new ResolveTexture2D(game.GraphicsDevice, width, height, 1,
                format);

            // Create two custom rendertargets.

            sceneRenderTarget = new RenderTarget2D(game.GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, 1,
                pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);

            normalDepthRenderTarget = new RenderTarget2D(game.GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, 1,
                pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);

            // Create two rendertargets for the bloom processing. These are half the
            // size of the backbuffer, in order to minimize fillrate costs. Reducing
            // the resolution in this way doesn't hurt quality, because we are going
            // to be blurring the bloom images in any case.
            width /= 2;
            height /= 2;

            renderTarget1 = new RenderTarget2D(game.GraphicsDevice, width, height, 1,
                pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);
            renderTarget2 = new RenderTarget2D(game.GraphicsDevice, width, height, 1,
                pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        public void Dispose()
        {
            resolveTarget.Dispose();
            renderTarget1.Dispose();
            renderTarget2.Dispose();
            game.GraphicsDevice.SetRenderTarget(0, null);
        }


        #endregion

        #region Draw

        public bool RenderNormals { get { return Settings.EdgeDetect; } }
        bool UsingBackBuffer { get { return !RenderNormals; } }
        public void SetNormalTargets ()
        {
            if (RenderNormals)
            {
                game.GraphicsDevice.SetRenderTarget(0, normalDepthRenderTarget);
                game.GraphicsDevice.Clear(Color.Black);
            }
        }

        public void SetRenderTargets()
        {
            if (UsingBackBuffer)
            {
                game.GraphicsDevice.SetRenderTarget(0, null);
            }
            else
            {
                game.GraphicsDevice.SetRenderTarget(0, sceneRenderTarget);
            }

            game.GraphicsDevice.Clear(Color.Black);
        }
        
        /// <summary>
        /// This is where it all happens. Grabs a scene that has already been rendered,
        /// and uses postprocess magic to add a glowing bloom effect over the top of it.
        /// </summary>
        public void ApplyPostProcess()
        {
            ProcessBloom();
            ProcessEdgeDetect();
        }

        void ProcessBloom()
        {
            EffectParameterCollection parameters = bloomCombineEffect.Parameters;

            Texture2D renderTexture = null;
            if (UsingBackBuffer)
            {
                game.GraphicsDevice.ResolveBackBuffer(resolveTarget);
                renderTexture = resolveTarget;
            }
            else
            {
                game.GraphicsDevice.SetRenderTarget(0, null);
                renderTexture = sceneRenderTarget.GetTexture();
            }
            
            // Pass 1: draw the scene into rendertarget 1, using a
            // shader that extracts only the brightest parts of the image.
            bloomExtractEffect.Parameters["BloomThreshold"].SetValue(
                Settings.BloomThreshold);
            
            DrawFullscreenQuad(renderTexture, renderTarget1,
                               bloomExtractEffect,
                               IntermediateBuffer.PreBloom);
            
            // Pass 2: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.
            SetBlurEffectParameters(1.0f / (float)renderTarget1.Width, 0);
            
            DrawFullscreenQuad(renderTarget1.GetTexture(), renderTarget2,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredHorizontally);
            
            // Pass 3: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            SetBlurEffectParameters(0, 1.0f / (float)renderTarget1.Height);

            DrawFullscreenQuad(renderTarget2.GetTexture(), renderTarget1,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredBothWays);
            
            // Pass 4: draw both rendertarget 1 and the original scene
            // image back into the main backbuffer, using a shader that
            // combines them to produce the final bloomed result.
            SetRenderTargets();

            parameters = bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(Settings.BloomIntensity);
            parameters["BaseIntensity"].SetValue(Settings.BaseIntensity);
            parameters["BloomSaturation"].SetValue(Settings.BloomSaturation);
            parameters["BaseSaturation"].SetValue(Settings.BaseSaturation);

            game.GraphicsDevice.Textures[1] = renderTexture;

            Viewport viewport = game.GraphicsDevice.Viewport;

            DrawFullscreenQuad(renderTarget1.GetTexture(),
                               viewport.Width, viewport.Height,
                               bloomCombineEffect,
                               IntermediateBuffer.FinalResult);
            

        }

        void ProcessEdgeDetect()
        {
            if (Settings.EdgeDetect)
            {
                EffectParameterCollection parameters = edgeDetectEffect.Parameters;

                // Drawing the edges
                game.GraphicsDevice.SetRenderTarget(0, null);

                string effectTechniqueName;

                // Set effect parameters controlling the pencil sketch effect.
                //if (Settings.EnableSketch)
                //{
                //    parameters["SketchThreshold"].SetValue(Settings.SketchThreshold);
                //    parameters["SketchBrightness"].SetValue(Settings.SketchBrightness);
                //    parameters["SketchJitter"].SetValue(sketchJitter);
                //    parameters["SketchTexture"].SetValue(sketchTexture);
                //}

                // Set effect parameters controlling the edge detection effect.
                if (Settings.EdgeDetect)
                {
                    Vector2 resolution = new Vector2(sceneRenderTarget.Width,
                                                     sceneRenderTarget.Height);

                    Texture2D normalDepthTexture = normalDepthRenderTarget.GetTexture();

                    parameters["EdgeWidth"].SetValue(Settings.EdgeWidth);
                    parameters["EdgeIntensity"].SetValue(Settings.EdgeIntensity);
                    parameters["ScreenResolution"].SetValue(resolution);
                    parameters["NormalDepthTexture"].SetValue(normalDepthTexture);

                    // Choose which effect technique to use.
                    //if (Settings.EnableSketch)
                    {
                        //if (Settings.SketchInColor)
                        //    effectTechniqueName = "EdgeDetectColorSketch";
                        //else
                        //    effectTechniqueName = "EdgeDetectMonoSketch";
                    }
                    //else
                    effectTechniqueName = "EdgeDetect";
                }
                else
                {
                    // If edge detection is off, just pick one of the sketch techniques.
                    //if (Settings.SketchInColor)
                    //    effectTechniqueName = "ColorSketch";
                    //else
                    //    effectTechniqueName = "MonoSketch";
                    effectTechniqueName = "EdgeDetect";
                }

                // Activate the appropriate effect technique.
                edgeDetectEffect.CurrentTechnique =
                                        edgeDetectEffect.Techniques[effectTechniqueName];

                // Draw a fullscreen sprite to apply the postprocessing effect.
                game.SpriteBatch.Begin(SpriteBlendMode.None,
                                  SpriteSortMode.Immediate,
                                  SaveStateMode.None);

                edgeDetectEffect.Begin();
                edgeDetectEffect.CurrentTechnique.Passes[0].Begin();

                game.SpriteBatch.Draw(sceneRenderTarget.GetTexture(), Vector2.Zero, Color.White);

                game.SpriteBatch.End();

                edgeDetectEffect.CurrentTechnique.Passes[0].End();
                edgeDetectEffect.End();

            }
        }


        /// <summary>
        /// Helper for drawing a texture into a rendertarget, using
        /// a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget,
                                Effect effect, IntermediateBuffer currentBuffer)
        {
            game.GraphicsDevice.SetRenderTarget(0, renderTarget);

            DrawFullscreenQuad(texture,
                               renderTarget.Width, renderTarget.Height,
                               effect, currentBuffer);

            game.GraphicsDevice.SetRenderTarget(0, null);
        }


        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuad(Texture2D texture, int width, int height,
                                Effect effect, IntermediateBuffer currentBuffer)
        {
            game.SpriteBatch.Begin(SpriteBlendMode.None,
                              SpriteSortMode.Immediate,
                              SaveStateMode.None);

            // Begin the custom effect, if it is currently enabled. If the user
            // has selected one of the show intermediate buffer options, we still
            // draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
            if (showBuffer >= currentBuffer)
            {
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
            }

            // Draw the quad.
            game.SpriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            game.SpriteBatch.End();

            // End the custom effect.
            if (showBuffer >= currentBuffer)
            {
                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }

        const int SampleCount = 30;
        float[] sampleWeights = new float[SampleCount];
        Vector2[] sampleOffsets = new Vector2[SampleCount];

        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;
            
            // Create temporary arrays for computing our filter settings.
            //float[] sampleWeights = new float[sampleCount];
            //Vector2[] sampleOffsets = new Vector2[sampleCount];
            if (sampleCount > SampleCount) throw new Exception("Increase the size of SampleCount");

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];
            
            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }


        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        float ComputeGaussian(float n)
        {
            float theta = Settings.BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }


        #endregion
    }
}
