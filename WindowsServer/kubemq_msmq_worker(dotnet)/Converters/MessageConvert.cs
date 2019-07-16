using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KubeMQ.MSMQSDK.Messages;
using KubeMQ.MSMQSDK;

namespace MSMQWorkerConsole.Converters
{
    public class MessageConvert
    {
        /// <summary>
        /// Convert SystemMessage.
        /// Input: SystemMessage Out: TMessage
        /// </summary>
        /// <param name="systemMessage"></param>
        /// <returns></returns>
        public static KubeMQ.MSMQSDK.Messages.Message ConvertFromSystemMessage(System.Messaging.Message systemMessage)
        {
            KubeMQ.MSMQSDK.Messages.Message TMessage = new KubeMQ.MSMQSDK.Messages.Message();
            byte[] myBinary = new byte[systemMessage.BodyStream.Length];
            systemMessage.BodyStream.Read(myBinary, 0, (int)systemMessage.BodyStream.Length);
            TMessage.BodyStream = myBinary;
        //  TMessage.Body = systemMessage.Body ?? string.Empty;
            TMessage.Label = systemMessage.Label ?? string.Empty;
            TMessage.Formatter = SetFormatter(systemMessage.Formatter !=null ? systemMessage.Formatter.ToString() : "XmlMessageFormatter");           
            TMessage.Id = systemMessage.Id??"0";
            return TMessage;
        }


        /// <summary>
        /// Convert TMessage.
        /// Input: TMessage Out: System Message
        /// </summary>
        /// <param name="TMessage">kubemq Message</param>
        /// <returns>System.Messaging.Message</returns>
        public static System.Messaging.Message ConvertToSystemMessage(KubeMQ.MSMQSDK.Messages.Message Tmessage)
        {
            string FormatterType = string.Empty;
            System.Messaging.Message InnerMessage = new System.Messaging.Message(Tmessage.Body);
            InnerMessage.Label = Tmessage.Label ?? string.Empty;

            InnerMessage.BodyType = Tmessage.BodyType;

            if (Tmessage.Formatter != null)
            {

                MicrosoftFormatter microsoftFormatter = Tmessage.Formatter as MicrosoftFormatter;
                if (microsoftFormatter != null)
                {

                    FormatterType = microsoftFormatter.FormatterName ?? string.Empty;
                }
            }
            switch (FormatterType)
            {
                case "ActiveXMessageFormatter":
                    InnerMessage.Formatter = new System.Messaging.ActiveXMessageFormatter();
                    break;
                case "BinaryMessageFormatter":
                    InnerMessage.Formatter = new System.Messaging.BinaryMessageFormatter();
                    break;
                default:
                    InnerMessage.Formatter = new System.Messaging.XmlMessageFormatter();
                    break;
            }
            return InnerMessage;
        }

        /// <summary>
        /// Convert string to System.Messaging.Message.
        /// </summary>
        /// <param name="str">the body to insert to queue</param>
        /// <param name="Formmater">The type of the formmater</param>
        /// <returns>System.Messaging.Message</returns>
        public static System.Messaging.Message ConvertStringToSystemMessage(string str, string Formmater,string label="")
        {
            string FormatterType = string.Empty;
            System.Messaging.Message InnerMessage= new System.Messaging.Message(str);
            InnerMessage.Label = label;
            InnerMessage.BodyType = 1;

            switch (Formmater)
            {
                case "ActiveXMessageFormatter":
                    InnerMessage.Formatter = new System.Messaging.ActiveXMessageFormatter();
                    break;
                case "BinaryMessageFormatter":
                    InnerMessage.Formatter = new System.Messaging.BinaryMessageFormatter();
                    break;
                default:
                    InnerMessage.Formatter = new System.Messaging.XmlMessageFormatter();
                    break;
            }
            return InnerMessage;
        }
        /// <summary>
        /// SetFormatterString According to MicrosoftFormatter
        /// Input:FormatName OutPut:MicrosoftFormatter our format struct for Core applictions
        /// </summary>
        /// <param name="FormaterName"></param>
        /// <returns>MicrosoftFormatter</returns>
        private static MicrosoftFormatter SetFormatter(string FormaterName)
        {
            switch (FormaterName)
            {
                case "ActiveXMessageFormatter":
                    return new ActiveXMessageFormatter();
                case "BinaryMessageFormatter":
                    return new BinaryMessageFormatter();
                case "XmlMessageFormatter":
                return new XmlMessageFormatter();
                default:
                    return null;
            }
        }
    }
}
