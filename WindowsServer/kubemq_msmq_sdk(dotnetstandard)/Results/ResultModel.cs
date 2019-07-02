using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using KubeMQ.MSMQSDK.Messages;

namespace KubeMQ.MSMQSDK.Results
{
    [Serializable] 
    public class ResultModel 
    {
        public Message message;
        public Exception exception;
        public object MessageBody;
        public int Result;
    }


    [Serializable]
    public class MessageQueueException: Exception,ISerializable
    {
        private string message;
        string stackTrace;
        IDictionary data;     
        string helpLink;
        string source;

        public MessageQueueException()
        {

        }
        public MessageQueueException(Exception ex):
            base(ex.Message )
        {
            message = ex.Message;
            stackTrace = ex.StackTrace;
            data = ex.Data;
            helpLink = ex.HelpLink;
            source = ex.Source;
        }
   
        protected MessageQueueException(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            Contract.EndContractBlock();

            data = (IDictionary)(info.GetValue("Data", typeof(IDictionary)));
            message = info.GetString("Message");
            helpLink = info.GetString("HelpLink") ?? string.Empty ;       
            stackTrace = info.GetString("StackTrace");                
            source = info.GetString("Source");          
        }

     
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            Contract.EndContractBlock();

            info.AddValue("Message", message, typeof(String));
            info.AddValue("HelpLink", helpLink, typeof(String));
            info.AddValue("StackTrace", stackTrace, typeof(String));
            info.AddValue("Source", source, typeof(String));
            info.AddValue("Data", data, typeof(IDictionary));
    
        }

        public override string HelpLink { get => helpLink; set => helpLink = value; }
        public override string StackTrace => stackTrace;
        public override IDictionary Data => data;
        public override string Source { get => source; set => source = value; }
        public override string Message => message; 
        public MessageQueueException(string name)
         : base(name)
        {

        }
    }
}
