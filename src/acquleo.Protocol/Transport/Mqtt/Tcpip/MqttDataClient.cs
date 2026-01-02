using acquleo.DataBroker.MQTT;
using MQTTnet.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TetSistemi.Base.Interfaces;
using TetSistemi.Base.Logging;
using acquleo.Protocol.Transport.Data.Endpoint;

namespace acquleo.Protocol.Transport.Mqtt.Tcpip
{
    /// <summary>
    /// mqtt data client
    /// </summary>
    public class MqttDataClient : IMqttDataTransport, IEnabler, IDisposable
    {
        readonly IApplicationLog logger;
        readonly acquleo.DataBroker.MQTT.TetMqttClient client;

        readonly List<TopicInfo> topics;

        ///<inheritdoc/>
        public event dDataReceivedAsync<MqttMessage> DataReceivedAsync;
        ///<inheritdoc/>
        public event dDataTransportAsync<MqttMessage> DataTransportAvailable;
        ///<inheritdoc/>
        public event dDataTransportAsync<MqttMessage> DataTransportUnavailable;
        ///<inheritdoc/>
        public event dDataTransportTraceAsync<MqttMessage> DataTransportTraceAsync;

        ///<inheritdoc/>
        public bool Available { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="topics"></param>
        public MqttDataClient(acquleo.DataBroker.MQTT.TetMqttClientConfig cfg, List<TopicInfo> topics)
        {
            this.logger = SingletonFactoryProvider.Provider.GetApplicationLogFactory().GetBuilder().WithObject(this).Build();
            this.client = new acquleo.DataBroker.MQTT.TetMqttClient(cfg);
            this.client.Connected += Client_Connected;
            this.client.Disconnected += Client_Disconnected;
            this.client.MessageReceived += Client_MessageReceived;
            this.topics = topics;
        }


        private async void Client_MessageReceived(acquleo.DataBroker.MQTT.ITetMqttClient client,
            acquleo.DataBroker.MQTT.MessageArgs message)
        {
            try
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
            catch (Exception ex)
            {
                this.logger.ErrorIf(this, () => $@"Exception", () => ex);
            }
        }

        private async void Client_Disconnected(acquleo.DataBroker.MQTT.ITetMqttClient client)
        {
            try
            {
                Available = false;

                if (DataTransportUnavailable != null)
                {
                    await Task.WhenAll(Array.ConvertAll(
                         DataTransportUnavailable.GetInvocationList(),
                         e => ((dDataTransportAsync<MqttMessage>)e)(this))).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                this.logger.ErrorIf(this, () => $@"Exception", () => ex);
            }
        }

        private async void Client_Connected(acquleo.DataBroker.MQTT.ITetMqttClient client)
        {
            try
            {
                try
                {
                    await this.client.SubscribeAsync(this.topics).ConfigureAwait(false);
                }
                catch (MqttCommunicationException ex)
                {
                    this.logger.ErrorIf(this, () => $@"MqttCommunicationException", () => ex);
                    return;
                }

                Available = true;

                if (DataTransportAvailable != null)
                {
                    await Task.WhenAll(Array.ConvertAll(
                         DataTransportAvailable.GetInvocationList(),
                         e => ((dDataTransportAsync<MqttMessage>)e)(this))).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                this.logger.ErrorIf(this, () => $@"Exception", () => ex);
            }
        }
        ///<inheritdoc/>
        public Task<bool> SendAsync(MqttMessage data)
        {
            try
            {
                if (!client.IsConnected) return Task.FromResult(false);

                return client.PublishAsync(new acquleo.DataBroker.MQTT.TopicInfo() { Topic = data.Topic, QosLevel = data.Qos },
                     data.Payload, data.Retain, contentType: data.ContentType, responseTopic: data.Responsetopic, userPropertyInfo: data.Userproperties
                     , data.PayloadFormatIndicator);
            }
            catch (Exception ex)
            {
                this.logger.ErrorIf(this, () => $@"Exception", () => ex);
                return Task.FromResult(false);
            }
        }
        ///<inheritdoc/>
        public Task<bool> SendAsync(MqttMessage data, DataEndpoint to)
        {
            return SendAsync(data);
        }

        bool enabled;
        ///<inheritdoc/>
        public void Enable()
        {
            if (enabled) return; 
            enabled = true;
            client.Enable();
        }
        ///<inheritdoc/>
        public void Disable()
        {
            if (!enabled) return; 
            enabled = false;
            client.Disable();
        }

        MqttDataEndpoint endpoint;
        ///<inheritdoc/>
        public DataEndpoint GetRemoteEndpoint()
        {
            if (endpoint == null) endpoint = new MqttDataEndpoint();
            endpoint.brokerip = client.Host;
            endpoint.port = this.client.Port;
            return endpoint;
        }

        /// <summary>
        /// 
        /// </summary>
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
            if (disposing)
            {
                client.Dispose();
            }            
        }
        ///<inheritdoc/>
        public bool IsEnabled()
        {
            return enabled;
        }

        ///<inheritdoc/>
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