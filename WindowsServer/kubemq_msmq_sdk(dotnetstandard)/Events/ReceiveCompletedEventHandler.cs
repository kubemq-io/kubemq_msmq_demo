using System;
using System.Collections.Generic;
using System.Text;

namespace KubeMQ.MSMQSDK.SDK.csharp.Events
{
    //
    // Summary:
    //     Represents the method that will handle the System.Messaging.MessageQueue.ReceiveCompleted
    //     event of a System.Messaging.MessageQueue.
    //
    // Parameters:
    //   sender:
    //     The source of the event, the System.Messaging.MessageQueue.
    //
    //   e:
    //     A System.Messaging.ReceiveCompletedEventArgs that contains the event data.
    public delegate void ReceiveCompletedEventHandler(object sender, ReceiveCompletedEventArgs e);
}
