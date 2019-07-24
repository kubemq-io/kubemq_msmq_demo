using System;
using System.Collections.Generic;
using System.Text;

namespace KubeMQ.MSMQSDK.SDK.csharp.Events
{
    //
    // Summary:
    //     Represents the method that will handle the System.Messaging.MessageQueue.PeekCompleted
    //     event of a System.Messaging.MessageQueue.
    //
    // Parameters:
    //   sender:
    //     The source of the event, the System.Messaging.MessageQueue.
    //
    //   e:
    //     A System.Messaging.PeekCompletedEventArgs that contains the event data.
    public delegate void PeekCompletedEventHandler(object sender, PeekCompletedEventArgs e);
}
