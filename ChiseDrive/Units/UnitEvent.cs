using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ChiseDrive.Units;

namespace ChiseDrive
{
    public enum EventType
    {
        Alive,// This Event plays constantly (to give units dedicated effects)

        TakeDamage,
        TakeHealing,

        Collision,

        StartDeath,
        ContinueDeath,
        FinishDeath,

        ShotFired,
        WeaponHit,

        Count
    };

    public class UnitEvent
    {
        EventType type;
        ID instigator;
        ID recipient;
        Vector3 location;
        float value;
        static UnitEvent instance = new UnitEvent();

        private UnitEvent() 
        {
            instigator = ID.None;
            recipient = ID.None;
            value = 0f;
        }
        public static ChiseDriveGame Game { get; set; }
        public static UnitEvent Announce(EventType type, ID instigator, ID recipient)
        {
            instance.type = type;
            instance.instigator = instigator;
            instance.recipient = recipient;
            instance.value = 0f;
            instance.location = Vector3.Zero;
            Game.AnnounceEvent(instance);
            return instance;
        }
        public static UnitEvent Announce(EventType type, ID instigator, ID recipient, float value)
        {
            instance.type = type;
            instance.instigator = instigator;
            instance.recipient = recipient;
            instance.value = value;
            instance.location = Vector3.Zero;
            Game.AnnounceEvent(instance);
            return instance;
        }
        public static UnitEvent Announce(EventType type, ID instigator, ID recipient, float value, Vector3 location)
        {
            instance.type = type;
            instance.instigator = instigator;
            instance.recipient = recipient;
            instance.value = value;
            instance.location = location;
            Game.AnnounceEvent(instance);
            return instance;
        }

        public ChiseDrive.Units.ID Instigator 
        {
            get
            {
                return instance.instigator;
            }
            set
            {
                instance.instigator = value;
            }
        }
        public ChiseDrive.Units.ID Recipient
        {
            get
            {
                return instance.recipient;
            }
            set
            {
                instance.recipient = value;
            }
        }
        public float Value
        {
            get
            {
                return instance.value;
            }
            set
            {
                instance.value = value;
            }
        }
        public EventType Type
        {
            get
            {
                return instance.type;
            }
            set
            {
                instance.type = value;
            }
        }
        public Vector3 Location
        {
            get
            {
                return instance.location;
            }
            set
            {
                instance.location = value;
            }
        }

        public static implicit operator int(UnitEvent e) 
        {
            return (int)e.Type;
        }
    }
}