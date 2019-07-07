using KubeMQ.SDK.csharp.CommandQuery;
using KubeMQ.SDK.csharp.CommandQuery.LowLevel;
using KubeMQ.SDK.csharp.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MSMQWorkerConsole.Converters;
using System;
using System.Collections.Concurrent;
using System.Messaging;
using System.Threading.Tasks;
using KubeMQ.MSMQSDK;
using KubeMQ.MSMQSDK.Results;

namespace MSMQWorkerConsole
{
    public class CommonMethods
    {
        //private List<System.Messaging.MessageQueue> OpenPeekQueues;
        //private List<System.Messaging.MessageQueue> OpenRecieveQueues;
        private ConcurrentDictionary<string, System.Messaging.MessageQueue> OpenPeekQueues;
        private ConcurrentDictionary<string, System.Messaging.MessageQueue> OpenRecieveQueues;
        private ILogger<CommonMethods> _logger;
        private Initiator initiator = new Initiator();
        private int _timeout;
        private readonly IConfiguration _config;
        private string clientID;
        public CommonMethods(ILogger<CommonMethods> logger, IConfiguration configuration) :base()
        {
            _logger = logger;
            _config = configuration;
            initiator = new Initiator(_config["KubeMQ:Address"]);
            clientID = $"Client:{GetChannelName()}";
             if(!int.TryParse(_config["KubeMQ:Timeout"],out _timeout))
            {
                _timeout = 1000;
            }
            OpenPeekQueues = new ConcurrentDictionary<string, System.Messaging.MessageQueue>();
            OpenRecieveQueues = new ConcurrentDictionary<string, System.Messaging.MessageQueue>();
        }

        /// <summary>
        /// Check if MSMQchannel exist
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal ResultModel Exists(string path)
        {
            ResultModel resultModel = new ResultModel();
            try
            {
                bool Exists= System.Messaging.MessageQueue.Exists(path);
                _logger.LogDebug(string.Format("Queue : {0} Exists", path));
                resultModel.Result = (Exists == true) ? (int)ResultsEnum.AlreadyExist : (int)ResultsEnum.ChannelDosntExist;
            }
            catch (System.Messaging.MessageQueueException ex)
            {
                KubeMQ.MSMQSDK.Results.MessageQueueException messageEx = new KubeMQ.MSMQSDK.Results.MessageQueueException(ex);
                _logger.LogCritical(string.Format("MSMQ Path:{0} Exists check failed on ex :{1}", path, ex.Message));
                resultModel.Result = (int)ResultsEnum.Error;
                resultModel.exception = messageEx;
            }
            catch (Exception ex)
            {
                resultModel.Result = (int)ResultsEnum.Error;
                _logger.LogCritical(string.Format("MSMQ Path:{0} Exists check failed on ex :{1}", path, ex.Message));
                resultModel.exception = ex;
            }
            return resultModel;
        }
        /// <summary>
        /// Create Queue with path name
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal ResultModel CreateQueue(string path)
        {
            ResultModel resultModel = new ResultModel();
            try
            {
                System.Messaging.MessageQueue.Create(path);
                _logger.LogDebug(string.Format("MSMQPathCreated :{0}", path));
                resultModel.Result = (int)ResultsEnum.Created;
            }
            catch (System.Messaging.MessageQueueException ex)
            {
                KubeMQ.MSMQSDK.Results.MessageQueueException messageEx = new KubeMQ.MSMQSDK.Results.MessageQueueException(ex);
                _logger.LogCritical(string.Format("MSMQ Path:{0} Creation failed on ex :{1}", path, ex.Message));
                resultModel.Result = (int)ResultsEnum.Error;
                resultModel.exception = messageEx;
            }
            catch (Exception ex)
            {
                resultModel.Result = (int)ResultsEnum.Error;
                _logger.LogCritical(string.Format("MSMQ Path:{0} Creation failed on ex :{1}", path, ex.Message));
                resultModel.exception = ex;
            }
            return resultModel;
        }



