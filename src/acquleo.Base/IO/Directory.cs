// ------------------------------------------------------------------------
//Società: T&TSistemi s.r.l.
//Anno: 2008 
//Progetto: AMIL5
//Autore: Acquisti Leonardo
//Nome modulo software: TetSistemi.Commons.dll
//Data ultima modifica: $LastChangedDate: 2015-12-14 15:47:15 +0100 (Mon, 14 Dec 2015) $
//Versione: $Rev: 425 $
// ------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace acquleo.Base.IO
{
    /// <summary>
    /// directory helper
    /// </summary>
    public class Directory
    {

        #region Static Members

        /// <summary>
        /// set the current working directory
        /// </summary>
        /// <param name="path">directory</param>
        public static void SetCurrentDirectory(string path)
        {
            System.IO.Directory.SetCurrentDirectory(path);
        }

        /// <summary>
        /// set the current working directory as the executing assembly directory
        /// </summary>
        public static void SetCurrentDirectory()
        {
            System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }

        /// <summary>
        /// returns the current working directory
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDirectory()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
        /// <summary>
        /// returns the current temp directory
        /// </summary>
        /// <returns></returns>
        public static string GetTempDirectory()
        {
            return System.IO.Path.GetTempPath();
        }
        #endregion

    }
}
