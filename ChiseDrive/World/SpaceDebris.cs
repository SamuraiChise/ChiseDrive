using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Cameras;
using ChiseDrive.Graphics;
using ChiseDrive.Graphics.InstancedModel;

namespace ChiseDrive.World
{/*
    public class SpaceDebris : IWorldComponent
    {
        static List<SpaceDebris> DrawList = new List<SpaceDebris>();
        static public void DrawAll(GraphicsDevice device, Camera camera)
        {
            if (DrawList.Count < 1) return;

            if (device == null) throw new NullReferenceException();
            if (camera == null) throw new NullReferenceException();

            foreach (SpaceDebris mesh in DrawList)
            {
                mesh.Draw(device, camera);
            }
        }

        public bool Visible
        {
            get
            {
                return DrawList.Contains(this);
            }
            set
            {
                if (value && !Visible) DrawList.Add(this);
                if (!value && Visible) DrawList.Remove(this);
            }
        }

        public PointLight Light { get { return null; } }

        const int PatchCount = 1;
        const int NoPatch = -1;
        DebrisPatch[] patch;

        Cooldown checkclosest = new Cooldown(Time.FromSeconds(0.3f));

        InstancedModel instancedModel;
        Matrix[] instanceTransforms;

        int currentpatch = NoPatch;

        public IWeather Weather
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public SpaceDebris(ContentManager content, String filename)
        {
            patch = new DebrisPatch[PatchCount];
            patch[0] = new DebrisPatch();
            for (int i = 1; i < PatchCount; i++)
            {
                patch[i] = new DebrisPatch(patch[0]);
            }

            int count = PatchCount * DebrisPatch.PatchDensity;
            Array.Resize(ref instanceTransforms, count);

            instancedModel = content.Load<InstancedModel>(filename);
        }

        public void Dispose()
        {
            for (int i = 0; i < PatchCount; i++)
            {
                patch[i] = null;
            }
            patch = null;
            instanceTransforms = null;
        }

        bool Contains(Vector3 origin, Vector3 test, bool current)
        {
            float dx = Math.Abs(origin.X - test.X);
            float dy = Math.Abs(origin.Y - test.Y);
            float dz = Math.Abs(origin.Z - test.Z);

            float padding = 1.05f;           // Have to be well inside
            if (current) padding = 1.2f;    // Or well outside

            if (dx < DebrisPatch.PatchScale * padding
                && dy < DebrisPatch.PatchScale * padding
                && dz < DebrisPatch.PatchScale * padding)
            {
                return true;
            }
            return false;
        }

        void ClosestSquares(Vector3 camera)
        {
            // This checks all the squares that are around the current one
            // and makes sure that there are debris drawing in the closest
            // eight squares.

            Vector3 testlocation = patch[currentpatch].Origin;

            // Pre-cache all the existing proximity information
            float[] proximity = new float[PatchCount];
            for (int i = 0; i < PatchCount; i++)
            {
                proximity[i] = Vector3.Distance(camera, patch[i].Origin);
            }

            int test = NoPatch;

            // Start in the positive quadrant and work it back
            testlocation.X += DebrisPatch.PatchScale;
            testlocation.Y += DebrisPatch.PatchScale;
            testlocation.Z += DebrisPatch.PatchScale;

            bool[] haschanged = new bool[PatchCount];
            for (int b = 0; b < PatchCount; b++)
            {
                haschanged[b] = false;
            }

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        test = ReplaceFarthest(camera, testlocation, proximity);
                        if (test != NoPatch)
                        {
                            haschanged[test] = true;
                        }
                        testlocation.Z -= DebrisPatch.PatchScale;
                    }
                    testlocation.Y -= DebrisPatch.PatchScale;
                }
                testlocation.X -= DebrisPatch.PatchScale;
            }

            for (int b = 0; b < PatchCount; b++)
            {
                if (haschanged[b]) Rebuild(b);
            }
        }

        int ReplaceFarthest(Vector3 camera, Vector3 test, float[] proximity)
        {
            for (int i = 0; i < PatchCount; i++)
            {
                if (patch[i].Origin == test) return NoPatch;
            }
            //****************************************************************************
            //
            //TODO:  Instead of testing by center point distance, compare by closest side?
            //Each "farthest" is based on the closest side to the current square, otherwise
            //the corners go away.
            //
            //****************************************************************************
            float testdistance = Vector3.Distance(camera, test);
            int farthest = NoPatch;
            for (int i = 0; i < proximity.Length; i++)
            {
                if (farthest != NoPatch && proximity[farthest] < proximity[i]) farthest = i;
                else if (testdistance < proximity[i]) farthest = i;
            }
            if (farthest != NoPatch)
            {
                patch[farthest].Origin = test;
                proximity[farthest] = testdistance;
            }
            return farthest;
        }
        static Vector3 ResetValue = new Vector3(100000f, 1000000f, 100000f);
        void Reset(Vector3 location)
        {
            currentpatch = 0;
            patch[currentpatch].Origin = location;
            Rebuild(0);
            //ClosestSquares(location);
        }

        void Rebuild(int patchkey)
        {
            int pad = patchkey * PatchCount;

            for (int i = 0; i < DebrisPatch.PatchDensity; i++)
            {
                patch[patchkey].Data[i].BuildTransform(patch[patchkey].Origin, out instanceTransforms[pad + i]);
            }
        }

        void RebuildAll()
        {
            Vector3 rebuildPoint = patch[currentpatch].Origin;

            const int Center = 14;

            currentpatch = Center;

            Vector3 testlocation = rebuildPoint;

            // Start in the positive quadrant and work it back
            testlocation.X += DebrisPatch.PatchScale;
            testlocation.Y += DebrisPatch.PatchScale;
            testlocation.Z += DebrisPatch.PatchScale;

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        patch[x + y + z].Origin = testlocation;
                        Rebuild(x + y + z);
                        testlocation.Z -= DebrisPatch.PatchScale;
                    }
                    testlocation.Y -= DebrisPatch.PatchScale;
                }
                testlocation.X -= DebrisPatch.PatchScale;
            }
        }

        public void Update(Time elapsed)
        {
            //Vector3 cameralocation = SolInvasion.Camera.Position;

            if (currentpatch == NoPatch)
            {
                //Reset(cameralocation);
                Reset(Vector3.Zero);
            }
            /*
            if (!Contains(patch[currentpatch].Origin, cameralocation, true))
            {
                // Reset the current patch, since there is none
                currentpatch = NoPatch;
                for (int i = 0; i < PatchCount; i++)
                {
                    if (Contains(patch[i].Origin, cameralocation, false))
                    {
                        currentpatch = i;
                    }
                }
                if (currentpatch == NoPatch)
                {
                    Reset(cameralocation);
                }

                //RebuildAll();
            }

            if (checkclosest.AutoTrigger(elapsed))
            {
                ClosestSquares(cameralocation);
            }*//*
        }

        public void Draw(GraphicsDevice device, Camera camera)
        {
            if (currentpatch != NoPatch)
            {
                instancedModel.DrawInstances(instanceTransforms, camera.View, camera.Projection);
            }
        }
    }*/
}