using acquleo.DataBroker.MQTT;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TetSistemi.Base.Interfaces;
using acquleo.Protocol.Transport.Data.Endpoint;

namespace acquleo.Protocol.Transport.Mqtt.Memory
{
    /// <summary>
    /// 
    /// </summary>
    public class InMemoryMqttBrokerClient : IMqttDataTransport, IEnabler, IDisposable
    {
        readonly List<TopicInfo> topics;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="topics"></param>
        public InMemoryMqttBrokerClient(List<TopicInfo> topics)
        {
            this.topics = topics;
        }

        /// <inheritdoc/>
        public bool Available => true;

        /// <inheritdoc/>
        public event dDataReceivedAsync<MqttMessage> DataReceivedAsync;
        /// <inheritdoc/>
        public event dDataTransportAsync<MqttMessage> DataTransportAvailable;
        /// <inheritdoc/>
        public event dDataTransportAsync<MqttMessage> DataTransportUnavailable;
        /// <inheritdoc/>
        public event dDataTransportTraceAsync<MqttMessage> DataTransportTraceAsync;
        /// <inheritdoc/>
        public DataEndpoint GetRemoteEndpoint()
        {
            return new EmptyDataEndpoint();
        }
        /// <inheritdoc/>
        public Task<bool> SendAsync(MqttMessage data)
        {
            return SendAsync(data, new EmptyDataEndpoint());
        }
        /// <inheritdoc/>
        public Task<bool> SendAsync(MqttMessage data, DataEndpoint to)
        {
            InMemoryMqttBrokerMessage retmsg = new InMemoryMqttBrokerMessage()
            {
                ContentType = data.ContentType,
                message = data.Payload,
                ResponseTopic = data.Responsetopic,
                Retain = data.Retain,
                topic = new TopicInfo()
                {
                    QosLevel = data.Qos,
                    Topic = data.Topic
                },
                UserProperties = data.Userproperties
            };


            InMemoryMqttBroker.Instance.Publish(retmsg);

            return Task.FromResult(true);
        }
        /// <inheritdoc/>
        public void Enable()
        {
            InMemoryMqttBroker.Instance.Subscribe(this, topics);
        }
        /// <inheritdoc/>
        public void Disable()
        {
            //do nothing
        }
        /// <inheritdoc/>
        public bool IsEnabled()
        {
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public async Task PushMessage(InMemoryMqttBrokerMessage msg)
        {
            MqttMessage retmsg = new MqttMessage()
            {
                ContentType = msg.ContentType,
                Payload = msg.message,
                Qos = msg.topic.QosLevel,
                Responsetopic = msg.ResponseTopic,
                Retain = msg.Retain,
                Topic = msg.topic.Topic,
                Userproperties = msg.UserProperties
            };

            await Task.WhenAll(Array.ConvertAll(
                    DataReceivedAsync.GetInvocationList(),
                    e => ((dDataReceivedAsync<MqttMessage>)e)(this, retmsg, new EmptyDataEndpoint()))).ConfigureAwait(false);
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
            // Cleanup
        }

        /// <inheritdoc/>
        public Task<bool> SubscribeAsync(List<MqttTopic> subscribe)
        {
            List<TopicInfo> topicInfos = new List<TopicInfo>();
            foreach (var intopic in subscribe)
            {
                topicInfos.Add(new TopicInfo() { QosLevel = intopic.Qos, Topic = intopic.Topic });
            }

            InMemoryMqttBroker.Instance.Subscribe(this, topics);

            return Task.FromResult(true);
        }
    }
}
