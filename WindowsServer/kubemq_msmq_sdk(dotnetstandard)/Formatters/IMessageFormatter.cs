using System;
using System.Collections.Generic;
using System.Text;
using KubeMQ.MSMQSDK.Messages;

namespace KubeMQ.MSMQSDK.Messages
{

    public interface IMessageFormatter
    {
        bool CanRead(Message message);
        object Read(Message message);
        void Write(Message message, object obj);
    }
    [Serializable]
    public abstract class MicrosoftFormatter :IMessageFormatter
    {
        public string FormatterName;

        public bool CanRead(Message message)
        {
            throw new NotImplementedException();
        }

        public object Read(Message message)
        {
            throw new NotImplementedException();
        }

        public void Write(Message message, object obj)
        {
            throw new NotImplementedException();
        }
    }

    public class ActiveXMessageFormatter : MicrosoftFormatter
    {
        public ActiveXMessageFormatter()
        {
            FormatterName = "ActiveXMessageFormatter";
        }
    }
}
