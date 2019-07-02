using System;
using System.Collections.Generic;
using System.Text;
using KubeMQ.MSMQSDK.Messages;

namespace KubeMQ.MSMQSDK.SDK.csharp.Events
{
    //
    // Summary:
    //     Provides data for the System.Messaging.MessageQueue.ReceiveCompleted event. When
    //     your asynchronous receive operation calls an event handler, an instance of this
    //     class is passed to the handler.
    public class ReceiveCompletedEventArgs : EventArgs
    {
        //
        // Summary:
        //     Gets the message associated with the asynchronous receive operation.
        //
        // Returns:
        //     A System.Messaging.Message that represents the end result of the asynchronous
        //     receive operation.
        //
        // Exceptions:
        //   T:System.Messaging.MessageQueueException:
        //     The System.Messaging.ReceiveCompletedEventArgs.Message could not be retrieved.
        //     The time-out on the asynchronous operation might have expired.
        public Message Message { get; set; }
        //
        // Summary:
        //     Gets or sets the result of the asynchronous operation requested.
        //
        // Returns:
        //     A System.IAsyncResult that contains the data associated with the receive operation.
        public IAsyncResult AsyncResult { get; set; }
    }
}
