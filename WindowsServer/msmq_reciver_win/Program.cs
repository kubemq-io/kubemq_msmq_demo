
using System;
using System.Messaging;

using System.Text;
using System.Threading.Tasks;

namespace msmq_reciver_win
{
    class Program
    {   
        static void Main(string[] args)
        {
         //   new KubeMQ.MSMQSDK.KubeMSMQInitiator();
            // this is an framework example soon to be converted to net core, this will replace msmq_receiver project
            MessageQueue queue = null;
            try
            {
                queue = new MessageQueue(@".\private$\RAQueue");

            }
            catch (Exception ex)
            {

            }

            MessageQueue sendqueue = null;
            try
            {
                sendqueue = new MessageQueue(@".\private$\q1");
            }
            catch (Exception ex)
            {

            }


            Task msmqsub = Task.Run(() =>
            {

                KubeMQ.SDK.csharp.Events.Channel channel = new KubeMQ.SDK.csharp.Events.Channel(new KubeMQ.SDK.csharp.Events.ChannelParameters
                {
                    ChannelName = "ratesstore",
                    KubeMQAddress = "104.47.142.90:50000",
                    ClientID = "msmq_reciver",
                    Store = true
                });

                queue.ReceiveCompleted += new ReceiveCompletedEventHandler((sender, eventArgs) =>
                {
                    eventArgs.Message.Formatter = new BinaryMessageFormatter();
                    Console.WriteLine(eventArgs.Message.Body.ToString());

                    channel.SendEvent(new KubeMQ.SDK.csharp.Events.Event
                    {
                        Body = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(eventArgs.Message.Body)),
                        Metadata = "Rate message json encoded in UTF8",
                        EventID = eventArgs.Message.Id
                    });
                    queue.BeginReceive();
                });
                queue.BeginReceive();
            });

            Task msmqcmd = Task.Run(() =>
            {
                KubeMQ.SDK.csharp.CommandQuery.Responder responder = new KubeMQ.SDK.csharp.CommandQuery.Responder("192.168.1.189:50000");
                responder.SubscribeToRequests(new KubeMQ.SDK.csharp.Subscription.SubscribeRequest()
                {
                    SubscribeType = KubeMQ.SDK.csharp.Subscription.SubscribeType.Commands,
                    Channel = "rateCMD",
                    ClientID = "msmq_reciver"

                }, (KubeMQ.SDK.csharp.CommandQuery.RequestReceive request) =>
                {
                    if (request != null)
                    {
                        string strMsg = string.Empty;
                        object body = Encoding.UTF8.GetString(request.Body);
                        sendqueue.Send(new Message
                        {
                            Formatter = new BinaryMessageFormatter(),
                            Body = body
                        });

                    }
                    KubeMQ.SDK.csharp.CommandQuery.Response response = new KubeMQ.SDK.csharp.CommandQuery.Response(request)
                    {
                        Body = Encoding.UTF8.GetBytes("o.k"),
                        CacheHit = false,
                        Error = "None",
                        ClientID = "msmq_reciver",
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
