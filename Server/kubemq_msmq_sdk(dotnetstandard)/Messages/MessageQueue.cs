using KubeMQ.SDK.csharp.CommandQuery;
using KubeMQ.SDK.csharp.Subscription;
using KubeMQ.SDK.csharp.Tools;
using System;
using System.Linq;
using System.Text;
using KubeMQ.MSMQSDK.SDK.csharp.Events;
using KubeMQ.MSMQSDK.Results;

namespace KubeMQ.MSMQSDK.Messages
{
    public class MessageQueue
    {
        internal Responder responder;
        public MicrosoftFormatter Formatter { get; set; }
        private static KubeMSMQInitiator instance;
        public string Path { get; set; }
        internal string QueueChannelName;
        private static KubeMSMQInitiator _KubeMSMQInitiator
        {
            get
            {
                if(instance ==null)
                {
                    instance = new KubeMSMQInitiator();
                }
                return instance;
            }
        
        }

        #region CTORS
        //
        // Summary:
        //     Initializes a new instance of the System.Messaging.MessageQueue class. After
        //     the default constructor initializes the new instance, you must set the instance's
        //     System.Messaging.MessageQueue.Path property before you can use the instance.
        public MessageQueue()
        {

        }
        // Summary:
        //     Initializes a new instance of the System.Messaging.MessageQueue class that references
        //     the Message Queuing queue at the specified path.
        //
        // Parameters:
        //   path:
        //     The location of the queue referenced by this System.Messaging.MessageQueue.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The System.Messaging.MessageQueue.Path property is not valid, possibly because
        //     it has not been set.
        public MessageQueue(string path)
        {
            Path = path;
        }
        #endregion
        #region Events
        private ReceiveCompletedEventHandler receiveCompleted;
        public event ReceiveCompletedEventHandler ReceiveCompleted
        {
            add
            {
                Guid guid = new Guid(
                   System.Security.Cryptography.SHA256.Create()
                      .ComputeHash(Encoding.UTF8.GetBytes(this.Path)).Take(16).ToArray());
                QueueChannelName = guid.ToString();
                MSMQMeta meta = new MSMQMeta("RegisterReceive", this.Path, "N/A", QueueChannelName);
                try
                {
                    responder = new Responder(_KubeMSMQInitiator.KubeMQ);
                    responder.SubscribeToRequests(GetSubscribeRequest(),HandleIncomingRequest);
                    receiveCompleted -= value;
                    receiveCompleted += value;
                }
                catch (Exception ex)
                {

                    throw ex.GetBaseException();
                }
                ResultModel resultModel = _KubeMSMQInitiator.EventRegister(meta, QueueChannelName);
            }
            remove
            {
                receiveCompleted -= value;
                QueueChannelName = new Guid(this.Path).ToString();
                MSMQMeta meta = new MSMQMeta("UnRegisterReceive", this.Path, "N/A", QueueChannelName);
                ResultModel resultModel = _KubeMSMQInitiator.EventRegister(meta, QueueChannelName);
            }
        }


        private SubscribeRequest GetSubscribeRequest()
        {
            SubscribeRequest subscribeRequest = new SubscribeRequest
            {
                Channel = QueueChannelName,
                ClientID = $"MSMQWorker{Path}",
                EventsStoreType=EventsStoreType.Undefined,
                SubscribeType=SubscribeType.Queries
            };
            return subscribeRequest;
        }

