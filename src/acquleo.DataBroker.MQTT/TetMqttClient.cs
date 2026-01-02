using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TetSistemi.Base.Logging;

namespace acquleo.DataBroker.MQTT
{
    public class TetMqttClient : ITetMqttClient
    {
        const string healthTopic = "mqtt/clients/{0}/connected";
        const string healthConnectedValue = "1";
        const string disconnectedValue = "0";

        bool enabled;
        readonly TetMqttClientConfig cfg;
        readonly IApplicationLog logger;
        readonly Encoding encoding;
        readonly IManagedMqttClient client;
        readonly MQTTnet.MqttFactory factory = new MQTTnet.MqttFactory();
        bool connected = false;
        public string ClientId => this.cfg.clientid;

        public string Host => this.cfg.host;

        public int Port => this.cfg.port;

        public bool IsConnected
        {
            get
            {
                if (client == null) return false;

                return this.client.IsConnected;
            }
        }

        string GetHealthTopic()
        {
            return string.Format(healthTopic, this.cfg.clientid);
        }

        public event dClientEvent Connected;
        public event dClientEvent Disconnected;
        public event dClientMessageEvent MessageReceived;

        public TetMqttClient(TetMqttClientConfig cfg)
        {
            this.cfg = cfg;

            this.logger = SingletonFactoryProvider.Provider.GetApplicationLogFactory().GetBuilder().WithObject(this).Build();
            this.client = factory.CreateManagedMqttClient();
            this.client.ApplicationMessageReceivedAsync += Client_ApplicationMessageReceivedAsync;
            this.client.ConnectedAsync += Client_ConnectedAsync;
            this.client.DisconnectedAsync += Client_DisconnectedAsync;
            this.client.ConnectingFailedAsync += Client_ConnectingFailedAsync;

            if (this.cfg.clientid == String.Empty) this.cfg.clientid = Guid.NewGuid().ToString();

            this.encoding = System.Text.Encoding.GetEncoding(cfg.encoding);
        }

        private Task Client_ConnectingFailedAsync(ConnectingFailedEventArgs arg)
        {
            this.logger.Log(LogLevels.Error, this, $@"MQTT client {this.cfg.clientid} Client_ConnectingFailedAsync", arg.Exception);

            return Task.CompletedTask;
        }
        public void Disable()
        {
            this.Disable(false);
        }

        public void Disable(bool disposing)
        {
            if (!enabled) return;

            Task beforeClose = Task.CompletedTask;

            if (this.cfg.health.Enabled && this.client.IsConnected)
            {
                //questo codice pubblica lo stato della connessione disattiva

                MqttApplicationMessage connMessage = new MqttApplicationMessage();
                connMessage.Topic = GetHealthTopic();
                connMessage.Retain = true;
                connMessage.PayloadSegment = new ArraySegment<byte>(UnicodeEncoding.UTF8.GetBytes(disconnectedValue));
                connMessage.QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce;

                beforeClose = this.client.InternalClient.PublishAsync(connMessage);

                this.logger.Info(this, $@"{this.cfg.clientid} publish health");
            }

            beforeClose.ConfigureAwait(false).GetAwaiter().OnCompleted(() =>
            {
                client.StopAsync().Wait();

                if (disposing)
                {
                    this.client.Dispose();
                }
            });

            this.logger.Log(LogLevels.Info, this, $@"Disabled MQTT client {this.cfg.clientid} host {this.cfg.host} port {this.cfg.port}");

            enabled = false;

            
        }

        public async void Enable()
        {
            if (enabled) return;

            enabled = true;

            var cli_options = factory.CreateClientOptionsBuilder();

            cli_options = cli_options.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500);
            cli_options = cli_options.WithClientId(this.cfg.clientid)
                .WithEndPoint(new DnsEndPoint(this.cfg.host, this.cfg.port, AddressFamily.Unspecified))
                .WithTcpServer(
                (opt) =>
                {
                    opt.BufferSize = this.cfg.bufferSize;
                });

            if (this.cfg.username != String.Empty)
            {
                cli_options = cli_options.WithCredentials(this.cfg.username, this.cfg.password);
            }
            if (this.cfg.tls.useTls)
            {
                cli_options = cli_options.WithTlsOptions(o =>
                {
                    o.UseTls()
                    .WithAllowUntrustedCertificates(this.cfg.tls.allowUntrustedCertificates)
                    .WithIgnoreCertificateChainErrors(this.cfg.tls.ignoreCertificateChainErrors)
                    .WithIgnoreCertificateRevocationErrors(this.cfg.tls.ignoreCertificateRevocationErrors);
                    if (this.cfg.tls.certificateHostName!=string.Empty)
                    {
                        o.WithTargetHost(this.cfg.tls.certificateHostName);
                    }
                    if (this.cfg.tls.disableCertificateValidation)
                    {
                        o.WithCertificateValidationHandler(h => { return true; });
                    }
                    
                }); 
            }

