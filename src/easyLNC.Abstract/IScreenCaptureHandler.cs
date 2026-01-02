using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface IScreenCaptureHandler
    {
        /// <summary>
        /// Occurs when a screen capture session begins.
        /// </summary>
        event Action<IScreenCaptureHandler, IScreenCapture> OnBegin;
        /// <summary>
        /// Occurs when a screen capture session ends.
        /// </summary>
        event Action<IScreenCaptureHandler, IScreenCapture> OnEnd;
        /// <summary>
        /// raised when a screen capture has started
        /// </summary>
        event Action<IScreenCaptureHandler, IScreenCapture> OnStarted;
        /// <summary>
        /// Occurs when a screen capture operation has stopped, providing access to the handler and the associated
        /// screen capture instance.
        /// </summary>
        event Action<IScreenCaptureHandler, IScreenCapture> OnStopped;

        /// <summary>
        /// begins capturing the specified screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        bool Begin(ScreenInfo screen,[NotNullWhen(true)] out IScreenCapture screenCapture);
        /// <summary>
        /// ends capturing the specified screen capture
        /// </summary>
        /// <param name="screenCapture"></param>
        void End(IScreenCapture screenCapture);

        /// <summary>
        /// begins capturing the specified screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        IEnumerable<IScreenCapture> Get();

    }
}