        //
        // Summary:
        //     Occurs when a message is read without being removed from the queue. This is a
        //     result of the asynchronous operation, System.Messaging.MessageQueue.BeginPeek.
        private PeekCompletedEventHandler peekCompleted;
        public event PeekCompletedEventHandler PeekCompleted
        {
            add
            {
                Guid guid = new Guid(
                   System.Security.Cryptography.SHA256.Create()
                      .ComputeHash(Encoding.UTF8.GetBytes(this.Path)).Take(16).ToArray());
                QueueChannelName = guid.ToString();
                MSMQMeta meta = new MSMQMeta("RegisterPeek", this.Path, "N/A", QueueChannelName);
                try
                {
                    responder = new Responder(instance.KubeMQ);
                    responder.SubscribeToRequests(GetSubscribeRequest(),HandleIncomingRequest);
                    peekCompleted -= value;
                    peekCompleted += value;
                }
                catch (Exception ex)
                {

                    throw ex.GetBaseException();
                }
                ResultModel resultModel = _KubeMSMQInitiator.EventRegister(meta, QueueChannelName);
            }
            remove
            {
                QueueChannelName = new Guid(this.Path).ToString();
                peekCompleted -= value;
                MSMQMeta meta = new MSMQMeta("UnRegisterPeek", this.Path, "N/A", QueueChannelName);
                ResultModel resultModel = _KubeMSMQInitiator.EventRegister(meta, QueueChannelName);
            }
        }
        public void BeginPeek()
        {
            try
            {
                MSMQMeta meta = new MSMQMeta("BeginPeek", this.Path, "N/A");
                ResultModel resultModel = _KubeMSMQInitiator.EventRequest(meta);
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }
        public void BeginReceive()
        {
            try
            {
                MSMQMeta meta = new MSMQMeta("BeginReceive", this.Path, "N/A");
                ResultModel resultModel = _KubeMSMQInitiator.EventRequest(meta);
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }
        //
        // Summary:
        //     Occurs when a message has been removed from the queue. This event is raised by
        //     the asynchronous operation, System.Messaging.MessageQueue.BeginReceive.       
        #endregion
        #region Simplecalls
        //
        // Summary:
        //     Sends an object to non-transactional queue referenced by this System.Messaging.MessageQueue.
        //
        // Parameters:
        //   obj:
        //     The object to send to the queue.
        //
        // Exceptions:
        //   T:System.Messaging.MessageQueueException:
        //     The System.Messaging.MessageQueue.Path property has not been set.-or- An error
        //     occurred when accessing a Message Queuing method.
        public void Send(object obj)
        {
            try
            {
                MSMQMeta meta = new MSMQMeta("Send", this.Path, "N/A");
                ResultModel resultModel = _KubeMSMQInitiator.SendWithObj(obj, meta);
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }
        // Summary:
        //     Deletes all the messages contained in the queue.
        //
        // Exceptions:
        //   T:System.Messaging.MessageQueueException:
        //     An error occurred when accessing a Message Queuing method.
        public void Purge()
        {
            try
            {
                MSMQMeta meta = new MSMQMeta("Purge", this.Path, "NA");
                ResultModel resultModel = _KubeMSMQInitiator.SendRequest(meta);
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }
        //
        // Summary:
        //     Deletes a queue on a Message Queuing server.
        //
        // Parameters:
        //   path:
        //     The location of the queue to be deleted.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The path parameter is null or is an empty string ("").
        //
        //   T:System.Messaging.MessageQueueException:
        //     The syntax for the path parameter is not valid.-or- An error occurred when accessing
        //     a Message Queuing method.
        public static void Delete(string path)
        {
            try
            {
                MSMQMeta meta = new MSMQMeta("Delete", path, "N/A");
                ResultModel resultModel = _KubeMSMQInitiator.SendRequest(meta);
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }
        public Message Peek()
        {
            try
            {
                MSMQMeta meta = new MSMQMeta("Peek", this.Path, "N/A");
                ResultModel resultModel = _KubeMSMQInitiator.SendRequest(meta);
                Message message = resultModel.message as Message;
                return message;
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }
        //
        // Summary:
        //     Determines whether a Message Queuing queue exists at the specified path.
        //
        // Parameters:
        //   path:
        //     The location of the queue to find.
        //
        // Returns:
        //     true if a queue with the specified path exists; otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The path syntax is not valid.
        //
        //   T:System.Messaging.MessageQueueException:
        //     An error occurred when accessing a Message Queuing method.-or- The System.Messaging.MessageQueue.Exists(System.String)
        //     method is being called on a remote private queue
        //
        //   T:System.InvalidOperationException:
        //     The application used format name syntax when verifying queue existence.
        public static bool Exists(string path)
        {
            try
            {
                MSMQMeta meta = new MSMQMeta("Exists", path, "N/A");
                ResultModel response = _KubeMSMQInitiator.SendRequest(meta);
                if (response.Result == (int)ResultsEnum.AlreadyExist || response.Result == (int)ResultsEnum.Created)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
            return false;
        }
        //
        // Summary:
        //     Creates a non-transactional Message Queuing queue at the specified path.
        //
        // Parameters:
        //   path:
        //     The path of the queue to create.
        //
        // Returns:
        //     A System.Messaging.MessageQueue that represents the new queue.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The path parameter is null or is an empty string ("").
        //
        //   T:System.Messaging.MessageQueueException:
        //     A queue already exists at the specified path.-or- An error occurred when accessing
        //     a Message Queuing method.
        public static MessageQueue Create(string path)
        {
            try
            {
                MSMQMeta meta = new MSMQMeta("Create", path, "N/A");
                ResultModel response = _KubeMSMQInitiator.SendRequest(meta);
                if (response.Result == (int)ResultsEnum.AlreadyExist)
                {

                }
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
            MessageQueue messageq = new MessageQueue(path);
            return messageq;
        }


        #endregion
        /// <summary>
        /// Receive request from the worker on either :Peek event or Receive event
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Response HandleIncomingRequest(RequestReceive request)
        {
            ResultModel resultModel = Converter.FromByteArray(request.Body) as ResultModel;
            switch (request.Metadata)
            {
                case "PeekOK":
                    PeekCompletedEventArgs PeekArgs = new PeekCompletedEventArgs();
                    PeekArgs.Message = resultModel.message;
                    peekCompleted?.Invoke(this, PeekArgs);
                    break;
                case "RecieveOK":
                    ReceiveCompletedEventArgs receiveCompletedEventArgs = new ReceiveCompletedEventArgs();
                    receiveCompletedEventArgs.Message= resultModel.message;
                    receiveCompleted?.Invoke(this, receiveCompletedEventArgs);
                    break;
                default:
                    break;
            }
            return new Response(request)
            {
                Metadata = "OK",
                Body = Converter.ToByteArray("OK"),
                CacheHit = false,
                ClientID = $"MSMQWorker{Path}",
                Timestamp=DateTime.UtcNow,
                Executed=true,
                Error=""
            };
        }

        #region NotInUseYet
        public IAsyncResult BeginReceive(TimeSpan timeout, object stateObject, AsyncCallback callback) { return null; }
        //
        // Summary:
        //     Initiates an asynchronous receive operation that has a specified time-out and
        //     a specified state object, which provides associated information throughout the
        //     operation's lifetime. The operation is not complete until either a message becomes
        //     available in the queue or the time-out occurs.
        //
        // Parameters:
        //   timeout:
        //     A System.TimeSpan that indicates the interval of time to wait for a message to
        //     become available.
        //
        //   stateObject:
        //     A state object, specified by the application, that contains information associated
        //     with the asynchronous operation.
        //
        // Returns:
        //     The System.IAsyncResult that identifies the posted asynchronous request.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The value specified for the timeout parameter is not valid.
        //
        //   T:System.Messaging.MessageQueueException:
        //     An error occurred when accessing a Message Queuing method.
        public IAsyncResult BeginReceive(TimeSpan timeout, object stateObject) { return null; }
        //
        // Summary:
        //     Initiates an asynchronous receive operation that has a specified time-out. The
        //     operation is not complete until either a message becomes available in the queue
        //     or the time-out occurs.
        //
        // Parameters:
        //   timeout:
        //     A System.TimeSpan that indicates the interval of time to wait for a message to
        //     become available.
        //
        // Returns:
        //     The System.IAsyncResult that identifies the posted asynchronous request.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The value specified for the timeout parameter is not valid, possibly because
        //     it represents a negative number.
        //
        //   T:System.Messaging.MessageQueueException:
        //     An error occurred when accessing a Message Queuing method.
        public IAsyncResult BeginReceive(TimeSpan timeout) { return null; }
        //
        // Summary:
        //     Initiates an asynchronous receive operation that has no time-out. The operation
        //     is not complete until a message becomes available in the queue.
        //
        // Returns:
        //     The System.IAsyncResult that identifies the posted asynchronous request.
        //
        // Exceptions:
        //   T:System.Messaging.MessageQueueException:
        //     An error occurred when accessing a Message Queuing method.

        private void handleAsyncException(AggregateException exception)
        {
            if (exception.InnerException != null)
            {
                throw exception.InnerException;
            }
        }
        #endregion

    }
}