        /// <summary>
        /// Purge Queue With Path Name
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal ResultModel PurgeQueue(string path)
        {
            ResultModel resultModel = new ResultModel();
            try
            {
                System.Messaging.MessageQueue messageQueue = new System.Messaging.MessageQueue(path);
                messageQueue.Purge();
                resultModel.Result = (int)ResultsEnum.QueuePurged;
                _logger.LogDebug(string.Format("Queue : {0} Purged", path));
            }
            catch (System.Messaging.MessageQueueException ex)
            {
                KubeMQ.MSMQSDK.Results.MessageQueueException messageEx = new KubeMQ.MSMQSDK.Results.MessageQueueException(ex);
                _logger.LogCritical(string.Format("MSMQ Path:{0} Purge failed on ex :{1}", path, ex.Message));
                resultModel.Result = (int)ResultsEnum.Error;
                resultModel.exception = messageEx;
            }
            catch (Exception ex)
            {
                resultModel.Result = (int)ResultsEnum.Error;
                _logger.LogCritical(string.Format("MSMQ Path:{0} Purge failed on ex :{1}", path, ex.Message));
                resultModel.exception = ex;
            }
            return resultModel;
        }

        /// <summary>
        /// Delete Queue With Path Name
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal ResultModel DeleteQueue(string path)
        {
            ResultModel resultModel = new ResultModel();
            try
            {
                System.Messaging.MessageQueue.Delete(path);
                _logger.LogDebug(string.Format("MSMQ Path:{0} Deleted", path));
                resultModel.Result = (int)ResultsEnum.Deleted;
            }
            catch (Exception ex)
            {
                resultModel.Result = (int)ResultsEnum.Error;
                _logger.LogCritical(string.Format("MSMQ Path:{0} Deleted failed on ex :{1}", path, ex.Message));
                resultModel.exception = ex;
            }
            return resultModel;
        }

        /// <summary>
        /// Send message to existing queue
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal ResultModel SendToQueue(MSMQMeta meta, KubeMQ.MSMQSDK.Messages.Message message)
        {
            ResultModel resultModel = new ResultModel();
            try
            {
                System.Messaging.MessageQueue myQueue = new System.Messaging.MessageQueue(meta.Path);
                myQueue.Formatter = new ActiveXMessageFormatter();
                System.Messaging.Message MyMessage = MessageConvert.ConvertToSystemMessage(message);
                MyMessage.Formatter = new ActiveXMessageFormatter();
                myQueue.Send(MyMessage);
                _logger.LogDebug(string.Format("Added message to Queue:{0}", meta.Path));
                resultModel.Result = (int)ResultsEnum.AddedToQueue;
            }
            catch (System.Messaging.MessageQueueException ex)
            {
                KubeMQ.MSMQSDK.Results.MessageQueueException messageEx = new KubeMQ.MSMQSDK.Results.MessageQueueException(ex);
                _logger.LogCritical("Failed Sending Message to queue {0} on exception {1}", meta.Path, ex.Message);
                resultModel.Result = (int)ResultsEnum.Error;
                resultModel.exception = messageEx;
            }
            catch (Exception ex)
            {
                resultModel.Result = (int)ResultsEnum.Error;
                _logger.LogCritical("Failed Sending Message to queue {0} on exception {1}", meta.Path, ex.Message);
                resultModel.exception = ex;
            }
            return resultModel;
        }

        /// <summary>
        /// Peek and return the first message in the queue
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal ResultModel PeekQueue(string path)
        {
            ResultModel resultModel = new ResultModel();
            try
            {
                System.Messaging.MessageQueue myQueue = new System.Messaging.MessageQueue(path);
                System.Messaging.Message myMessage = myQueue.Peek();
                resultModel.message = MessageConvert.ConvertFromSystemMessage(myMessage);
                _logger.LogDebug(string.Format("Peek done on path :{0}", path));
                resultModel.Result = (int)ResultsEnum.Done;
            }
            catch (Exception ex)
            {
                resultModel.Result = (int)ResultsEnum.Error;
                _logger.LogCritical(string.Format("Failed to peek path {0} on ex {1}", path, ex.Message));
                resultModel.exception = ex;
            }
            return resultModel;
        }

