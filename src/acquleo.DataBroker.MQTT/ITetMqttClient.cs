using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TetSistemi.Base.Interfaces;

namespace acquleo.DataBroker.MQTT
{
    public delegate void dClientMessageEvent(ITetMqttClient client, MessageArgs message);

    public delegate void dClientEvent(ITetMqttClient client);

    public interface ITetMqttClient : IEnabler, IDisposable
    {
        string ClientId { get; }
        string Host { get; }
        int Port { get; }
        bool IsConnected { get; }

        event dClientEvent Connected;

        event dClientEvent Disconnected;

        event dClientMessageEvent MessageReceived;

        Task SubscribeAsync(List<TopicInfo> topics);
        Task<bool> PublishAsync(TopicInfo topic, string message, bool retain, string contentType = null, string responseTopic = null, List<UserPropertyInfo> userPropertyInfo = null, byte formatIndicator = 0);
        Task<bool> PublishAsync(TopicInfo topic, byte[] message, bool retain, string contentType = null, string responseTopic = null, List<UserPropertyInfo> userPropertyInfo = null, byte formatIndicator = 0);
    }

    public class TopicInfo
    {
        public string Topic { get; set; } = string.Empty;
        public byte QosLevel { get; set; } = 0;
        public bool RetainAsPublished { get; set; } = false;        
        public SubscriptionRetainTypes SubscriptionRetainType { get; set; } =  SubscriptionRetainTypes.SendAtSubscribe;
    }

    public enum SubscriptionRetainTypes
    {
        SendAtSubscribe = 0,

        SendAtSubscribeIfNewSubscriptionOnly = 1,

        DoNotSendOnSubscribe = 2
    }


    public class UserPropertyInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class MessageArgs
    {
        public string ResponseTopic { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public string Topic { get; set; } = string.Empty;

        public byte[] Message { get; set; }

        public bool DupFlag { get; set; } = false;

        public byte QosLevel { get; set; } = 0;

        public bool Retain { get; set; } = false;
    }
}
