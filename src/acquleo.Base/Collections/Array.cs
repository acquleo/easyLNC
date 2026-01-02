// ------------------------------------------------------------------------
//Società: T&TSistemi s.r.l.
//Anno: 2010
//Progetto: AMIL5
//Autore: Acquisti Leonardo
//Nome modulo software: TetSistemi.Commons.dll
//Data ultima modifica: $LastChangedDate: 2015-12-14 15:47:15 +0100 (lun, 14 dic 2015) $
//Versione: $Rev: 425 $
// ------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Collections;

using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace acquleo.Base.Collections
{
    /// <summary>
    /// Consente la gestione di array e liste generiche
    /// </summary>
    public class Array
    {          
        #region Public Methods

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitSet">Indice del bit</param>
        /// <param name="value">Valore del bit</param>
        public static void BitSet(ref byte data, int bitSet, bool value)
        {
            byte[] byteArray = new byte[1]{data};

            BitSet(ref byteArray, bitSet, value);

            data = byteArray[0];

        }

        /// <summary>
        /// Get Bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitGet">Indice del bit</param>
        /// <returns>Valore del bit</returns>
        public static bool BitGet(byte data, int bitGet)
        {
            byte[] byteArray = new byte[1] { data };

            return BitGet(byteArray, bitGet);

        }

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitSet">Indice del bit</param>
        /// <param name="value">Valore del bit</param>
        public static void BitSet(ref char data, int bitSet, bool value)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            BitSet(ref byteArray, bitSet, value);

            data = BitConverter.ToChar(byteArray, 0);

        }

        /// <summary>
        /// Get Bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitGet">Indice del bit</param>
        /// <returns>Valore del bit</returns>
        public static bool BitGet(char data, int bitGet)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            return BitGet(byteArray, bitGet);

        }

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitSet">Indice del bit</param>
        /// <param name="value">Valore del bit</param>
        public static void BitSet(ref double data, int bitSet, bool value)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            BitSet(ref byteArray, bitSet, value);

            data = BitConverter.ToDouble(byteArray, 0);

        }

        /// <summary>
        /// Get Bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitGet">Indice del bit</param>
        /// <returns>Valore del bit</returns>
        public static bool BitGet(double data, int bitGet)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            return BitGet(byteArray, bitGet);

        }

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitSet">Indice del bit</param>
        /// <param name="value">Valore del bit</param>
        public static void BitSet(ref float data, int bitSet, bool value)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            BitSet(ref byteArray, bitSet, value);

            data = BitConverter.ToSingle(byteArray, 0);

        }

        /// <summary>
        /// Get Bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitGet">Indice del bit</param>
        /// <returns>Valore del bit</returns>
        public static bool BitGet(float data, int bitGet)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            return BitGet(byteArray, bitGet);

        }

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitSet">Indice del bit</param>
        /// <param name="value">Valore del bit</param>
        public static void BitSet(ref int data, int bitSet, bool value)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            BitSet(ref byteArray, bitSet, value);

            data = BitConverter.ToInt32(byteArray, 0);

        }

        /// <summary>
        /// Get Bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitGet">Indice del bit</param>
        /// <returns>Valore del bit</returns>
        public static bool BitGet(int data, int bitGet)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            return BitGet(byteArray, bitGet);

        }

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitSet">Indice del bit</param>
        /// <param name="value">Valore del bit</param>
        public static void BitSet(ref long data, int bitSet, bool value)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            BitSet(ref byteArray, bitSet, value);

            data = BitConverter.ToInt64(byteArray, 0);

        }

        /// <summary>
        /// Get Bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitGet">Indice del bit</param>
        /// <returns>Valore del bit</returns>
        public static bool BitGet(long data, int bitGet)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            return BitGet(byteArray, bitGet);

        }

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitSet">Indice del bit</param>
        /// <param name="value">Valore del bit</param>
        public static void BitSet(ref short data, int bitSet, bool value)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            BitSet(ref byteArray, bitSet, value);

            data = BitConverter.ToInt16(byteArray, 0);

        }

        /// <summary>
        /// Get Bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitGet">Indice del bit</param>
        /// <returns>Valore del bit</returns>
        public static bool BitGet(short data, int bitGet)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            return BitGet(byteArray, bitGet);

        }

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitSet">Indice del bit</param>
        /// <param name="value">Valore del bit</param>
        public static void BitSet(ref uint data, int bitSet, bool value)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            BitSet(ref byteArray, bitSet, value);

            data = BitConverter.ToUInt32(byteArray, 0);

        }

        /// <summary>
        /// Get Bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitGet">Indice del bit</param>
        /// <returns>Valore del bit</returns>
        public static bool BitGet(uint data, int bitGet)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            return BitGet(byteArray, bitGet);

        }

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitSet">Indice del bit</param>
        /// <param name="value">Valore del bit</param>
        public static void BitSet(ref ulong data, int bitSet, bool value)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            BitSet(ref byteArray, bitSet, value);

            data = BitConverter.ToUInt64(byteArray, 0);

        }

        /// <summary>
        /// Get Bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitGet">Indice del bit</param>
        /// <returns>Valore del bit</returns>
        public static bool BitGet(ulong data, int bitGet)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            return BitGet(byteArray, bitGet);

        }

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitSet">Indice del bit</param>
        /// <param name="value">Valore del bit</param>
        public static void BitSet(ref ushort data, int bitSet, bool value)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            BitSet(ref byteArray, bitSet, value);

            data = BitConverter.ToUInt16(byteArray, 0);

        }

        /// <summary>
        /// Get Bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitGet">Indice del bit</param>
        /// <returns>Valore del bit</returns>
        public static bool BitGet(ushort data, int bitGet)
        {
            byte[] byteArray = BitConverter.GetBytes(data);

            return BitGet(byteArray, bitGet);

        }

        /// <summary>
        /// Set bit
        /// </summary>
        /// <param name="data">Dati</param>
        /// <param name="bitSet">Indice del bit</param>
        /// <param name="value">Valore del bit</param>
        public static void BitSet(ref byte[] data,int bitSet,bool value)
        {
            BitArray f = new BitArray(data);

            f.Set(bitSet, value);
            
            f.CopyTo(data, 0);

        }

        /// <summary>
        /// Converte un array di  byte in un arrau di bit.
        /// </summary>
        /// <param name="data">Array di byte.</param>
        /// <param name="bitGet ">Bit get.</param>
        /// <returns>Ritorna lo lo stato della conversione.</returns>
        public static bool BitGet(byte[] data, int bitGet)
        {
            BitArray f = new BitArray(data);
            
            return f.Get(bitGet);

        }

        /// <summary>
        /// Converte un BitArray in un array di byte.
        /// </summary>
        /// <param name="bits">Array di bit.</param>
        /// <returns>Ritorna l'array di byte.</returns>
        public static byte[] ToByteArray(BitArray bits)
        {
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << (bitIndex));

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }

        /// <summary>
        /// Compara due liste di tipo T
        /// </summary>
        /// <param name="dataA">Lista A</param>
        /// <param name="dataB">Lista B</param>
        /// <returns>Ritorna lo stato della comparazione.</returns>
        public static bool Equals<T>(List<T> dataA,List<T> dataB)
        {
            //Caso lunghezza diversa
            if(dataA.Count!=dataB.Count) return false;

            for (int i = 0; i < dataA.Count; i++)
            {
                //Caso uno dei due valori null
                if ((dataA[i] == null) && (dataB[i] != null) ||
                    (dataA[i] != null) && (dataB[i] == null)) return false;

                //Controllo solo se i due valori sono != da null
                if((dataA[i]!=null)&&(dataB[i]!=null))
                    if (!dataA[i].Equals(dataB[i]))
                        return false;   

            }
            return true;

        }

        /// <summary>
        /// Compara due array di tipo T
        /// </summary>
        /// <param name="dataA">Array A</param>
        /// <param name="dataB">Array B</param>
        /// <returns>Ritorna lo stato della comparazione.</returns>
        public static bool Equals<T>(T[] dataA, T[] dataB)
        {
            //Caso lunghezza diversa
            if (dataA.Length != dataB.Length) return false;

            for (int i = 0; i < dataA.Length; i++)
            {
                //Caso uno dei due valori null
                if ((dataA[i] == null) && (dataB[i] != null) ||
                    (dataA[i] != null) && (dataB[i] == null)) return false;

                //Controllo solo se i due valori sono != da null
                if ((dataA[i] != null) && (dataB[i] != null))
                    if (!dataA[i].Equals(dataB[i]))
                        return false;

            }
            return true;

        }

        #endregion
    }
}
