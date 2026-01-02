using acquleo.DataBroker.MQTT;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TetSistemi.Base.Interfaces;
using acquleo.Protocol.Transport.Data.Endpoint;

namespace acquleo.Protocol.Transport.Mqtt.Tcpip
{
    /// <summary>
    /// implements a mqtt client that connects to a broker and subscribes to topics
    /// </summary>
    public class AutoBrokerMqttDataClient : IMqttDataTransport, IEnabler, IDisposable
    {
        readonly acquleo.DataBroker.MQTT.TetMqttClient client;
        readonly List<TopicInfo> topics;
        readonly MqttServer mqttServer;
        Thread spawnerThread;
        /// <inheritdoc/>
        public event dDataReceivedAsync<MqttMessage> DataReceivedAsync;
        /// <inheritdoc/>
        public event dDataTransportAsync<MqttMessage> DataTransportAvailable;
        /// <inheritdoc/>
        public event dDataTransportAsync<MqttMessage> DataTransportUnavailable;
        /// <inheritdoc/>
        public event dDataTransportTraceAsync<MqttMessage> DataTransportTraceAsync;

        /// <inheritdoc/>
        public bool Available => client.IsConnected;

        bool IsAvailable()
        {
            int port = 1883; //<--- This is your value
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (var tcpi in tcpConnInfoArray)
            {
                if (tcpi.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }

            return isAvailable;
        }
        /// <summary>
        /// creates a new instance of the class
        /// </summary>
        /// <param name="cfg">configuration</param>
        /// <param name="topics">topic subscriptions</param>
        public AutoBrokerMqttDataClient(acquleo.DataBroker.MQTT.TetMqttClientConfig cfg, List<TopicInfo> topics)
        {
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(1883);

            mqttServer = new MqttFactory().CreateMqttServer(optionsBuilder.Build());


            this.client = new acquleo.DataBroker.MQTT.TetMqttClient(cfg);
            this.client.Connected += Client_Connected;
            this.client.Disconnected += Client_Disconnected;
            this.client.MessageReceived += Client_MessageReceived;
            this.topics = topics;

        }

        private async void Client_MessageReceived(acquleo.DataBroker.MQTT.ITetMqttClient client,
            acquleo.DataBroker.MQTT.MessageArgs message)
        {
            MqttMessage msg = new MqttMessage()
            {
                Payload = message.Message,
                Qos = message.QosLevel,
                Responsetopic = message.ResponseTopic,
                Retain = message.Retain,
                Topic = message.Topic,
                ContentType = message.ContentType,

            };

            if (DataReceivedAsync != null)
            {
                await Task.WhenAll(Array.ConvertAll(
                     DataReceivedAsync.GetInvocationList(),
                     e => ((dDataReceivedAsync<MqttMessage>)e)(this, msg, GetRemoteEndpoint()))).ConfigureAwait(false);
            }

        }

        private async void Client_Disconnected(acquleo.DataBroker.MQTT.ITetMqttClient client)
        {
            if (DataTransportUnavailable != null)
            {
                await Task.WhenAll(Array.ConvertAll(
                     DataTransportUnavailable.GetInvocationList(),
                     e => ((dDataTransportAsync<MqttMessage>)e)(this))).ConfigureAwait(false);
            }
        }

        private async void Client_Connected(acquleo.DataBroker.MQTT.ITetMqttClient client)
        {
            await this.client.SubscribeAsync(this.topics).ConfigureAwait(false);

            if (DataTransportAvailable != null)
            {
                await Task.WhenAll(Array.ConvertAll(
                     DataTransportAvailable.GetInvocationList(),
                     e => ((dDataTransportAsync<MqttMessage>)e)(this))).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public Task<bool> SendAsync(MqttMessage data)
        {
            if (!client.IsConnected) return Task.FromResult(false);

            return client.PublishAsync(new acquleo.DataBroker.MQTT.TopicInfo() { Topic = data.Topic, QosLevel = data.Qos },
                 data.Payload, data.Retain, contentType: data.ContentType, responseTopic: data.Responsetopic, userPropertyInfo: data.Userproperties
                 , data.PayloadFormatIndicator);
        }

        /// <inheritdoc/>
        public Task<bool> SendAsync(MqttMessage data, DataEndpoint to)
        {
            return SendAsync(data);
        }

        async void MqttServerSpawner()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if (!mqttServer.IsStarted && IsAvailable())
                            await mqttServer.StartAsync().ConfigureAwait(false);
                    }
                    catch (SocketException)
                    {
                        await mqttServer.StopAsync().ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException)
                    {
                        //do nothing
                    }

                    Thread.Sleep(5000);
                }
            }
            catch (ThreadInterruptedException)
            {
                //do nothing
            }
            catch (ThreadAbortException)
            {
                //do nothing
            }
        }

        bool enabled;

        /// <inheritdoc/>
        public void Enable()
        {
            if (enabled) return;

            enabled = true;
            client.Enable();

            spawnerThread = new Thread(new ThreadStart(MqttServerSpawner));
            spawnerThread.IsBackground = true;
            spawnerThread.Start();

        }
        /// <inheritdoc/>
        public void Disable()
        {
            if (!enabled) return;

            enabled = false;
            spawnerThread.Interrupt();


            client.Disable();

            mqttServer.StopAsync().Wait();

        }

        MqttDataEndpoint endpoint;
        /// <inheritdoc/>
        public DataEndpoint GetRemoteEndpoint()
        {
            if (endpoint == null) endpoint = new MqttDataEndpoint();
            endpoint.brokerip = client.Host;
            endpoint.port = this.client.Port;
            return endpoint;
        }


        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            this.Disable();

            client.Dispose();

            mqttServer.Dispose();
        }

        /// <inheritdoc/>
        public bool IsEnabled()
        {
            return enabled;
        }

        /// <inheritdoc/>
        public async Task<bool> SubscribeAsync(List<MqttTopic> subscribe)
        {
            List<TopicInfo> topicInfos = new List<TopicInfo>();
            foreach (var intopic in subscribe)
            {
                topicInfos.Add(new TopicInfo() { QosLevel = intopic.Qos, Topic = intopic.Topic });
            }

            await client.Subscribe(topicInfos);

            return true;
        }
    }
}