        #region Events
        /// <summary>
        /// Frees all resources allocated by the System.Messaging.MessageQueue. regarding a specific queue
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal ResultModel UnregisterPeek(string path)
        {
            ResultModel resultModel = new ResultModel();
            if (OpenPeekQueues != null && OpenPeekQueues.Count > 0)
            {
                try
                {
                    if (OpenPeekQueues.ContainsKey(path))
                    {
                        OpenRecieveQueues[path].Close();
                        _logger.LogDebug(string.Format("Ended Peek on Queue :{0} ", path));
                        resultModel.Result = (int)ResultsEnum.EndPeek;
                    }
                }
                catch (Exception ex)
                {
                    resultModel.Result = (int)ResultsEnum.Error;
                    resultModel.exception = ex;
                }
            }
            if (resultModel.Result == (int)ResultsEnum.Empty)
            {
                resultModel.Result = (int)ResultsEnum.Error;
                resultModel.exception = new Exception(string.Format("Could not find queue :{0}", path));
            }
            return resultModel;
        }

        internal ResultModel UnregisterReceive(string path)
        {
            ResultModel resultModel = new ResultModel();
            if (OpenRecieveQueues != null && OpenRecieveQueues.Count > 0)
            {
                try
                {
                    if (OpenRecieveQueues.ContainsKey(path))
                    {
                        OpenRecieveQueues[path].Close();
                        _logger.LogDebug(string.Format("Ended Receive on Queue :{0} ", path));
                        resultModel.Result = (int)ResultsEnum.Done;
                    }
                }
                catch (Exception ex)
                {
                    resultModel.Result = (int)ResultsEnum.Error;
                    resultModel.exception = ex;
                }
            }
            if (resultModel.Result == (int)ResultsEnum.Empty)
            {
                resultModel.Result = (int)ResultsEnum.Error;
                resultModel.exception = new Exception(string.Format("Could not find queue :{0}", path));
            }
            return resultModel;
        }
        /// <summary>
        /// Initiates an asynchronous peek operation that has no time-out. The operation
        ///    is not complete until a message becomes available in the queue.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal ResultModel BeginPeek(string path)
        {
            ResultModel resultModel = new ResultModel();
            if (OpenPeekQueues != null && OpenPeekQueues.Count > 0)
            {
                try
                {
                    if (OpenPeekQueues.ContainsKey(path))
                    {
                        OpenPeekQueues[path].BeginPeek();
                        _logger.LogDebug(string.Format("Begin Peek on Queue :{0} ", path));
                        resultModel.Result = (int)ResultsEnum.BeginPeek;
                    }
                }
                catch (Exception ex)
                {
                    resultModel.Result = (int)ResultsEnum.Error;
                    resultModel.exception = ex;
                }
            }


            if (resultModel.Result == (int)ResultsEnum.Empty)
            {
                resultModel.Result = (int)ResultsEnum.Error;
                resultModel.exception = new Exception(string.Format("Could not find queue :{0}", path));
            }
            return resultModel;
        }
        internal ResultModel BeginRecieve(string path)
        {
            ResultModel resultModel = new ResultModel();
            if (OpenRecieveQueues != null && OpenRecieveQueues.Count > 0)
            {
                try
                {
                    if (OpenRecieveQueues.ContainsKey(path))
                    {
                        OpenRecieveQueues[path].BeginReceive();
                        _logger.LogDebug(string.Format("Begin Receive on Queue :{0} ", path));
                        resultModel.Result = (int)ResultsEnum.BeginReceive;
                    }
                }
                catch (Exception ex)
                {
                    resultModel.Result = (int)ResultsEnum.Error;
                    resultModel.exception = ex;
                }

            }
            if (resultModel.Result == (int)ResultsEnum.Empty)
            {
                resultModel.Result = (int)ResultsEnum.Error;
                resultModel.exception = new Exception(string.Format("Could not find queue :{0}", path));
            }
            return resultModel;
        }
        internal ResultModel EventReceive(string path, string ChannelToReturnTo)
        {
            System.Messaging.MessageQueue MyQueue = new System.Messaging.MessageQueue(path);
            MyQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(async (s, a) => await MyReceiveCompleted(s, a, ChannelToReturnTo, path));
            OpenRecieveQueues.TryAdd(path,MyQueue);
            ResultModel resultModel = new ResultModel();
            resultModel.Result = (int)ResultsEnum.Done;
            return resultModel;
        }

