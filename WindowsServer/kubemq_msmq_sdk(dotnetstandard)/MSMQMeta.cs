using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KubeMQ.MSMQSDK
{
    public class MSMQMeta
    {
        public string ActionType { get; set; }
        public string Path { get; set; }
        public string Label { get; set; }
        public string ChannelToReturn { get; set; }
        public MSMQMeta()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_actionType">Action to preform</param>
        /// <param name="_path">MSMQ Path address</param>
        /// <param name="_label">label can be null</param>
        /// <param name="_channelToReturn">what channel to return to</param>
        public MSMQMeta(string _actionType,string _path,string _label,string _channelToReturn = "N/A")
        {
            ActionType = _actionType;
            Path = _path;
            Label = _label;
            ChannelToReturn = _channelToReturn;
        }
        /// <summary>
        /// in : MSMQMeta
        /// Out: String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"ActionType:{ActionType};Path:{Path};Label:{Label??"N/A"};ChannelToReturn:{ChannelToReturn??"N/A"}";
        }

        /// <summary>
        /// In:String
        /// Out:MSMQMeta
        /// </summary>
        /// <param name="MSMQMeta"></param>
        /// <returns></returns>
        public MSMQMeta FromString(string MSMQMeta)
        {
            MSMQMeta mSMQMeta = new MSMQMeta();
            string[] split = MSMQMeta.Split(';');
            foreach (var item in split)
            {

                var actionspplit = item.Split(':');
                switch (actionspplit[0])
                {
                    case "ActionType":
                        mSMQMeta.ActionType = item.Replace("ActionType:", "");
                        break;
                    case
                        "Path":
                        mSMQMeta.Path = item.Replace("Path:", "");
                        break;
                    case "Label":
                        mSMQMeta.Label = item.Replace("Label:", "");
                        break;
                    case "ChannelToReturn":
                        mSMQMeta.ChannelToReturn = item.Replace("ChannelToReturn:", "");
                        break;
                    default:
                        break;
                }
            }
            return mSMQMeta;
        }
    }
}
