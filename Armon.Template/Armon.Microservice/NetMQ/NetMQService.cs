using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Armon.Microservice.NetMQ
{
    internal class NetMQService
    {
        private log4net.ILog logger = log4net.LogManager.GetLogger(typeof(NetMQService));
        private static readonly NetMQService _instance = new NetMQService();
        private readonly Queue<NetMQMessage> _pubMsgQueue = new Queue<NetMQMessage>();
        private PublisherSocket _publisher;
        private NetMQPoller _poller;

        public static NetMQService Instance { get { return _instance; } }

        public void Start()
        {
            _publisher = new PublisherSocket();
            _publisher.SendReady += _publisher_SendReady;
            int pPort = 60085;
            _publisher.Bind(string.Format("tcp://*:{0}", 5555));
            logger.DebugFormat("Start publisher on {0}", pPort);
            _poller = new NetMQPoller { _publisher };

            _poller.RunAsync();
        }

        public void Stop()
        {
            if (_publisher != null)
            {
                _publisher.SendReady -= _publisher_SendReady;
                _publisher.Close();
            }

            if (_poller != null)
            {
                _poller.Stop();
                _poller.Remove(_publisher);
            }
        }

        private void _publisher_SendReady(object sender, NetMQSocketEventArgs e)
        {
            try
            {
                if (e.IsReadyToSend && _pubMsgQueue.Count > 0)
                {
                    var msg = _pubMsgQueue.Dequeue();
                    if (msg != null)
                    {
                        e.Socket.SendMultipartMessage(msg);
                    }
                }
                else
                {
                    Thread.Sleep(3);
                }
            }
            catch (Exception err)
            {
                logger.ErrorFormat("Pub message error:{0}", err);
            }
        }

        public void Publish(string msgKey, object messageValue)
        {
            if (string.IsNullOrEmpty(msgKey) || messageValue == null)
            {
                logger.WarnFormat("Can't pub message with empty key or null message");
                return;
            }

            NetMQMessage message = new NetMQMessage();
            message.Append(msgKey);
            message.Append(JsonConvert.SerializeObject(messageValue));

            _pubMsgQueue.Enqueue(message);
        }
    }
}