using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Units
{
    public class Attributes
    {
        // Simulation
        public float MaximumHealth;
        public float CurrentHealth;
        public float CollisionDamage;

        // Locomotion/Physics
        public float Acceleration;
        public float Deceleration;
        public float TerminalVelocity;
        public float Mass;
        public float TurnSpeed;

        public Attributes Clone()
        {
            Attributes copy = new Attributes();
            copy.Acceleration = this.Acceleration;
            copy.Deceleration = this.Deceleration;
            copy.TerminalVelocity = this.TerminalVelocity;
            copy.Mass = this.Mass;
            copy.TurnSpeed = this.TurnSpeed;
            copy.MaximumHealth = this.MaximumHealth;
            copy.CurrentHealth = this.CurrentHealth;
            copy.CollisionDamage = this.CollisionDamage;
            return copy;
        }
    }
}