using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Interfaces
{
    /// <summary>
    /// generic enabler interface
    /// </summary>
    public interface IEnabler
    {
        /// <summary>
        /// enable the object
        /// </summary>
        void Enable();
        /// <summary>
        /// disable the object
        /// </summary>
        void Disable();
        /// <summary>
        /// verify if the object is enabled
        /// </summary>
        /// <returns>enabled</returns>
        bool IsEnabled();
    }
}
