using System;
using Microsoft.Extensions.Logging;
using KubeMQ.MSMQSDK.Results;
using KubeMQ.SDK.csharp.CommandQuery;
using KubeMQ.SDK.csharp.Tools;

namespace KubeMQ.MSMQSDK
{
    public class KubeMSMQInitiator
    {
        public Channel requestChannel;
        internal string KubeMQ;
        internal string KubeMQChannel { get; }
        internal int KubeMQTimeout;
        private ILogger _logger;
        public KubeMSMQInitiator()
        {
            InitLogger();
            KubeMQChannel = GetKubeMSMQChannel();
            KubeMQTimeout = GetKubeMQTimeOut();
            KubeMQ = GetKubeMQAddress();
            requestChannel = new Channel(GetChannelParam());
        }

        private ChannelParameters GetChannelParam()
        {
            ChannelParameters requestChannel = new ChannelParameters
            {
                ChannelName = KubeMQChannel,
                ClientID = $"Client:{KubeMQChannel}",
                KubeMQAddress = KubeMQ,
                Logger = _logger,
                Timeout = GetKubeMQTimeOut(),
                CacheKey="",
                CacheTTL=0,
                RequestsType=RequestType.Query

            };
            return requestChannel;
        }

        internal ResultModel SendRequest(MSMQMeta Metadata)
        {
            Request request = CreateRequest(Metadata.ActionType, Metadata);
            Response response = requestChannel.SendRequest(request);
            ResultModel resultModel = Converter.FromByteArray(response.Body) as ResultModel;
            if (resultModel.exception != null)
            {
                throw resultModel.exception;
            }
            return resultModel;
        }


        internal ResultModel SendWithObj(object obj, MSMQMeta Metadata)
        {
            Request request = CreateRequest(obj, Metadata);

            Response response = requestChannel.SendRequest(request);
            ResultModel resultModel = Converter.FromByteArray(response.Body) as ResultModel;
            if(resultModel.exception!= null)
            {
                throw resultModel.exception;
            }
            return resultModel;
        }



        internal ResultModel EventRegister(MSMQMeta meta,string ChannelName)
        {
            Request request = CreateRequest(meta.ActionType, meta);
            Response response = requestChannel.SendRequest(request);
            ResultModel resultModel = Converter.FromByteArray(response.Body) as ResultModel;
            if (resultModel.exception != null)
            {
                throw resultModel.exception;
            }
            return resultModel;
        }

        internal ResultModel EventRequest(MSMQMeta meta)
        {
            Request request = CreateRequest(meta.ActionType, meta);
            Response response = requestChannel.SendRequest(request);
            ResultModel resultModel = Converter.FromByteArray(response.Body) as ResultModel;
            if (resultModel.exception != null)
            {
                throw resultModel.exception;
            }
            return resultModel;
        }


        private Request CreateRequest(object objbody, MSMQMeta Metadata)
        {
            Request request = new Request()
            {
                Metadata = Metadata.ToString(),
                Body = Converter.ToByteArray(objbody),

            };
            return request;
        }


        #region KubeAttributes
        private  string GetKubeMQAddress()
        {
            // Get environment variable 'KUBEMQSADDRESS' from configuration 
            string KubeMQAddress = Environment.GetEnvironmentVariable("KUBEMQSADDRESS");

            if (string.IsNullOrEmpty(KubeMQAddress))
            {
                _logger.LogWarning("Did not find environment variable 'KubeMQAddress'.");
            }

            _logger.LogDebug("'KubeMQAddress' was set to{0}", KubeMQAddress.ToString());

            return KubeMQAddress;
        }

        private  string GetKubeMSMQChannel()
        {
            // Get environment variable 'KUBEMQSADDRESS' from configuration
            string KubeMQChannel = Environment.GetEnvironmentVariable("KUBEMQSCHANNEL");

            if (string.IsNullOrEmpty(KubeMQChannel))
            {
                _logger.LogWarning("Did not find environment variable 'KubeMQChannel'.");
            }

            _logger.LogDebug("'KubeMQchannel' was set to{0}", KubeMQChannel.ToString());

            return KubeMQChannel;
        }


        private  int GetKubeMQTimeOut()
        {

            int _KubeMQTimeOut = 0;
            // Get environment variable 'KUBEMQBUFFERSIZE' from configuration
            string KubeMQTimeOut = Environment.GetEnvironmentVariable("KUBEMQTIMEOUT");

            if (string.IsNullOrEmpty(KubeMQTimeOut))
            {
                _logger.LogWarning("Did not find environment variable 'KUBEMQTIMEOUT'. ");
            }

            try
            {
                _KubeMQTimeOut = int.Parse(KubeMQTimeOut);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid environment variable 'KUBEMQTIMEOUT'");
            }

            _logger.LogDebug("'KUBEMQBUFFERSIZE' was set to{0}", _KubeMQTimeOut.ToString());

            return _KubeMQTimeOut;
        }

        private  void InitLogger()
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            _logger = loggerFactory.CreateLogger("Sender");
        }
        #endregion
    }
}
