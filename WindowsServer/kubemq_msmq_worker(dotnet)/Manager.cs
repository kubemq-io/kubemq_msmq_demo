using KubeMQ.SDK.csharp.CommandQuery;
using KubeMQ.SDK.csharp.Subscription;
using KubeMQ.SDK.csharp.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using KubeMQ.MSMQSDK;
using KubeMQ.MSMQSDK.Results;
using KubeMQ.MSMQSDK.Messages;

namespace MSMQWorkerConsole
{
    public class Manager
    {
        private int _KubeMQTimeout;
        private Responder responder;
        private readonly CommonMethods common;
        private readonly IConfiguration _config;
        private ILogger<Manager> _logger;
        private string ChannelName;
        private string GroupName;
        private string clientID;
        public Manager(IConfiguration configuration, ILogger<Manager> logger ,CommonMethods _common) :base()
        {
            _logger = logger;
            _config = configuration;
            common = _common;
            _KubeMQTimeout = GetKubeMQTimeout();
            ChannelName = GetChannelName();
            GroupName = GetKubeMQGroup();
            clientID = $"Client:{ChannelName}";
            responder = new Responder(GetKubeMQAddress());
            SubscribeRequest subscribeRequest = GetSubcribeRequest();
            responder.SubscribeToRequests(subscribeRequest, HandleIncomingRequest);
        }

        private SubscribeRequest GetSubcribeRequest()
        {
            SubscribeRequest subscribeRequest = new SubscribeRequest
            {
                Channel = ChannelName,
                EventsStoreType=EventsStoreType.Undefined,
                Group= GroupName,
                ClientID = $"Client:{ChannelName}",
                SubscribeType=SubscribeType.Queries
            };
            return subscribeRequest;
        }

        /// <summary>
        /// Get request from KubeMQ and run the appropriate method
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Response HandleIncomingRequest(RequestReceive request)
        {
            ResultModel result = new ResultModel();
            KubeMQ.MSMQSDK.Messages.Message MessageBody = null; 
            _logger.LogDebug("Received Message from {0} for: {1} ", request.ReplyChannel, request.Metadata);
            try
            {
                _logger.LogInformation("Started Converting Object to request Message");
                MSMQMeta meta = new MSMQMeta();
                meta = meta.FromString(request.Metadata);
                switch (meta.ActionType)
                {
                    case "Exists":
                        result = common.Exists(meta.Path);
                        break;
                    case "Create":
                        result = common.CreateQueue(meta.Path);
                        break;
                    case "Purge":
                        result = common.PurgeQueue(meta.Path);
                        break;
                    case "Delete":
                        result = common.DeleteQueue(meta.Path);
                        break;
                    case "Send":
                        MessageBody = Converter.FromByteArray(request.Body) as KubeMQ.MSMQSDK.Messages.Message;
                        result = common.SendToQueue(meta, MessageBody);
                        break;
                    case "Peek":
                        result = common.PeekQueue(meta.Path);
                        break;
                    case "RegisterPeek":
                        result =  common.PeekEvent(meta.Path, meta.ChannelToReturn);
                        break;
                    case "UnRegisterPeek":
                        result = common.UnregisterPeek(meta.Path);
                        break;
                    case "BeginPeek":
                        result = common.BeginPeek(meta.Path);
                        break;
                    case "RegisterReceive":
                        result = common.EventReceive(meta.Path, meta.ChannelToReturn);
                        break;
                    case "UnRegisterRecieve":
                        result = common.UnregisterReceive(meta.Path);
                        break;
                    case "BeginReceive":
                        result = common.BeginRecieve(meta.Path);
                        break;
                    case "SendJson":
                        string str = System.Text.Encoding.Default.GetString(request.Body);
                        result = common.SendJsonRequestToQueue(meta, str);
                        break;
                }
                if (result.exception != null)
                {
                    return new Response(request)
                    {
                        Metadata = "Error",
                        Body = Converter.ToByteArray(result),
                        CacheHit = false,
                        ClientID = clientID,
                        Error = result.exception.ToString(),
                        Executed=false
                    };
                }
                else
                {
                    return new Response(request)
                    {
                        Metadata = "Ok",
                        Body = Converter.ToByteArray(result),
                        CacheHit = false,
                        ClientID = clientID,
                        Executed=true,
                        Error="none"
                    };
                }
            }
            catch(ArgumentException ex)
            {
                _logger.LogCritical(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
                return null;
            }
        }


        #region kubeMQProprty
        private string GetChannelName()
        {


            string Channel = Convert.ToString(_config["KubeMQ:ChannelName"]);

            _logger.LogDebug("'ChannelName' was set to{0}", Channel.ToString());

            if (string.IsNullOrEmpty(Channel))
            {
                _logger.LogError("Did not find ChannelName");
            }


            return Channel;
        }

        private string GetKubeMQAddress()
        {


            // get address from appsettings.json
            string KubeMQAddress = Convert.ToString(_config["KubeMQ:Address"]);

            _logger.LogDebug("'KubeMQAddress' was set to{0}", KubeMQAddress.ToString());

            if (string.IsNullOrEmpty(KubeMQAddress))
            {
                _logger.LogError("Did not find KubeMQ Address");
            }


            return KubeMQAddress;
        }

        private int GetKubeMQBufferSize()
        {
            int BufferSize = 0;


            // get address from appsettings.json
            string KubeMQBuffersize = Convert.ToString(_config["KubeMQ:BufferSize"]);

            try
            {
                BufferSize = int.Parse(KubeMQBuffersize);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid _config 'KUBEMQBUFFERSIZE'");
            }

            _logger.LogDebug("'KUBEMQBUFFERSIZE' was set to{0}", KubeMQBuffersize.ToString());

            if (string.IsNullOrEmpty(KubeMQBuffersize))
            {
                _logger.LogError("Did not find KUBEMQBUFFERSIZE");
            }

            return BufferSize;
        }

        private int GetKubeMQTimeout()
        {
            int KubeMQTimeout = 0;

            // get Timeout from appsettings.json
            string strTimeout = _config["KubeMQ:Timeout"];

            if (string.IsNullOrEmpty(strTimeout))
            {
                _logger.LogError("Did not find KUBEMQTIMEOUT");
            }

            try
            {
                KubeMQTimeout = int.Parse(strTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid appsetting 'KUBEMQTIMEOUT'");
            }

            _logger.LogDebug("'KUBEMQTIMEOUT' was set to{0}", KubeMQTimeout.ToString());

            return KubeMQTimeout;

        }

        private string GetKubeMQGroup()
        {
            // get address from appsettings.json
            string GroupName = Convert.ToString(_config["KubeMQ:GroupName"]);


            if (string.IsNullOrEmpty(GroupName))
            {
                _logger.LogError("Did not find KubeMQGroup Name , setting to default of empty string(no Group)");
            }

            _logger.LogDebug("GroupName was set to{0}", GroupName.ToString());

            return GroupName;
        }
        #endregion
    }
}