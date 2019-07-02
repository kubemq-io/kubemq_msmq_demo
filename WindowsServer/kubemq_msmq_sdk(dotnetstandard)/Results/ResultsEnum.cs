using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeMQ.MSMQSDK.Results
{/// <summary>
/// Enum for results from system.messaging
/// </summary>
    public enum ResultsEnum
    {
        Empty,
        Created,
        AlreadyExist,
        Deleted,
        QueuePurged,
        AddedToQueue,
        Done,
        Error,
        ChannelDosntExist,
        BeginPeek,
        EndPeek,
        BeginReceive,
        NotExist = -1,
    }
}
