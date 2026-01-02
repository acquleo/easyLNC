namespace acquleo.Base.Text
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Globalization;
    using System.Diagnostics.Contracts;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// carattere illegale e replacement
    /// </summary>
    public class IllegalChar
    {
        /// <summary>
        /// carattere illegale
        /// </summary>
        public char Char { get; set; } = ' ';
        /// <summary>
        /// rimpiazzo
        /// </summary>
        public string Replace { get; set; }=string.Empty;
    }

    /// <summary>
    /// utility escape stringa
    /// </summary>
    sealed public class CharEscaper 
    {
        private readonly String[] s_escapeStringPairs;
        private readonly char[] s_escapeChars;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="illegalChars"></param>
        public CharEscaper(List<IllegalChar> illegalChars)
        {
            s_escapeStringPairs= new string[illegalChars.Count * 2];
            s_escapeChars = new char[illegalChars.Count];

            for(int i=0;i<illegalChars.Count;i++)
            {
                var index1 = i * 2;
                var index2 = i * 2 + 1;
                s_escapeStringPairs[index1] = illegalChars[i].Char.ToString();
                s_escapeStringPairs[index2] = illegalChars[i].Replace.ToString();
                s_escapeChars[i] = illegalChars[i].Char;
            }
        }

        /// <summary>
        /// esegue escape di caratteri illegali
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public String Escape( String str )
        {
            if (str == null)
                return null;

            StringBuilder sb = null;

            int strLen = str.Length;
            int index; // Pointer into the string that indicates the location of the current '&' character
            int newIndex = 0; // Pointer into the string that indicates the start index of the "remaining" string (that still needs to be processed).


            do
            {
                index = str.IndexOfAny( s_escapeChars, newIndex );

                if (index == -1)
                {
                    if (sb == null)
                        return str;
                    else
                    {
                        sb.Append( str, newIndex, strLen - newIndex );
                        return sb.ToString();
                    }
                }
                else
                {
                    if (sb == null)
                        sb = new StringBuilder();                    

                    sb.Append( str, newIndex, index - newIndex );
                    sb.Append( GetEscapeSequence( str[index] ) );

                    newIndex = ( index + 1 );
                }
            }
            while (true);
            
            // no normal exit is possible
        }

        private String GetEscapeSequence(char c)
        {
            int iMax = s_escapeStringPairs.Length;
            Contract.Assert(iMax % 2 == 0, "Odd number of strings means the attr/value pairs were not added correctly");

            for (int i = 0; i < iMax; i += 2)
            {
                String strEscSeq = s_escapeStringPairs[i];
                String strEscValue = s_escapeStringPairs[i + 1];

                if (strEscSeq[0] == c)
                    return strEscValue;
            }

            Contract.Assert(false, "Unable to find escape sequence for this character");
            return c.ToString();
        }

        private String GetUnescapeSequence( String str, int index, out int newIndex )
        {
            int maxCompareLength = str.Length - index;

            int iMax = s_escapeStringPairs.Length;
            Contract.Assert( iMax % 2 == 0, "Odd number of strings means the attr/value pairs were not added correctly" );
                          
            for (int i = 0; i < iMax; i += 2)
            {
                String strEscSeq = s_escapeStringPairs[i];
                String strEscValue = s_escapeStringPairs[i+1];
                
                int length = strEscValue.Length;

                if (length <= maxCompareLength && String.Compare( strEscValue, 0, str, index, length, StringComparison.Ordinal) == 0)
                {
                    newIndex = index + strEscValue.Length;
                    return strEscSeq;
                }
            }

            newIndex = index + 1;
            return str[index].ToString();
        }
            
        /// <summary>
        /// esegue unescape di caratteri illegali
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public String Unescape( String str )
        {
            if (str == null)
                return null;

            StringBuilder sb = null;

            int strLen = str.Length;
            int index = 0; // Pointer into the string that indicates the location of the current '&' character
            int newIndex = 0; // Pointer into the string that indicates the start index of the "remainging" string (that still needs to be processed).

            do
            {
                if (sb == null)
                    sb = new StringBuilder();

                sb.Append(GetUnescapeSequence(str, index, out newIndex));
                    index = newIndex;
            }
            while(index<strLen);
            return sb.ToString();
        }

    }                        
}
