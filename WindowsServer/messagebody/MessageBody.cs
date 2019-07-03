using System;

namespace messagebody
{
    [Serializable]
    public class MessageBody
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Ask { get; set; }
        public string Bid { get; set; }

        public override string ToString()
        {
            return $"ID:{ID}, Name:{Name}, Ask:{Ask}, Bid:{Bid}";
        }
    }
}
