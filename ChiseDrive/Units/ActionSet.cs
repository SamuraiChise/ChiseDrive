using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ChiseDrive.Graphics.SkinnedModel;

namespace ChiseDrive.Units
{
    public class ActionSet
    {
        static List<Action> ScratchList = new List<Action>(10);
        public static bool DebugActions { get; set; }

        public class Action
        {
            public string Tag;
            public float Weight;
            public string MainAnimation;
            public string HoldAnimation;
            public string RecoverAnimation;
            public string ChainAnimationTag1;
            public string ChainAnimationTag2;
            public string ChainAnimationTag3;
            public string ChainAnimationTag4;
            public Vector3 Motion;
            public float DamageModifier;

            public Action() { }
        }

        public bool IsIdle
        {
            get
            {
                if (PassiveAction == null) return true;
                if (lastAnimation == PassiveAction.MainAnimation) return true;
                return false;
            }
        }

        public string ActionName
        {
            get
            {
                return CurrentAction == null ? "None" : CurrentAction.Tag;
            }
        }

        public string LastPlayed
        {
            get
            {
                return lastAnimation == null ? "None" : lastAnimation;
            }
        }

        public float DamageModifier
        {
            get
            {
                if (CurrentAction != null
                    && CurrentAction.MainAnimation == lastAnimation)
                {
                    return CurrentAction.DamageModifier;
                }

                return 0f;
            }
        }

        bool animationStarted = false;
        public bool AnimationStarted
        {
            get
            {
                return animationStarted;
            }
        }
        public void ClearStarted()
        {
            animationStarted = false;
        }

        public List<Action> Actions = new List<Action>();
        AnimationPlayer AnimationPlayer;

        Action CurrentAction = null;
        Action PassiveAction = null;
        string lastAnimation = null;
        string pending = null;

        static int lastID = 0;
        int id = 0;

        Cooldown pendingTimer = new Cooldown(30f);

        public ActionSet(AnimationPlayer player)
        {
            AnimationPlayer = player;
        }

        public ActionSet()
        {
            AnimationPlayer = null;
            animationStarted = false;

            id = lastID++;
        }

        public ActionSet Clone()
        {
            ActionSet clone = new ActionSet();
            clone.CurrentAction = null;
            clone.PassiveAction = null;
            clone.lastAnimation = null;
            clone.pending = null;
            clone.Actions = this.Actions;
            
            return clone;
        }

        public void SetAnimationPlayer(AnimationPlayer player)
        {
            AnimationPlayer = player;
        }

        void StartAnimation(string animation, bool fadeIn)
        {
            AnimationPlayer.StartClip(animation, AnimationLayer.Body, 1f, false, fadeIn);
            lastAnimation = animation;
            animationStarted = true;
        }

        public void StopAnimation()
        {
            AnimationPlayer.EndClip();
            lastAnimation = null;
            animationStarted = false;
        }

        public Action FindAction(string tag)
        {
            if (tag == "") return null;

            ScratchList.Clear();
            float totalweight = 0;
            
            foreach (Action action in Actions)
            {
                if (action.Tag == tag)
                {
                    ScratchList.Add(action);
                    totalweight += action.Weight;
                }
            }
            if (ScratchList.Count == 0)
                throw new ArgumentException("No action named ", tag);

            float weight = Helper.Randomf * totalweight;

            foreach (Action action in ScratchList)
            {
                weight -= action.Weight;

                if (weight <= 0f)
                    return action;
            }

            throw new Exception("Failed to find an Animation");
        }

        public void ForceAction(string tag)
        {
            CurrentAction = FindAction(tag);
            StartAnimation(CurrentAction.MainAnimation, false);
        }

        public void StartAction(string tag)
        {
            CurrentAction = FindAction(tag);
            StartAnimation(CurrentAction.MainAnimation, true);
        }

        public void StartAction(string tag, bool fadeIn)
        {
            CurrentAction = FindAction(tag);
            StartAnimation(CurrentAction.MainAnimation, fadeIn);
        }

