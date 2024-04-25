using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Units
{
    interface IDestructible
    {
        /// <summary>
        /// Represented as a percentage
        /// </summary>
        float Health { get; }

        /// <summary>
        /// This guy's on the way out
        /// </summary>
        bool IsDying { get; }

        /// <summary>
        /// Take damage from a source
        /// </summary>
        /// <param name="value"></param>
        /// <param name="source"></param>
        void TakeDamage(float value, ID source);
    }
}
