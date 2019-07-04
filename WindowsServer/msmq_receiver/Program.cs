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
        private static string ClientID = Environment.GetEnvironmentVariable("CLIENT") ?? $"pub_Demo_{Environment.MachineName}";
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
                Console.WriteLine($"[Demo][msmqsub] init KubeMQ publish presistance channel  PubChannel:{PubChannel}");

                KubeMQ.SDK.csharp.Events.Channel channel = new KubeMQ.SDK.csharp.Events.Channel(new KubeMQ.SDK.csharp.Events.ChannelParameters
                {
                    ChannelName = PubChannel,                  
                    ClientID = ClientID,
                    Store = true
                });

                /// KubeMQ msmq message handler
                receiveMQ.ReceiveCompleted += new ReceiveCompletedEventHandler((sender, eventArgs) =>
                {
                  
                    
                    System.IO.Stream stream = new System.IO.MemoryStream(eventArgs.Message.BodyStream);
                    StreamReader reader = new StreamReader(stream);
                    string msgBody = reader.ReadToEnd();
                    Console.WriteLine(msgBody);

                    channel.SendEvent(new KubeMQ.SDK.csharp.Events.Event
                    {
                        Body = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(msgBody)),
                        Metadata = "Rate message json encoded in UTF8",
                        EventID = eventArgs.Message.Id
                    });
                    receiveMQ.BeginReceive();
                });

                sendMQ.BeginReceive();
            });




            //start a task for enqueue comand messages to MSMSQ using KubeMQ MSMQ SDK
            Task msmqcmd = Task.Run(() =>
            {
                /// Init a new CommandQuery subscriber on the KubeMQ to receive commands
                Console.WriteLine($"[Demo][msmqcmd] init KubeMQ publish presistance channel  PubChannel:{PubChannel}");

                KubeMQ.SDK.csharp.CommandQuery.Responder responder = new KubeMQ.SDK.csharp.CommandQuery.Responder();
                responder.SubscribeToRequests(new KubeMQ.SDK.csharp.Subscription.SubscribeRequest()
                {
                    SubscribeType = KubeMQ.SDK.csharp.Subscription.SubscribeType.Commands,
                    Channel = CMDChannel,
                    ClientID = ClientID

                }, (KubeMQ.SDK.csharp.CommandQuery.RequestReceive request) =>
                {
                    if (request != null)
                    {
                        string strMsg = string.Empty;
                        object body = Encoding.UTF8.GetString(request.Body);
                        sendMQ.Send(new Message
                        {                   
                            Body = body
                        });

                    }
                    KubeMQ.SDK.csharp.CommandQuery.Response response = new KubeMQ.SDK.csharp.CommandQuery.Response(request)
                    {
                        Body = Encoding.UTF8.GetBytes("o.k"),
                        CacheHit = false,
                        Error = "None",
                        ClientID = ClientID,
                        Executed = true,
                        Metadata = "OK",
                        Timestamp = DateTime.UtcNow,
                    };
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