        public void StopAction()
        {
            CurrentAction = null;
            StopAnimation();
        }

        public void QueueAction(string tag)
        {
            pending = tag == "" ? null : tag;
            pendingTimer.Trigger();
        }

        public void QueueChain(int index)
        {
            Action toTry = CurrentAction != null ? CurrentAction : PassiveAction != null ? PassiveAction : null;

            if (toTry != null)
            {
                string next = null;

                switch (index)
                {
                    case 1: next = toTry.ChainAnimationTag1; break;
                    case 2: next = toTry.ChainAnimationTag2; break;
                    case 3: next = toTry.ChainAnimationTag3; break;
                    case 4: next = toTry.ChainAnimationTag4; break;
                }

                QueueAction(next);
            }
        }

        public void SetPassiveAction(string tag)
        {
            PassiveAction = FindAction(tag);
        }

        public Action GetCurrentAction()
        {
            return CurrentAction != null ? CurrentAction : PassiveAction != null ? PassiveAction : null;
        }

        bool IsPassive(string animation)
        {
            return PassiveAction != null && animation == PassiveAction.MainAnimation;
        }

        void UpdateAction()
        {
            if (AnimationPlayer == null) throw new Exception("Please call SetAnimationPlayer before updating. (Did you use the SkinnedModelProcessor to build the asset?)");

            Animation currentAnimation = AnimationPlayer.GetCurrent(AnimationLayer.Body);
            if (DebugActions) ChiseDrive.Debug.DebugText.Write("Currently Playing [" + currentAnimation == null ? "none" : currentAnimation.Name + "]");

            // First check for pending animations which are valid when:
            // 1) An action has been attempted (pending != null) and either:
            //   A) Starting a new action chain (!currentAnimation.Active) or
            //   B) Continuing an action chain (currentAnimation.Name == CurrentAction.HoldAnimation)
            //   C) Current action is idle (CurrentAction == null)
            if (pending != null)
            {
                if (CurrentAction == null
                    || !currentAnimation.Active 
                    || IsPassive(currentAnimation.Name)
                    || currentAnimation.Name == CurrentAction.HoldAnimation)
                {
                    CurrentAction = FindAction(pending);

                    if (CurrentAction == null || CurrentAction.MainAnimation == null)
                    {
                        throw new Exception("Trying to find an action that doesn't exist: " + pending);
                    }

                    pending = null;

                    StartAnimation(CurrentAction.MainAnimation, true);
                    return;
                }
            }

            // Otherwise update our current animation (so long as we're not idle)
            if (!currentAnimation.Active && CurrentAction != null)
            {
                if (lastAnimation == CurrentAction.MainAnimation) // Stage1
                {
                    if (CurrentAction.HoldAnimation != "")
                    {
                        StartAnimation(CurrentAction.HoldAnimation, true);
                    }
                    else if (CurrentAction.RecoverAnimation != "")
                    {
                        StartAnimation(CurrentAction.RecoverAnimation, true);
                    }
                    else
                    {
                        lastAnimation = null;
                        CurrentAction = null;
                    }
                    return;
                }
                if (lastAnimation == CurrentAction.HoldAnimation)// Stage2
                {
                    if (CurrentAction.RecoverAnimation != "")
                    {
                        StartAnimation(CurrentAction.RecoverAnimation, true);
                    }
                    else
                    {
                        lastAnimation = null;
                        CurrentAction = null;
                    }
                    return;
                }
                else if (lastAnimation == CurrentAction.RecoverAnimation)//Stage3
                {
                    CurrentAction = null;
                    lastAnimation = null;
                }
            }

            // Finally, update passive actions
            if (PassiveAction != null && CurrentAction == null)
            {
                if (!currentAnimation.Active || currentAnimation.Name != PassiveAction.MainAnimation)
                {
                    StartAnimation(PassiveAction.MainAnimation, true);
                }
            }
        }

        public void Update(Time elapsed)
        {
            animationStarted = false;

            pendingTimer.Update(elapsed);
            if (pendingTimer.IsReady) pending = null;

            UpdateAction();
        }
    }
}
