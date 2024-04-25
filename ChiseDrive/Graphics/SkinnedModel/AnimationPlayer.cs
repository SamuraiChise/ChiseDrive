#region File Description
//-----------------------------------------------------------------------------
// AnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace ChiseDrive.Graphics.SkinnedModel
{
    public class Animation
    {
        AnimationClip Clip;
        TimeSpan Time;
        int Keyframe;
        float Weight;
        bool Looping;
        bool FirstPlay;

        TimeSpan fadeTime = TimeSpan.Zero;
        float fadeAmmount = 0f;

        public string Name;
        public int Frame
        {
            get
            {
                return (int)(Time.TotalSeconds * 60f);
            }
        }
        public bool IsFading
        {
            get
            {
                if (Clip == null) return false;
                return fadeTime == TimeSpan.Zero ? false : true;
            }
        }

        //Keyframe[] LastTransforms;
        //Keyframe[] NextTransforms;
        Keyframe[] BindTransforms;

        Matrix[] BoneTransforms;
        Matrix[] BindPosition;

        Matrix[] BonePrevious;
        Matrix[] BoneAcceleration;

        public bool[] RagDoll { get; set; }
        public IList<int> SkeletonHierarchy { get; set; }

        public static readonly TimeSpan StandardFade = new TimeSpan(0, 0, 0, 0, 50);//83,50

        public Animation(SkinningData data)
        {
            BoneTransforms = new Matrix[data.BindPose.Count];
            //LastTransforms = new Keyframe[data.BindPose.Count];
            //NextTransforms = new Keyframe[data.BindPose.Count];
            BindTransforms = new Keyframe[data.BindPose.Count];

            BonePrevious = new Matrix[data.BindPose.Count];
            BoneAcceleration = new Matrix[data.BindPose.Count];

            BindPosition = new Matrix[data.BindPose.Count];
            data.BindPose.CopyTo(BindPosition, 0);

            for (int i = 0; i < BindTransforms.Length; i++)
            {
                BindTransforms[i] = new Keyframe(i, TimeSpan.Zero, BindPosition[i]);
            }
        }

        public bool Active
        {
            get
            {
                if (Clip == null) return false;
                if (Time + StandardFade >= Clip.Duration) return false;
                return true;
            }
        }

        bool Done
        {
            get
            {
                if (Clip == null) return true;
                if (Time >= Clip.Duration) return true;
                return false;
            }
        }
        
        public static void Blend(Animation[] input, ref Matrix[] output)
        {
            for (int bone = 0; bone < output.Length; bone++)
            {
                for (int anim = 0; anim < input.Length; anim++)
                {
                    if (!input[anim].Done)
                    {
                        double delta = input[anim].Weight * input[anim].fadeAmmount;
                        
                        if (input[anim].FirstPlay)
                        {
                            if (input[anim].Time < StandardFade)
                            {
                                delta *= input[anim].Time.TotalSeconds / StandardFade.TotalSeconds;
                            }
                        }

                        if (delta == 1) output[bone] = input[anim].BoneTransforms[bone];
                        else
                        {
                            float fade = (float)(delta);

                            Quaternion rotation1 = Quaternion.CreateFromRotationMatrix(output[bone]);
                            Quaternion rotation2 = Quaternion.CreateFromRotationMatrix(input[anim].BoneTransforms[bone]);
                            Quaternion blendR = Quaternion.Lerp(rotation1, rotation2, fade);

                            Vector3 translation1 = output[bone].Translation;
                            Vector3 translation2 = input[anim].BoneTransforms[bone].Translation;
                            Vector3 blendV = Vector3.Lerp(translation1, translation2, fade);

                            output[bone] = Matrix.CreateFromQuaternion(blendR) * Matrix.CreateTranslation(blendV);

                            //output[bone] =
                            //    Matrix.Lerp(input[anim].BoneTransforms[bone],
                            //    output[bone], (float)(1 - delta));
                        }
                    }
                }
            }
        }

        public void SetClip(AnimationClip clip, string name, float weight, bool looping, bool fadeIn)
        {
            if (clip == null)
                throw new ArgumentNullException("clip");

            Clip = clip;
            Name = name;
            Looping = looping;
            FirstPlay = fadeIn;
            Weight = weight;
            Time = TimeSpan.Zero;
            Keyframe = 0;

            // Clear the data
            BindPosition.CopyTo(BoneTransforms, 0);
            //BindTransforms.CopyTo(LastTransforms, 0);
            //BindTransforms.CopyTo(NextTransforms, 0);

            fadeTime = TimeSpan.Zero;
            fadeAmmount = 1f;
        }

        public void FadeOut(TimeSpan fade)
        {
            if (fadeTime == TimeSpan.Zero)
            {
                fadeTime = fade;
                fadeAmmount = 1f;
                Looping = false;
            }
        }

        public void Stop()
        {
            Time = Clip.Duration;
            fadeAmmount = 0f;
        }

        public void Update(TimeSpan time, bool relativeToCurrentTime)
        {
            if (Done) return;

            // Update the animation position.
            if (relativeToCurrentTime)
            {
                time += Time;
            }

            // Check for fadeout
            if (fadeTime != TimeSpan.Zero)
            {
                fadeAmmount -= (float)(time.TotalMilliseconds / fadeTime.TotalMilliseconds);
            }
            if (fadeAmmount <= 0f)
            {
                time = Clip.Duration;
            }

            if (time >= Clip.Duration)//At the end
            {
                if (Looping)//Loop the anim
                {
                    Keyframe = 0;
                    Time = time - Clip.Duration;
                    FirstPlay = false;
                }
                else//Hold the last frame
                {
                    Time = Clip.Duration;
                }
            }
            else//Advance the time
            {
                Time = time;
            }

            // Read keyframe matrices.
            IList<Keyframe> keyframes = Clip.Keyframes;

            while (Keyframe < keyframes.Count)
            {
                Keyframe keyframe = keyframes[Keyframe];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > Time)
                    break;

                if (RagDoll[keyframe.Bone])
                {

                    int parent = SkeletonHierarchy[keyframe.Bone];
                    if (parent < 0) parent = 0;
                    Matrix parentDelta = BonePrevious[parent] - BoneTransforms[parent];

                    BoneTransforms[keyframe.Bone] =
                        BindPosition[keyframe.Bone]
                        + parentDelta;
                }
                else
                {
                    // Use this keyframe.
                    BonePrevious[keyframe.Bone] = BoneTransforms[keyframe.Bone];
                    BoneTransforms[keyframe.Bone] = keyframe.Transform;
                }

                //LastTransforms[keyframe.Bone] = keyframe;
                //if (LastTransforms[keyframe.Bone].Time == NextTransforms[keyframe.Bone].Time)
                //{
                //    NextTransforms[keyframe.Bone] = FindNextKey(keyframe.Bone, Keyframe);
                //}

                Keyframe++;
            }
            /*
            Debug.Metrics.OpenMetric("KeyframeInterpolation");
            
            for (int i = 0; i < BoneTransforms.Length; i++)
            {
                if (NextTransforms[i].Time != LastTransforms[i].Time)
                {
                    float mu = (float)((Time.TotalSeconds - LastTransforms[i].Time.TotalSeconds)
                        / (NextTransforms[i].Time.TotalSeconds - LastTransforms[i].Time.TotalSeconds));

                    BoneTransforms[i] = Matrix.Lerp(LastTransforms[i].Transform, NextTransforms[i].Transform, mu);
                }
            }
            Debug.Metrics.CloseMetric("KeyframeInterpolation");*/
        }

        Keyframe FindNextKey(int bone, int lastkey)
        {
            int nextkey = lastkey + 1;

            while (nextkey < Clip.Keyframes.Count)
            {
                Keyframe newkey = Clip.Keyframes[nextkey];

                if (newkey.Bone == bone) return newkey;

                nextkey++;
            }

            return Clip.Keyframes[lastkey];
        }
    }

    public enum AnimationLayer
    {
        Body = 0,
        Face = 1,
        Count = 2,
    };

    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// </summary>
    public class AnimationPlayer
    {
        public static float BlendWeight
        {
            get
            {
                return blend;
            }
            set
            {
                blend = value;
                blend = blend > 1f ? 1f : blend < 0f ? 0f : blend;
            }
        }
        static float blend = 0.5f;
        #region Fields

        // Current animation transform matrices.
        Matrix[] boneTransforms;
        Matrix[] worldTransforms;
        Matrix[] skinTransforms;

        const int AnimationBlendLevels = 2;
        Animation[] Animations = new Animation[(int)AnimationLayer.Count * AnimationBlendLevels];

        // Backlink to the bind pose and skeleton hierarchy data.
        SkinningData skinningDataValue;


        #endregion

        bool slow = false;
        public bool SetSlow
        {
            get { return slow; }
            set { slow = value; }
        }

        public bool IsFading
        {
            get
            {
                bool fade = false;
                foreach (Animation anim in Animations)
                {
                    if (anim.IsFading) fade = true;
                }
                return fade;
            }
        }

        /// <summary>
        /// Sets the animation speed for the playback.  1=normal, 2=double, 0=none
        /// </summary>
        public float AnimationSpeed
        {
            get;
            set;
        }

        bool[] RagDollBones;

        public void SetRagDoll(string bone)
        {
            if (skinningDataValue.BoneNames.Contains(bone))
            {
                int index = skinningDataValue.BoneNames.IndexOf(bone);
                RagDollBones[index] = true;
            }
        }

        public void ClearAllRagDoll()
        {
            for (int i = 0; i < RagDollBones.Length; i++)
            {
                RagDollBones[i] = false;
            }
        }

        public Animation GetCurrent(AnimationLayer layer)
        {
            int key = (int)layer * AnimationBlendLevels;

            for (int i = key; i < key + AnimationBlendLevels; i++)
            {
                if (Animations[i].Active) return Animations[i];
            }
            return Animations[key];
        }

        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public AnimationPlayer(SkinningData skinningData)
        {
            if (skinningData == null)
                throw new ArgumentNullException("skinningData");

            skinningDataValue = skinningData;

            for (int i = 0; i < Animations.Length; i++)
            {
                Animations[i] = new Animation(skinningData);
            }

            boneTransforms = new Matrix[skinningData.BindPose.Count];
            worldTransforms = new Matrix[skinningData.BindPose.Count];
            skinTransforms = new Matrix[skinningData.BindPose.Count];
            RagDollBones = new bool[skinningData.BindPose.Count];
            ClearAllRagDoll();

            skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
            AnimationSpeed = 1f;
        }

        public void StartClip(string name, AnimationLayer layer, float blendWeight, bool looping, bool fadeIn)
        {
            if (skinningDataValue != null)
            {
                AnimationClip clip = skinningDataValue.AnimationClips[name];

                int key = (int)layer * AnimationBlendLevels;

                Animation swap = Animations[key];
                Animations[key] = Animations[key + 1];
                Animations[key + 1] = swap;
                Animations[key + 1].SetClip(clip, name, blendWeight, looping, fadeIn);
                Animations[key + 1].RagDoll = RagDollBones;
                Animations[key + 1].SkeletonHierarchy = skinningDataValue.SkeletonHierarchy;

                //if (Animations[key].Active) Animations[key].FadeOut(Animation.StandardFade);
                if (Animations[key].Active) Animations[key].Stop();

                //for (int i = key; i < key + AnimationBlendLevels; i++)
                //{
                //    if (!Animations[i].Active)
                //    {
                //        Animations[i].SetClip(clip, name, blendWeight, looping);
                //        break;
                //    }
                //}
            }
        }

        public void EndClip(string name)
        {
            foreach (Animation anim in Animations)
            {
                if (anim.Name == name)
                {
                    anim.FadeOut(Animation.StandardFade);
                }
            }
        }

        public void EndClip()
        {
            foreach (Animation anim in Animations)
            {
                anim.FadeOut(Animation.StandardFade);
            }
        }

        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(TimeSpan time, bool relativeToCurrentTime,
                           Matrix rootTransform)
        {
            TimeSpan updateTime = new TimeSpan((long)((float)time.Ticks * AnimationSpeed));

            UpdateBoneTransforms(updateTime, relativeToCurrentTime);
            UpdateWorldTransforms(rootTransform);
            UpdateSkinTransforms();
        }


        /// <summary>
        /// Helper used by the Update method to refresh the BoneTransforms data.
        /// </summary>
        public void UpdateBoneTransforms(TimeSpan time, bool relativeToCurrentTime)
        {
            TimeSpan newtime;

            if (slow) newtime = new TimeSpan(time.Ticks / 20);
            else newtime = time;

            foreach (Animation anim in Animations)
            {
                anim.Update(newtime, relativeToCurrentTime);
            }
            Animation.Blend(Animations, ref boneTransforms);
        }


        /// <summary>
        /// Helper used by the Update method to refresh the WorldTransforms data.
        /// </summary>
        public void UpdateWorldTransforms(Matrix rootTransform)
        {
            // Root bone.
            worldTransforms[0] = boneTransforms[0] * rootTransform;

            // Child bones.
            for (int bone = 1; bone < worldTransforms.Length; bone++)
            {
                int parentBone = skinningDataValue.SkeletonHierarchy[bone];

                worldTransforms[bone] = boneTransforms[bone] *
                                             worldTransforms[parentBone];
            }
        }


        /// <summary>
        /// Helper used by the Update method to refresh the SkinTransforms data.
        /// </summary>
        public void UpdateSkinTransforms()
        {
            for (int bone = 0; bone < skinTransforms.Length; bone++)
            {
                skinTransforms[bone] = skinningDataValue.InverseBindPose[bone] *
                                            worldTransforms[bone];
            }
        }

        public Matrix GetBoneTransform(string name)
        {
            if (!skinningDataValue.BoneNames.Contains(name)) return Matrix.Identity;
            int index = skinningDataValue.BoneNames.IndexOf(name);
            return worldTransforms[index];
        }

        /// <summary>
        /// Gets the current bone transform matrices, relative to their parent bones.
        /// </summary>
        public Matrix[] GetBoneTransforms()
        {
            return boneTransforms;
        }


        /// <summary>
        /// Gets the current bone transform matrices, in absolute format.
        /// </summary>
        public Matrix[] GetWorldTransforms()
        {
            return worldTransforms;
        }


        /// <summary>
        /// Gets the current bone transform matrices,
        /// relative to the skinning bind pose.
        /// </summary>
        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        }
    }
}
