using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KubeMQ.MSMQSDK.Messages
{
    [Serializable]
    public class Message
    {
        #region MostReleventProprty
        public DateTime? SentTime { get; set; }
        public string Label { get; set; }
        public string Id { get; set; }
        public object Body { get; set; }
        public byte[] BodyStream { get; set; }
        public int BodyType { get; set; }
        public IMessageFormatter Formatter { get; set; }
        #endregion

        #region NotInUseForNow
        public bool UseEncryption { get; set; }
        public bool UseAuthentication { get; set; }
        #endregion

        public Message()
        {

        }
        public Message(object _Body)
        {
            Body = _Body;
        }
        public Message(object body, IMessageFormatter binaryMessageFormatter)
        {
            this.Body = body;
            this.Formatter = binaryMessageFormatter;
        }
      
    }
}