        private async Task MyReceiveCompleted(Object source,ReceiveCompletedEventArgs asyncResult, string channelToReturnTo, string path)
        {
            ResultModel resultModel = new ResultModel();
            try
            {
                // Connect to the queue.
                System.Messaging.MessageQueue mq = (System.Messaging.MessageQueue)source;

                // End the asynchronous Receive operation.
                System.Messaging.Message myMessage = mq.EndReceive(asyncResult.AsyncResult);
                resultModel.message = MessageConvert.ConvertFromSystemMessage(myMessage);
                resultModel.Result = (int)ResultsEnum.Done;
                // Restart the asynchronous Receive operation.
                Response response = await initiator.SendRequestAsync(new KubeMQ.SDK.csharp.CommandQuery.LowLevel.Request()
                {
                    Metadata = "RecieveOK",
                    Body = Converter.ToByteArray(resultModel),
                    CacheKey="",
                    CacheTTL=0,
                    Channel=channelToReturnTo,
                    ClientID= clientID,
                    RequestType=RequestType.Query,
                    Timeout = _timeout 
                });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(string.Format("Failed to peek path {0} on ex {1}", channelToReturnTo, ex.Message));
            }
        }

        /// <summary>
        ///Occurs when a message is read without being removed from the queue. This is a
        ///result of the asynchronous operation, System.Messaging.MessageQueue.BeginPeek.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ChannelToReturnTo"></param>
        /// <returns></returns>
        internal ResultModel PeekEvent(string path, string ChannelToReturnTo)
        {
            System.Messaging.MessageQueue MyQueue = new System.Messaging.MessageQueue(path);
            MyQueue.PeekCompleted += new PeekCompletedEventHandler(async (s, a) => await MyPeekCompleted(s, a, ChannelToReturnTo,path));
            OpenPeekQueues.TryAdd(path,MyQueue);
            ResultModel resultModel = new ResultModel();
            resultModel.Result = (int)ResultsEnum.Done;
            return resultModel;
        }

        /// <summary>
        /// Peek Event Handler
        /// </summary>
        /// <param name="source"></param>
        /// <param name="asyncResult"></param>
        /// <param name="ChannelToReturnTo"></param>
        /// <param name="channelPeeked"></param>
        private async Task MyPeekCompleted(Object source,
                    PeekCompletedEventArgs asyncResult, string ChannelToReturnTo,string channelPeeked)
        {
            ResultModel resultModel = new ResultModel();
            try
            {
                // Connect to the queue.
                System.Messaging.MessageQueue mq = (System.Messaging.MessageQueue)source;

                // End the asynchronous peek operation.
                System.Messaging.Message myMessage = mq.EndPeek(asyncResult.AsyncResult);
                resultModel.message = MessageConvert.ConvertFromSystemMessage(myMessage);
                resultModel.Result = (int)ResultsEnum.Done;
                // Restart the asynchronous peek operation.
                Response response = await initiator.SendRequestAsync(new KubeMQ.SDK.csharp.CommandQuery.LowLevel.Request()
                {
                    Metadata = "PeekOK",
                    Body = Converter.ToByteArray(resultModel),
                    CacheKey = "",
                    CacheTTL = 0,
                    Channel = ChannelToReturnTo,
                    ClientID = clientID,
                    RequestType = RequestType.Query
                });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(string.Format("Failed to peek path {0} on ex {1}", channelPeeked, ex.Message));
            }
        }

        #endregion
        #region kubeMQProprty
        private string GetKubeMQAddress()
        {
            // get address from appsettings.json
            string KubeMQAddress = Convert.ToString(_config["KubeMQ:Address"]);

            _logger.LogDebug("'KubeMQAddress' was set to{0}", KubeMQAddress.ToString());

            if (string.IsNullOrEmpty(KubeMQAddress))
            {
                _logger.LogError("Did not find KubeMQ Address");
            }


            return KubeMQAddress;
        }
        private string GetChannelName()
        {
            string Channel = Convert.ToString(_config["KubeMQ:ChannelName"]);

            _logger.LogDebug("'ChannelName' was set to{0}", Channel.ToString());

            if (string.IsNullOrEmpty(Channel))
            {
                _logger.LogError("Did not find ChannelName");
            }


            return Channel;
        }
        #endregion
    }
}