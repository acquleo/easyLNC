using System;
using System.Collections.Generic;
using acquleo.Protocol.Converter.BYTE;

namespace acquleo.Protocol.Aggregator.Byte
{
    /// <summary>
    /// 
    /// </summary>
    internal class ByteArrayAggregatorEndPoint
    {
        readonly IByteArrayMessageEnveloper serializer;
        readonly object syncDataReceived = new object();
        private byte[] leaningData = new byte[0];
        private bool leaning = false;
        readonly Queue<byte[]> aggregatedmessagequeue = new Queue<byte[]>();

        public ByteArrayAggregatorEndPoint(IByteArrayMessageEnveloper serializer)
        {
            this.serializer = serializer;
        }
        ///<inheritdoc/>
        public void Aggregate(byte[] data)
        {
            DataReceived(data);
        }
        ///<inheritdoc/>
        public bool AggregatedDataAvailable()
        {
            lock (aggregatedmessagequeue)
            {
                return aggregatedmessagequeue.Count > 0;
            }
        }

        ///<inheritdoc/>
        public void Clear()
        {
            lock (syncDataReceived)
            {
                leaning = false;
                leaningData = new byte[0];

                lock (aggregatedmessagequeue)
                {
                    aggregatedmessagequeue.Clear();
                }
            }
        }

        public byte[] GetAggregatedData()
        {
            lock (aggregatedmessagequeue)
            {
                return aggregatedmessagequeue.Dequeue();
            }
        }

        private void DataReceived(byte[] byteRecv)
        {
            lock (syncDataReceived)
            {
                try
                {

                    byte[] message;
                    uint dataLength = 0;
                    bool repeat = false;
                    //  Uso una struttura ausiliaria per lavorare sui dati:
                    byte[] data;
                    if (leaning)
                    {// Ho dati pendenti:
                     // Concateno i dati pendenti con quelli attualmente ricevuti:
                        data = new byte[leaningData.Length + byteRecv.Length];
                        Array.Copy(leaningData, 0, data, 0, leaningData.Length);
                        Array.Copy(byteRecv, 0, data, leaningData.Length, byteRecv.Length);
                        leaning = false;
                    }
                    else
                    {// Non ho dati pendenti:
                        data = byteRecv;
                    }
                    do
                    {
                        if (!serializer.CanReadLength(data))
                        {
                            // Ho meno dati di quelli necessari a ricavare la lunghezza del messaggio:
                            // Aggiorno l'array di dati pendenti:
                            leaningData = new byte[data.Length];
                            Array.Copy(data, 0, leaningData, 0, leaningData.Length);
                            // Notifico che ci sono dati pendenti:
                            leaning = true;
                            // Non devo ripetere:
                            repeat = false;
                        }
                        else
                        {
                            // Estraggo la dimensione del messaggio dai dati:
                            dataLength = serializer.GetLength(data);
                            // Controllo quanti dati ho:
                            if (data.Length == dataLength)
                            {// Ho ricevuto tutti i dati dichiarati:
                             // Non devo ripetere
                                repeat = false;
                                try
                                {
                                    // Decido quale comando eseguire:
                                    SelectCommand(data);
                                }
                                catch (Exception)
                                {
                                    //nothing to do
                                }
                            }
                            if (data.Length < dataLength)
                            {// Ho meno dati di quelli dichiarati:
                             // Aggiorno l'array di dati pendenti:
                                leaningData = new byte[data.Length];
                                Array.Copy(data, 0, leaningData, 0, leaningData.Length);
                                // Notifico che ci sono dati pendenti:
                                leaning = true;
                                // Non devo ripetere:
                                repeat = false;
                            }
                            if (data.Length > dataLength)
                            {// Ho più dati di quelli dichiarati:
                             // Estraggo il messaggio dai dati ricevuti:
                                message = new byte[dataLength];
                                Array.Copy(data, 0, message, 0, dataLength);
                                // Devo ripetere la procedura:
                                repeat = true;
                                try
                                {
                                    // Decido quale comando eseguire:
                                    SelectCommand(message);
                                }
                                catch (Exception)
                                {
                                    //nothing to do
                                }

                                // Preparo "data" per lavorare sul resto dei dati ricevuti:
                                byte[] remainder = new byte[data.Length - message.Length];
                                Array.Copy(data, message.Length, remainder, 0, remainder.Length);
                                data = remainder;

                            }
                        }
                    } while (repeat);
                }
                catch (Exception)
                {
                    //nothing to do
                }
            }

            void SelectCommand(byte[] bytes)
            {
                lock (aggregatedmessagequeue)
                {
                    aggregatedmessagequeue.Enqueue(bytes);
                }
            }
        }
    }
}
