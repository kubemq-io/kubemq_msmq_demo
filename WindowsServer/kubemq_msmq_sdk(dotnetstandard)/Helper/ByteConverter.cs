using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KubeMQ.MSMQSDK.Messages;

namespace KubeMQ.MSMQSDK.SDK.csharp.Helper
{
   public class BodyStreamConverter
    {
       public static byte[] FromBodyStream(Stream stream)
        {
            byte[] myBinary = new byte[stream.Length];
            stream.Read(myBinary, 0, (int)stream.Length);
            return myBinary;
        }

        public static Stream ToBodyStream(byte[] byteArray)
        {
            return new System.IO.MemoryStream(byteArray);               
        }

        public object ConvertToObject(byte[] byteArray, IMessageFormatter formatter )
        {
            return null;

        }
    }
}
