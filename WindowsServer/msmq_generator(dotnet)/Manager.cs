﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Timers;

namespace msmq_generator
{
    /// <summary>
    /// Manager run the program main logic
    /// </summary>
    class Manager
    {
        private Dictionary<string,Rates> rateCollection;
        private string ListenPath;
        private string SendPath;
        private static System.Timers.Timer timer;
        public Manager(string pListenQueuePath, string pSendRateQueue)
        {
            ListenPath = pListenQueuePath;
            SendPath = pSendRateQueue;
            StartListen();
            startSendingRates();
            Console.WriteLine($"initialized Manger on path {ListenPath}");

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
                Console.WriteLine($"Failed to check if Queue {SendPath} exists on exception {ex}");
                throw ex;
            }
            if (!exist)
            {
                Console.WriteLine($"Queue {SendPath} does not exists, creating new Queue");
                try
                {
                    MessageQueue rateQueue = MessageQueue.Create(SendPath);
                    Console.WriteLine($"Queue {SendPath} created");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create Queue {SendPath} on exception {ex}");
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
            Console.WriteLine($"Starting to generate rates, will send them to {SendPath}");
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
                Console.WriteLine($"Failed to check if Queue {ListenPath} exists on exception {ex}");
                throw ex;
            }
            if (!exist)
            {
                Console.WriteLine($"Queue {ListenPath} does not exists, creating new Queue");
                try
                {
                    MessageQueue rateQueue = MessageQueue.Create(ListenPath);
                    Console.WriteLine($"Queue {ListenPath} created");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create Queue {ListenPath} on exception {ex}");
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
            Console.WriteLine($"Received message to queue");
            MessageQueue mq = (MessageQueue)source;
            Message m = mq.EndReceive(asyncResult.AsyncResult);
            RateRequest request = new RateRequest();
            try
            {
                request= JsonConvert.DeserializeObject<RateRequest>((string)m.Body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse active/inactive request on {ex}");
            }
            if (rateCollection.ContainsKey(request.Name))
            {
                Console.WriteLine($"Changing {request.Name} active to:{request.Active}");
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
            mq.BeginReceive();

            return;
        }



        /// <summary>
        /// Set the rate timer.
        /// </summary>
        private void SetRateTimer()
        {

            timer = new System.Timers.Timer(1000);

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
                newMessage.BodyStream = new MemoryStream(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(ratesList)));
                newMessage.Label = strLabel;
                MessageQueue queue = new MessageQueue(SendPath, QueueAccessMode.Send);
                queue.Send(newMessage, MessageQueueTransactionType.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send rates to queue on ex {ex}");
            }
        }
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
