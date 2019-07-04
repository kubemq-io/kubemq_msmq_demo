using KubeMQ.MSMQSDK.Messages;
using KubeMQ.MSMQSDK.SDK.csharp.Events;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace msmq_receiver
{
    class Program
    {
        /// <summary>
        /// this is the KubeMQ message bus address located on Azure
        /// </summary>
        private static string ClientID = Environment.GetEnvironmentVariable("CLIENT") ?? $"MSMQ_Demo_{Environment.MachineName}";
        private static string PubChannel = Environment.GetEnvironmentVariable("PUBCHANNEL") ?? "ratesstore";      
        private static string CMDChannel = Environment.GetEnvironmentVariable("CMDCHANNEL") ?? "rateCMD";
        private static string RateMQ = Environment.GetEnvironmentVariable("RateMQ") ?? @".\private$\raqueue";
        private static string CMDMQ = Environment.GetEnvironmentVariable("RateMQ") ?? @".\private$\receiver";


        static void Main(string[] args)
        {

            Console.WriteLine("[Demo] Start demo msmq_receiver");
            //KubeMQ address environment var "KubeMQServerAddress" GRPC port
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



            //start a task for dequeue messages from MSMSQ using KubeMQ MSMQ SDK

            Task msmqsub = Task.Run(() =>
            {
                /// Init a new sender channel on the KubeMQ to publish recived rates
          

                KubeMQ.SDK.csharp.Events.Channel channel = new KubeMQ.SDK.csharp.Events.Channel(new KubeMQ.SDK.csharp.Events.ChannelParameters
                {
                    ChannelName = PubChannel,                  
                    ClientID = ClientID,
                    Store = true
                });
                Console.WriteLine($"[Demo][msmqsub] init KubeMQ publish presistance channel  PubChannel:{PubChannel}");

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
                        Console.WriteLine($"[Demo][msmqsub] Msg recived from RateMQ {sender}:{msgBody}");
                    }
                    catch (Exception ex )
                    {
                        Console.WriteLine($"[Demo][msmqsub] Error parse msg from RateMQ {sender}:{ex.Message}");                        
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
                            Console.WriteLine($"[Demo][msmqsub] SendEvent {PubChannel}:{msgBody}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Demo][msmqsub] Error parse msg from RateMQ {sender}:{ex.Message}");
                        }
                    }
                   
                    receiveMQ.BeginReceive();
                });

                receiveMQ.BeginReceive();
            });




            //start a task for enqueue comand messages to MSMSQ using KubeMQ MSMQ SDK
            Task msmqcmd = Task.Run(() =>
            {
                /// Init a new CommandQuery subscriber on the KubeMQ to receive commands
             

                KubeMQ.SDK.csharp.CommandQuery.Responder responder = new KubeMQ.SDK.csharp.CommandQuery.Responder();

                Console.WriteLine($"[Demo][msmqcmd] init KubeMQ CommandQuery subscriber :{CMDChannel}");

                responder.SubscribeToRequests(new KubeMQ.SDK.csharp.Subscription.SubscribeRequest()
                {
                    SubscribeType = KubeMQ.SDK.csharp.Subscription.SubscribeType.Commands,
                    Channel = CMDChannel,
                    ClientID = ClientID

                }, (KubeMQ.SDK.csharp.CommandQuery.RequestReceive request) =>
                {
                     Console.WriteLine($"[Demo][msmqcmd] CommandQuery RequestReceive :{request}");
                    KubeMQ.SDK.csharp.CommandQuery.Response response;
                    if (request != null)
                    {
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
                            Console.WriteLine($"[Demo][msmqcmd] Error CommandQuery send response :{ex.Message}");
                            response = new KubeMQ.SDK.csharp.CommandQuery.Response(request)
                            {
                                Body = Encoding.UTF8.GetBytes(ex.Message),
                                CacheHit = false,
                                Error = $"Recived error from KubeMQ.MSMQ {ex.Message}",
                                ClientID = ClientID,
                                Executed = false,
                                Metadata = "Bad",
                                Timestamp = DateTime.UtcNow
                                
                            };
                            Console.WriteLine($"[Demo][msmqcmd] CommandQuery send response :{response}");
                            return response;
                        }                      
                    }

                    response = new KubeMQ.SDK.csharp.CommandQuery.Response(request)
                    {
                        Body = Encoding.UTF8.GetBytes("o.k"),
                        CacheHit = false,
                        Error = (request != null) ?"None": "Request received was null",
                        ClientID = ClientID,
                        Executed = (request != null),
                        Metadata = (request != null) ? "OK" : "Bad",
                        Timestamp = DateTime.UtcNow,
                    };
                    Console.WriteLine($"[Demo][msmqcmd] CommandQuery send response :{response}");
                    return response;
                });

            });
            Console.WriteLine("[Demo] press Ctrl+c to stop");

            System.Threading.AutoResetEvent waitHandle = new System.Threading.AutoResetEvent(false);
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {

                e.Cancel = true;
                waitHandle.Set();

            };

            waitHandle.WaitOne();

        }
    }
}
