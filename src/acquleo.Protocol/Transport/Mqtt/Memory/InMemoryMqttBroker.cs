using acquleo.DataBroker.MQTT;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace acquleo.Protocol.Transport.Mqtt.Memory
{
    /// <summary>
    /// 
    /// </summary>
    public class InMemoryMqttBrokerMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public TopicInfo topic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Retain { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ResponseTopic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<UserPropertyInfo> UserProperties { get; set; } = new List<UserPropertyInfo>();
    }

    internal class InMemoryMqttBroker
    {
        readonly static object intancelock = new object();
        static InMemoryMqttBroker instance;
        public static InMemoryMqttBroker Instance
        {
            get
            {
                lock (intancelock)
                {
                    if (instance == null)
                        instance = new InMemoryMqttBroker();
                    return instance;
                }
            }
        }

        internal class InMemoryMqttBrokerSubscriberTopic
        {
            public TopicInfo topic { get; set; }
            public InMemoryMqttBrokerClient client { get; set; }
        }

        readonly List<InMemoryMqttBrokerSubscriberTopic> subscribers = new List<InMemoryMqttBrokerSubscriberTopic>();
        readonly Dictionary<string, InMemoryMqttBrokerMessage> messages = new Dictionary<string, InMemoryMqttBrokerMessage>();

        internal InMemoryMqttBroker()
        {

        }

        public async Task NotifyRetainMessages(InMemoryMqttBrokerClient client, TopicInfo topics)
        {
            List<InMemoryMqttBrokerMessage> toBeNotified = new List<InMemoryMqttBrokerMessage> { };
            lock (messages)
            {
                foreach (var msg in messages)
                {
                    if (InMemoryMqttTopicFilterComparer.Compare(msg.Key, topics.Topic) == InMemoryMqttTopicFilterCompareResult.IsMatch)
                    {
                        toBeNotified.Add(msg.Value);
                    }
                }
            }


            foreach (var msg in toBeNotified)
            {
                await client.PushMessage(msg);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="topics"></param>
        /// <returns></returns>
        public void Subscribe(InMemoryMqttBrokerClient client, List<TopicInfo> topics)
        {
            lock (subscribers)
            {
                foreach (var topic in topics)
                {
                    subscribers.Add(new InMemoryMqttBrokerSubscriberTopic() { client = client, topic = topic });

                    new TaskFactory().StartNew(async () => { await NotifyRetainMessages(client, topic); });
                }
            }


        }

        public void Publish(InMemoryMqttBrokerMessage message)
        {
            if (message.Retain)
            {
                lock (messages)
                {
                    if (!messages.ContainsKey(message.topic.Topic))
                        messages.Add(message.topic.Topic, message);
                    else
                        messages[message.topic.Topic] = message;
                }
            }

            new TaskFactory().StartNew(async () => { await PublishToSubscribers(message); });

        }

        async Task PublishToSubscribers(InMemoryMqttBrokerMessage message)
        {
            List<InMemoryMqttBrokerSubscriberTopic> link;
            lock (subscribers)
            {
                link = subscribers.ToList();
            }

            foreach (var topic in link)
            {
                if (InMemoryMqttTopicFilterComparer.Compare(message.topic.Topic, topic.topic.Topic) == InMemoryMqttTopicFilterCompareResult.IsMatch)
                {
                    await topic.client.PushMessage(message);
                }

            }
        }

    }
}
