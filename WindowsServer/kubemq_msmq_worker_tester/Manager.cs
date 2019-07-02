using KubeMsmqSDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using Tradency.KubeMSMQ.Messages;
using Tradency.KubeMSMQ.SDK.csharp.Events;
using KubeMQ.MSMQSDK.Results;

namespace MSMQTester
{
    public class Manager
    {
        private int _KubeMQTimeout;
        private KubeMSMQInitiator kubeMSMQ;
        private readonly IConfiguration _config;
        private ILogger<Manager> _logger;
        public Manager(IConfiguration configuration, ILogger<Manager> logger)
        {
            _logger = logger;
            _config = configuration;
            _KubeMQTimeout = GetKubeMQTimeout();
            kubeMSMQ = new KubeMSMQInitiator();
            TestMSMQ();
            TestMSMQPeekEvent();
            TestMSMQRecieveEvent();
            Console.ReadLine();
        }

        private void TestMSMQPeekEvent()
        {
            string path = ".\\Private$\\peekevent";
            try
            {
                MessageQueue.Create(path);
                MessageQueue messageQueue = new MessageQueue(path);
                messageQueue.Formatter = new XmlMessageFormatter(new Type[]
                    {typeof(String)});
                messageQueue.PeekCompleted += new PeekCompletedEventHandler((s, a) => MyPeekCompleted(s, a));
                messageQueue.BeginPeek();
            }
            catch (Exception exe)
            {
                _logger.LogCritical(string.Format("Exception on {0}", exe));
                throw;
            }
        }

        private void TestMSMQRecieveEvent()
        {
            string path = ".\\Private$\\recieveevent";
            try
            {
                MessageQueue.Create(path);
                MessageQueue messageQueue = new MessageQueue(path);
                messageQueue.Formatter = new XmlMessageFormatter(new Type[]
                    {typeof(String)});
                messageQueue.ReceiveCompleted += new ReceiveCompletedEventHandler((s, a) => MyReciveCompleted(s, a));
                messageQueue.BeginReceive();
            }
            catch (Exception exe)
            {
                _logger.LogCritical(string.Format("Exception on {0}", exe));
                throw;
            }
        }

        private void MyReciveCompleted(object s, ReceiveCompletedEventArgs a)
        {
            _logger.LogDebug("receive info from Worker");
            Message message = a.Message;
            message.Formatter = new XmlMessageFormatter();
            System.IO.Stream stream = new System.IO.MemoryStream(message.BodyStream);
            StreamReader reader = new StreamReader(stream);
            string msgBody = reader.ReadToEnd();
        }

        private void MyPeekCompleted(object s, PeekCompletedEventArgs a)
        {
            _logger.LogDebug("Peek info from Worker");
            Message message = a.Message;
            message.Formatter = new XmlMessageFormatter();
            System.IO.Stream stream = new System.IO.MemoryStream(message.BodyStream);
            StreamReader reader = new StreamReader(stream);
            string msgBody = reader.ReadToEnd();
        }

        private void TestMSMQ()
        {
            string path = ".\\Private$\\TestQ";
            try
            {
                if(MessageQueue.Exists(path))
                {
                    _logger.LogDebug(string.Format("MessageQueue {0} Already exist", path));
                    MessageQueue.Delete(path);
                    _logger.LogDebug(string.Format("MessageQueue {0} deleted", path));
                }
                MessageQueue myNewPublicQueue = MessageQueue.Create(path);
                MessageQueue messageQueue = new MessageQueue(path);
                Message myMessage = new Message("Hello World");
                myMessage.Label = "LabelTest";
                messageQueue.Formatter = new XmlMessageFormatter();
                messageQueue.Send(myMessage);
                myMessage = messageQueue.Peek();
                System.IO.Stream stream = new System.IO.MemoryStream(myMessage.BodyStream);
                myMessage.Formatter = new XmlMessageFormatter();
                StreamReader reader = new StreamReader(stream);
                string msgBody = reader.ReadToEnd();
                messageQueue.Purge();
                MessageQueue.Delete(path);
            }
            catch (MessageQueueException ex)
            {
                _logger.LogCritical(string.Format("Exception on {0}", ex));
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(string.Format("Exception on {0}", ex));
                throw ex;
            }

        }

        private void TestMSMQWithException()
        {
            string path = ".\\newPublicQueue";
            try
            {
                if (MessageQueue.Exists(path))
                {
                    _logger.LogDebug(string.Format("MessageQueue {0} Already exist", path));
                    MessageQueue.Delete(path);
                    _logger.LogDebug(string.Format("MessageQueue {0} deleted", path));
                }
                MessageQueue myNewPublicQueue =
                MessageQueue.Create(path);
                MessageQueue messageQueue = new MessageQueue(path);
                Message myMessage = new Message("Hello World");
                messageQueue.Send(myMessage);
                myMessage = messageQueue.Peek();
                MessageQueue.Delete(path);
            }
            catch (MessageQueueException ex)
            {
                _logger.LogCritical("Test failed on ex {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Test failed on ex {0}", ex.Message);
                throw ex;
            }

        }


        private string GetChannelName()
        {
            // Get environment variable 'CHANNELNAME' from config
            string Channel = Environment.GetEnvironmentVariable("CHANNELNAME");

            if (string.IsNullOrEmpty(Channel))
            {
                _logger.LogWarning("Did not find environment variable 'CHANNELNAME'. getting address from appsettings.json.");

                // get address from appsettings.json
                Channel = Convert.ToString(_config["KubeMQ:ChannelName"]);
            }

            _logger.LogDebug("'ChannelName' was set to{0}", Channel.ToString());

            if (string.IsNullOrEmpty(Channel))
            {
                _logger.LogError("Did not find ChannelName");
            }


            return Channel;
        }



        private string GetKubeMQAddress()
        {
            // Get environment variable 'KUBEMQSADDRESS' from config
            string KubeMQAddress = Environment.GetEnvironmentVariable("KUBEMQSADDRESS");

            if (string.IsNullOrEmpty(KubeMQAddress))
            {
                _logger.LogWarning("Did not find environment variable 'KubeMQAddress'. getting address from appsettings.json.");

                // get address from appsettings.json
                KubeMQAddress = Convert.ToString(_config["KubeMQ:Address"]);
            }

            _logger.LogDebug("'KubeMQAddress' was set to{0}", KubeMQAddress.ToString());

            if (string.IsNullOrEmpty(KubeMQAddress))
            {
                _logger.LogError("Did not find KubeMQ Address");
            }


            return KubeMQAddress;
        }

        private int GetKubeMQTimeout()
        {
            int KubeMQTimeout = 0;

            // Get environment variable 'KUBEMQTIMEOUT' from config
            string strTimeout = Environment.GetEnvironmentVariable("KUBEMQTIMEOUT");

            if (string.IsNullOrEmpty(strTimeout))
            {
                _logger.LogWarning("Did not find environment variable 'KUBEMQTIMEOUT'. getting Timeout from appsettings.json.");

                // get Timeout from appsettings.json
                strTimeout = _config["KubeMQ:Timeout"];
            }

            if (string.IsNullOrEmpty(strTimeout))
            {
                _logger.LogError("Did not find KUBEMQTIMEOUT");
            }

            try
            {
                KubeMQTimeout = int.Parse(strTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid environment variable&&Appsetting 'KUBEMQTIMEOUT'");
            }

            _logger.LogDebug("'KUBEMQTIMEOUT' was set to{0}", KubeMQTimeout.ToString());

            return KubeMQTimeout;

        }
    }
}