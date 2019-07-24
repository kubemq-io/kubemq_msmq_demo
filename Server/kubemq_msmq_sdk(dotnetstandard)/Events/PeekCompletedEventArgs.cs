using System;
using System.Collections.Generic;
using System.Text;
using KubeMQ.MSMQSDK.Messages;

namespace KubeMQ.MSMQSDK.SDK.csharp.Events
{
    public class PeekCompletedEventArgs: EventArgs
    {
        //
        // Summary:
        //     Gets the message associated with the asynchronous peek operation.
        //
        // Returns:
        //     A System.Messaging.Message that represents the end result of the asynchronous
        //     peek operation.
        //
        // Exceptions:
        //   T:System.Messaging.MessageQueueException:
        //     The System.Messaging.PeekCompletedEventArgs.Message could not be retrieved. The
        //     time-out on the asynchronous operation might have expired.
        public Message Message { get; set; }
        //
        // Summary:
        //     Gets or sets the result of the asynchronous operation requested.
        //
        // Returns:
        //     A System.IAsyncResult that contains the data associated with the peek operation.
        public IAsyncResult AsyncResult { get; set; }
    }
}
