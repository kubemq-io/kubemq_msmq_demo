using KubeMQ.MSMQSDK.Messages;
using KubeMQ.MSMQSDK.SDK.csharp.Events;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace msmq_receiver
{
    /// <summary>
    /// KubeMQ MSMQ Receiver was migrated from framework to net core to be containerized.
    /// MSMQ functionality is achieved by KubeMQ MSMQ worker and MSMQ SDK.
    /// KubeMQ MSMQ receiver will publish messages dequeued by KubeMQ_MSMQ_worker, will handle requests by enqueue messages by KubeMQ_MSMQ_worker. 
    ///  
    /// </summary>
    class Program
    {
        /// <summary>
        /// KubeMQ ClientID for tracing and persistency tracking.
        /// </summary>
        private static string ClientID = Environment.GetEnvironmentVariable("CLIENT") ?? $"MSMQ_Demo_{Environment.MachineName}";
        /// <summary>
        /// KubeMQ persistent channel to publish rates.
        /// </summary>
        private static string PubChannel = Environment.GetEnvironmentVariable("PUBCHANNEL") ?? "ratesstore";
        /// <summary>
        /// KubeMQ Command Chanel subscriber for handling  command request.
        /// </summary>
        private static string CMDChannel = Environment.GetEnvironmentVariable("CMDCHANNEL") ?? "rateCMD";
        /// <summary>
        /// MSMQ queues for legacy code handling.
        /// dequeue RateMQ/ enqueue CMDMQ
        /// </summary>
        private static string RateMQ = Environment.GetEnvironmentVariable("RateMQ") ?? @".\private$\raqueue";
        private static string CMDMQ = Environment.GetEnvironmentVariable("CMDMQ") ?? @".\private$\receiver";


        static void Main(string[] args)
        {

            Console.WriteLine("[Demo] Start demo msmq_receiver");
            //KubeMQ address is environment var "KubeMQServerAddress" GRPC port
            Console.WriteLine($"[Demo] KUBEMQ GRPC address:{Environment.GetEnvironmentVariable("KubeMQServerAddress")}");
            Console.WriteLine($"[Demo] ClientID:{ClientID}");
            Console.WriteLine($"[Demo] Publish rates channel PUBCHANNEL:{PubChannel}");
            Console.WriteLine($"[Demo] Command channel CMDChannel:{CMDChannel}");


            Console.WriteLine($"[Demo] KubeMQ MSMQ SDK message timeout KUBEMQTIMEOUT:{Environment.GetEnvironmentVariable("KUBEMQTIMEOUT")}");
            Console.WriteLine($"[Demo] KubeMQ MSMQ SDK message channel:{Environment.GetEnvironmentVariable("KUBEMQSCHANNEL")}");



            Console.WriteLine($"[Demo] init KubeMQ MessageQueue RateMQ:{RateMQ}");
            MessageQueue receiveMQ = new MessageQueue(RateMQ);
            Console.WriteLine($"[Demo] init KubeMQ MessageQueue CMDMQs:{CMDMQ}");
            MessageQueue sendMQ =new MessageQueue(CMDMQ);


            System.Threading.CancellationTokenSource source = new System.Threading.CancellationTokenSource();
            System.Threading.CancellationToken token = source.Token ;

            //start a task for dequeue messages from MSMSQ using KubeMQ MSMQ SDK
            //DequeueAndEventPub task implementing KubeMQ.MSMQ.SDK will request a Dequeue from KubeMQ MSMQ Worker publish the message to persistent KubeMQ channel.
            Task DequeueAndEventPub = Task.Run(() =>
            {
                /// Init a new sender channel on the KubeMQ to publish received rates
          

                KubeMQ.SDK.csharp.Events.Channel channel = new KubeMQ.SDK.csharp.Events.Channel(new KubeMQ.SDK.csharp.Events.ChannelParameters
                {
                    ChannelName = PubChannel,                  
                    ClientID = ClientID,
                    Store = true
                });
                Console.WriteLine($"[Demo][DequeueAndEventPub] init KubeMQ publish persistence channel  PubChannel:{PubChannel}");

                /// KubeMQ msmq message handler
                receiveMQ.ReceiveCompleted += new ReceiveCompletedEventHandler((sender, eventArgs) =>
                {                    
                    eventArgs.Message.Formatter = new BinaryMessageFormatter();
                   
                    System.IO.Stream stream = new System.IO.MemoryStream(eventArgs.Message.BodyStream);
                    StreamReader reader = new StreamReader(stream);
                    string msgBody = null;
                    try
                    {
                        msgBody = reader.ReadToEnd();
                        Console.WriteLine($"[Demo][DequeueAndEventPub] Msg received from RateMQ {sender}:{msgBody}");
                    }
                    catch (Exception ex )
                    {
                        Console.WriteLine($"[Demo][DequeueAndEventPub] Error parse msg from RateMQ {sender}:{ex.Message}");                        
                    }

                    if (msgBody != null)
                    {
                        try
                        {
                            channel.SendEvent(new KubeMQ.SDK.csharp.Events.Event
                            {
                                Body = Encoding.UTF8.GetBytes(msgBody),
                                Metadata = "Rate message json encoded in UTF8",
                                EventID = eventArgs.Message.Id
                            });
                            Console.WriteLine($"[Demo][DequeueAndEventPub] SendEvent {PubChannel}:{msgBody}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Demo][DequeueAndEventPub] Error parse msg from RateMQ {sender}:{ex.Message}");
                        }
                    }
                   
                    receiveMQ.BeginReceive();
                });

                receiveMQ.BeginReceive();
            }, token);




            //start a task for enqueue command messages to MSMSQ using KubeMQ MSMQ SDK
            Task CommandHanleAndEnqueue = Task.Run(() =>
            {
                /// Init a new CommandQuery subscriber on the KubeMQ to receive commands
             

                KubeMQ.SDK.csharp.CommandQuery.Responder responder = new KubeMQ.SDK.csharp.CommandQuery.Responder();

                Console.WriteLine($"[Demo][CommandHanleAndEnqueue] init KubeMQ CommandQuery subscriber :{CMDChannel}");

                responder.SubscribeToRequests(new KubeMQ.SDK.csharp.Subscription.SubscribeRequest()
                {
                    SubscribeType = KubeMQ.SDK.csharp.Subscription.SubscribeType.Commands,
                    Channel = CMDChannel,
                    ClientID = ClientID

                }, (KubeMQ.SDK.csharp.CommandQuery.RequestReceive request) =>
                {
                    Console.WriteLine($"[Demo][CommandHanleAndEnqueue] CommandQuery RequestReceive :{request}");
                    KubeMQ.SDK.csharp.CommandQuery.Response response;
          
                    string strMsg = string.Empty;
                    object body = Encoding.UTF8.GetString(request.Body);
                    try
                    {
                        sendMQ.Send(new Message
                        {
                            Body = body
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Demo][CommandHanleAndEnqueue] Error CommandQuery send response :{ex.Message}");
                        response = new KubeMQ.SDK.csharp.CommandQuery.Response(request)
                        {
                            Body = Encoding.UTF8.GetBytes(ex.Message),
                            Error = $"Received error from KubeMQ.MSMQ {ex.Message}",
                            ClientID = ClientID,
                            Metadata = "Bad",
                            Timestamp = DateTime.UtcNow

                        };
                        Console.WriteLine($"[Demo][CommandHanleAndEnqueue] CommandQuery send response :{response}");
                        return response;
                    }

                    response = new KubeMQ.SDK.csharp.CommandQuery.Response(request)
                    {
                        Body = Encoding.UTF8.GetBytes("o.k"),
                        Error = "None",
                        ClientID = ClientID,
                        Executed = true,
                        Metadata = "OK",
                        Timestamp = DateTime.UtcNow,
                    };
                    Console.WriteLine($"[Demo][CommandHanleAndEnqueue] CommandQuery send response :{response}");
                    return response;
                });

            }, token);
            Console.WriteLine("[Demo] press Ctrl+c to stop");

            System.Threading.AutoResetEvent waitHandle = new System.Threading.AutoResetEvent(false);
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {

                e.Cancel = true;
                waitHandle.Set();

            };

            waitHandle.WaitOne();
            source.Cancel();

        }
    }
}
