# kubemq_msmq_demo

Kubemq.msmsq.sdk example use case, the upgrade to netcore when depended on msmq.

## Getting Started

This demo includes 4 main components
* msmq_generator(dotnet) - representing msmq message generator running on windows server
* kubemq_msmq_worker(dotnet) - represents a general brige between KubeMQ and msmq, running on windows server
* kubemq_msmq_receiver - represents an upgraded netcore process communicating with the msmq_generator(dotnet) using kubemq and msmq

### Prerequisites

* Kubernetes environment or docker orchestration 
* Windows server .net  with msmq queues ".\private$\raqueue", ".\private$\receiver"
* KubeMQ running with port forward access to GRPC_PORT and REST_PORT

### Installing

Windows server:
Configure msmq queues ".\private$\raqueue", ".\private$\receiver"
run kubemq_msmq_rates_generator 
configure kubme_msmq_worker appsettings.json and run
```
"KubeMQ": {
    "ChannelName": "MsDemo.KubeMQ.0", //equal to the SDK
    "Timeout": 10000, //RPC timout
    "Address": "104.47.142.90:50000", //KubmeMQ GRPC_PORT 
    "GroupName": ""//fill if more then one in round robing pattern
  }
```
Docker:
Deploy KubeMQ from kubemq.io (https://account.kubemq.io/home/get-kubemq/kubernetes)

Deploy client container kubemq/kubemq_msmq_client
```
docker pull kubemq/kubemq_msmq_client
```
Deploy kubemq_msmq_receiver 
```
docker pull kubemq/msmq_receiver
```

msmq_receiver has some environment variable to communicate with kubme_msmq_worker 
```
 Environment:
      KUBEMQSCHANNEL:       MsDemo.KubeMQ.0
      KUBEMQTIMEOUT:        10000
      KubeMQServerAddress:  104.47.142.90:50000
```