            if(this.cfg.health.Enabled)
            {
                //questo codice configura il testamento per pubblicare lo stato della connessione disattiva in caso di interruzione non prevista
                cli_options.WithWillPayload(disconnectedValue)
                    .WithWillQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                    .WithWillRetain(true).
                    WithWillTopic(GetHealthTopic());
            }
            var managed_options = new ManagedMqttClientOptionsBuilder();
            managed_options.WithClientOptions(cli_options)
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithMaxPendingMessages(int.MaxValue)
                .WithMaxTopicFiltersInSubscribeUnsubscribePackets(int.MaxValue)
                .WithPendingMessagesOverflowStrategy(MQTTnet.Server.MqttPendingMessagesOverflowStrategy.DropOldestQueuedMessage);

            await this.client.StartAsync(managed_options.Build()).ConfigureAwait(false);
          
            this.logger.Log(LogLevels.Info, this, $@"Enabled MQTT client {this.cfg.clientid} host {this.cfg.host} port {this.cfg.port}");
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled()
        {
            return enabled;
        }

        public static MQTTnet.Protocol.MqttQualityOfServiceLevel getLevel(byte num)
        {
            if (num <= 0) return MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce;
            if (num == 1) return MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce;
            return MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce;
        }

        public static byte getLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel en)
        {
            if (en == MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce) return 0;
            if (en == MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) return 1;
            return 2;
        }

        public async Task Subscribe(List<TopicInfo> topics)
        {
            await this.SubscribeAsync(topics).ConfigureAwait(false);
        }

        MQTTnet.Protocol.MqttRetainHandling Convert(SubscriptionRetainTypes types)
        {
            switch(types)
            {
                case SubscriptionRetainTypes.DoNotSendOnSubscribe: 
                    return MQTTnet.Protocol.MqttRetainHandling.DoNotSendOnSubscribe;
                case SubscriptionRetainTypes.SendAtSubscribe:
                    return MQTTnet.Protocol.MqttRetainHandling.SendAtSubscribe;
                case SubscriptionRetainTypes.SendAtSubscribeIfNewSubscriptionOnly:
                    return MQTTnet.Protocol.MqttRetainHandling.SendAtSubscribeIfNewSubscriptionOnly;
            }
            return MQTTnet.Protocol.MqttRetainHandling.SendAtSubscribe;
        }

        public Task SubscribeAsync(List<TopicInfo> topics)
        {
            if (!this.enabled) throw new NotEnabledException(string.Empty);
            if(topics.Count==0)
            {
                this.logger.WarningIf(this, () => $@"mqtt client {this.ClientId} no subscription topics configured");

                return Task.CompletedTask;
            }
            List<MqttTopicFilter> filters = new List<MqttTopicFilter>();

            foreach (var topic in topics)
            {
                filters.Add(new MqttTopicFilter()
                {
                    QualityOfServiceLevel = getLevel(topic.QosLevel),
                    RetainAsPublished = topic.RetainAsPublished,
                    RetainHandling = Convert(topic.SubscriptionRetainType),
                    Topic = topic.Topic
                });

                this.logger.Log(LogLevels.Info, this, $@"MQTT client {this.cfg.clientid} host {this.cfg.host} port {this.cfg.port} subscription {topic.Topic} qos {topic.QosLevel}");
            }
            try
            {
                MqttClientSubscribeOptions options = new MqttClientSubscribeOptions();
                options.TopicFilters = filters;
                

                return this.client.InternalClient.SubscribeAsync(options);
            }
            catch (ObjectDisposedException)
            {
                return Task.CompletedTask;
            }
        }

        public Task<bool> PublishAsync(TopicInfo topic, string message, bool retain,
            string contentType = null, string responseTopic = null, List<UserPropertyInfo> userPropertyInfo = null, byte formatIndicator=0)
        {
            if (!this.enabled) return Task.FromResult(false);

            if (this.client == null) return Task.FromResult(false);

            if (!this.client.IsConnected) return Task.FromResult(false);

            var message_byte = encoding.GetBytes(message);

            return PublishAsync(topic, message_byte, retain, contentType, responseTopic, userPropertyInfo);
        }

