using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Timers;
using System.Xml.Linq;

namespace kubemq_msmq_rates_generator
{
    /// <summary>
    /// Manager run the program main logic.
    /// Contain the Random for all the rates classes.
    /// </summary>
    class Manager
    {
        public static Random rnd;
        private Dictionary<string,Rates> rateCollection;
        private string ListenPath;
        private string SendPath;
        private int RateInterval;
        private static System.Timers.Timer timer;
        private readonly IConfiguration _config;
        private ILogger<Manager> _logger;
        public Manager(IConfiguration configuration, ILogger<Manager> logger)
        {
            _logger = logger;
            _config = configuration;
            rnd = new Random();

            ListenPath = GetCommendQueueName();
            SendPath = GetRateQueueName();
            RateInterval = GetRateInterval();
            StartListen();
            startSendingRates();
            _logger.LogInformation($"initialized Manger on listen path {ListenPath} , and send path {SendPath}");

        }
        /// <summary>
        /// Start sending rate to MSMQ under selected path
        /// </summary>
        private void startSendingRates()
        {
            bool exist = false;
            try
            {
                exist = MessageQueue.Exists(SendPath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to check if Queue {SendPath} exists on exception {ex}");
                throw ex;
            }
            if (!exist)
            {
                _logger.LogDebug($"Queue {SendPath} does not exists, creating new Queue");
                try
                {
                    MessageQueue rateQueue = MessageQueue.Create(SendPath);
                    _logger.LogDebug($"Queue {SendPath} created");

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to create Queue {SendPath} on exception {ex}");
                    throw ex;
                }
            }
            rateCollection = new Dictionary<string, Rates>
            {
                {"Golden Coin", new Rates("Golden Coin",1) },
                {"GIL",new Rates("GIL",2) },
                {"Zenny", new Rates("Zenny",3) },
                {"Dollar", new Rates("Dollar",4) },
                {"Red Orbs", new Rates("Red Orbs",5) },
                {"Credit", new Rates("Credit",6) },
                {"Vespene gas",new Rates("Vespene gas",7) }
            };
            _logger.LogInformation($"Starting to generate rates, will send them to {SendPath}");
            SetRateTimer();
        }
        /// <summary>
        /// Starts receiving request from MSMQ under selected path
        /// </summary>
        private void StartListen()
        {
            bool exist = false;
            try
            {
                exist = MessageQueue.Exists(ListenPath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to check if Queue {ListenPath} exists on exception {ex}");
                throw ex;
            }
            if (!exist)
            {
                _logger.LogDebug($"Queue {ListenPath} does not exists, creating new Queue");
                try
                {
                    MessageQueue rateQueue = MessageQueue.Create(ListenPath);
                    _logger.LogDebug($"Queue {ListenPath} created");

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to create Queue {ListenPath} on exception {ex}");
                    throw ex;
                }
            }
            MessageQueue messageQueue = new MessageQueue(ListenPath);
            messageQueue.Formatter = new BinaryMessageFormatter();
            messageQueue.ReceiveCompleted += new ReceiveCompletedEventHandler((s, a) => MyReciveCompleted(s, a));
            messageQueue.BeginReceive();
        }

        /// <summary>
        /// On receiving message from Queue , main use to active or deactivate rates 
        /// </summary>
        private void MyReciveCompleted(object source, ReceiveCompletedEventArgs asyncResult)
        {
            _logger.LogDebug($"Received message to queue");
            MessageQueue mq = (MessageQueue)source;
            asyncResult.Message.Formatter = new ActiveXMessageFormatter();

            System.IO.Stream a = new System.IO.MemoryStream(ReadFully(asyncResult.Message.BodyStream));
            RateRequest request = new RateRequest();
            try
            {
                request = JsonConvert.DeserializeObject<RateRequest>(asyncResult.Message.Body.ToString());
                if (rateCollection.ContainsKey(request.Name))
                {
                    _logger.LogInformation($"Changing {request.Name} active to:{request.Active}");
                    switch (request.Active)
                    {
                        case true:
                            rateCollection[request.Name].isActive = true;
                            break;
                        case false:
                            rateCollection[request.Name].isActive = false;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to parse active/inactive request on {ex}");
            }
            mq.BeginReceive();

            return;
        }



        /// <summary>
        /// Set the rate timer.
        /// </summary>
        private void SetRateTimer()
        {

            timer = new System.Timers.Timer(RateInterval);

            timer.Elapsed += OnRateSend;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        /// <summary>
        /// Create and send the active rate report.
        /// </summary>
        private void OnRateSend(Object source, ElapsedEventArgs e)
        {
            List<SenderMessageBody> ratesList = new List<SenderMessageBody>();
            foreach (var rate in rateCollection)
            {
                if (rate.Value.isActive)
                {
                    SenderMessageBody message = new SenderMessageBody()
                    {
                        ID=rate.Value.id,
                        Name = rate.Value.rateName,
                        Ask = rate.Value.buy.ToString(),
                        Bid = rate.Value.sell.ToString()
                    };
                    ratesList.Add(message);
                }
            }
            try
            {
                string strLabel = "";
                System.Messaging.Message newMessage = new System.Messaging.Message();
                string rateList = JsonConvert.SerializeObject(ratesList);
                newMessage.BodyStream = new MemoryStream(Encoding.ASCII.GetBytes(rateList));
                newMessage.Label = strLabel;
                MessageQueue queue = new MessageQueue(SendPath, QueueAccessMode.Send);
                queue.Send(newMessage, MessageQueueTransactionType.None);
                _logger.LogDebug($"saved rate {rateList}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send rates to queue on ex {ex}");
            }
        }

        /// <summary>
        /// Read a stream input and return byte[]
        /// </summary>
        /// <param name="input">Receive System.IO.Stream</param>
        /// <returns>Return the stream input byte[]</returns>
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


        #region configuration load-up
        private string GetCommendQueueName()
        {


            string cmdQu = Convert.ToString(_config["CMDQueue"]);

            _logger.LogDebug("'commend queue' was set to{0}", cmdQu.ToString());

            if (string.IsNullOrEmpty(cmdQu))
            {
                _logger.LogError("Did not find cmdQu");
            }


            return cmdQu;
        }

        private string GetRateQueueName()
        {

            // get address from appsettings.json
            string rateQu = Convert.ToString(_config["RateQueue"]);

            _logger.LogDebug("'RateQueue' was set to{0}", rateQu.ToString());

            if (string.IsNullOrEmpty(rateQu))
            {
                _logger.LogError("Did not find RateQueue");
            }


            return rateQu;
        }

        private int GetRateInterval()
        {

            // get interval from appsettings.json
            string rateQu = Convert.ToString(_config["RateInterval"]);
            int parsedInterval = 0;
            int.TryParse(rateQu, out parsedInterval);

            if (parsedInterval<0)
            {
                _logger.LogError("failed to set 'RateInterval' to value of {0}, setting to defualt of 3000", rateQu.ToString());
                parsedInterval = 3000;
            }
            _logger.LogDebug("'RateInterval' was set to{0}", rateQu.ToString());


            return parsedInterval;
        }
        #endregion
    }
    /// <summary>
    /// Rate sending to the Client
    /// </summary>
    [Serializable]
    class SenderMessageBody
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Ask { get; set; }
        public string Bid { get; set; }
    }

    /// <summary>
    /// Request Receiving from the Client
    /// </summary>
    [Serializable]
    public class RateRequest
    {
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}
