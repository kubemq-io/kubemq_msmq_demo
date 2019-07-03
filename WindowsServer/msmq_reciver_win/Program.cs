using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace msmq_reciver_win
{
    class Program
    {
        static void Main(string[] args)
        {
            // this is an framework example soon to be converted to net core, this will replace msmq_receiver project
            MessageQueue queue = null;
            try
            {
                queue = new MessageQueue(@".\private$\RAQueue", QueueAccessMode.Receive);
                //queue.SetPermissions("Everyone", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
                //queue.Authenticate = false;
                //queue.EncryptionRequired = EncryptionRequired.None;
            }
            catch (Exception ex)
            {

            }

            var message = queue.Receive();
            message.Formatter = new BinaryMessageFormatter();
            Console.WriteLine(message.Body.ToString());

            KubeMQ.SDK.csharp.Events.Channel channel = new KubeMQ.SDK.csharp.Events.Channel(new KubeMQ.SDK.csharp.Events.ChannelParameters
            {
                ChannelName = "ratesstore",
                KubeMQAddress = "192.168.1.189:50000",
                ClientID = "msmq_reciver",
                Store = true
            });
            channel.SendEvent(new KubeMQ.SDK.csharp.Events.Event
            {
                Body = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message.Body)),
                Metadata = "Rate message json encoded in UTF8",
                EventID = message.Id
            });
        }
    }
}
