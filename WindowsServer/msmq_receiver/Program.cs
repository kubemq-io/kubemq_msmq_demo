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
        const string kubemqAddress = "104.47.142.90:50000";
        static void Main(string[] args)
        {
           // this is an framework example soon to be converted to net core, this will replace msmq_receiver project
            MessageQueue queue = null;
            try
            {
                queue = new MessageQueue(@".\private$\raqueue");

            }
            catch (Exception ex)
            {

            }

            MessageQueue sendqueue = null;
            try
            {
                sendqueue = new MessageQueue(@".\private$\receiver");
            }
            catch (Exception ex)
            {

            }


            Task msmqsub = Task.Run(() =>
            {

                KubeMQ.SDK.csharp.Events.Channel channel = new KubeMQ.SDK.csharp.Events.Channel(new KubeMQ.SDK.csharp.Events.ChannelParameters
                {
                    ChannelName = "ratesstore",
                    KubeMQAddress = kubemqAddress,
                    ClientID = "msmq_reciver",
                    Store = true
                });

                queue.ReceiveCompleted += new ReceiveCompletedEventHandler((sender, eventArgs) =>
                {
                    eventArgs.Message.Formatter = new BinaryMessageFormatter();
                    //{typeof(String)});
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
                    queue.BeginReceive();
                });

                queue.BeginReceive();
            });

            Task msmqcmd = Task.Run(() =>
            {
                KubeMQ.SDK.csharp.CommandQuery.Responder responder = new KubeMQ.SDK.csharp.CommandQuery.Responder(kubemqAddress);
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
                    //        Formatter =  new XmlMessageFormatter(new Type[]
                    //{typeof(String)}),
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