        public async Task<bool> PublishAsync(TopicInfo topic, byte[] message, bool retain,
            string contentType = null, string responseTopic = null, List<UserPropertyInfo> userPropertyInfo = null, byte formatIndicator = 0)
        {
            if (!this.enabled) return false;

            if (this.client == null) return false;

            if (!this.client.IsConnected) return false;

            var message_byte = message;

            MqttApplicationMessage msg = new MqttApplicationMessage()
            {
                PayloadSegment = new ArraySegment<byte>(message_byte),
                Retain = retain,
                Topic = topic.Topic,
                QualityOfServiceLevel = getLevel(topic.QosLevel),
                PayloadFormatIndicator = (MQTTnet.Protocol.MqttPayloadFormatIndicator)formatIndicator
            };

            if (responseTopic != null)
            {
                msg.ResponseTopic = responseTopic;
            }
            if (contentType != null)
            {
                msg.ContentType = contentType;
            }
            if (userPropertyInfo != null && userPropertyInfo.Count > 0)
            {
                msg.UserProperties = new List<MqttUserProperty>();

                foreach (var pro in userPropertyInfo)
                {
                    msg.UserProperties.Add(new MqttUserProperty(pro.Name, pro.Value));
                }
            }

            try
            {
                var result = await this.client.InternalClient.PublishAsync(msg).ConfigureAwait(false);
                return result.IsSuccess;
            }
            catch (ObjectDisposedException)
            {
                //NOTHING TO DO
            }

            this.logger.Log(LogLevels.Trace, this, $@"MQTT client {this.cfg.clientid} host {this.cfg.host} port {this.cfg.port} publish {topic.Topic} qos {topic.QosLevel} ");

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Disable(true);
            }
        }

        private Task Client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            try
            {
                if (this.MessageReceived != null)
                {
                    var msg = new MessageArgs()
                    {
                        DupFlag = arg.ApplicationMessage.Dup,
                        QosLevel = getLevel(arg.ApplicationMessage.QualityOfServiceLevel),
                        Retain = arg.ApplicationMessage.Retain,
                        Topic = arg.ApplicationMessage.Topic,
                        ContentType = arg.ApplicationMessage.ContentType,
                        ResponseTopic = arg.ApplicationMessage.ResponseTopic,
                        Message = arg.ApplicationMessage.PayloadSegment.Array
                    };

                    this.MessageReceived?.Invoke(this, msg);
                }
            }
            catch(Exception ex)
            {
                this.logger.Log(LogLevels.Error, this, $@"MQTT client {this.cfg.clientid} Client_ApplicationMessageReceivedAsync", ex);
            }

            return Task.CompletedTask;
        }

        private Task Client_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            try
            {
                if (!connected) return Task.CompletedTask;

                connected = false;

                this.Disconnected?.Invoke(this);

                return Task.CompletedTask;
            }
            catch (MqttCommunicationException ex)
            {
                connected = false;

                this.logger.Log(LogLevels.Error, this, $@"MQTT client {this.cfg.clientid} Client_DisconnectedAsync", ex);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevels.Error, this, $@"MQTT client {this.cfg.clientid} Client_DisconnectedAsync", ex);
                return Task.CompletedTask;
            }
        }

        private async Task Client_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            if (connected) return;

            try
            {
                connected = true;

                this.Connected?.Invoke(this);

                if (this.cfg.health.Enabled)
                {
                    //questo codice pubblica lo stato della connessione attiva

                    MqttApplicationMessage connMessage = new MqttApplicationMessage();
                    connMessage.Topic = GetHealthTopic();
                    connMessage.Retain = true;
                    connMessage.PayloadSegment = new ArraySegment<byte>(UnicodeEncoding.UTF8.GetBytes(healthConnectedValue));
                    connMessage.QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce;

                    var result = await this.client.InternalClient.PublishAsync(connMessage);

                    this.logger.Info(this, $@"{this.cfg.clientid} result publish health {result}");
                }
            }
            catch (MqttCommunicationException ex)
            {
                connected = false;

                this.logger.Log(LogLevels.Error, this, $@"MQTT client {this.cfg.clientid} Client_ConnectedAsync", ex);
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevels.Error, this, $@"MQTT client {this.cfg.clientid} Client_ConnectedAsync", ex);
            }
        }
    }
}
