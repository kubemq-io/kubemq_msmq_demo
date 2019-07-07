FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

COPY . .
RUN dotnet restore kubemq_msmq_receiver.csproj
WORKDIR /src
RUN dotnet build kubemq_msmq_receiver.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish kubemq_msmq_receiver.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "kubemq_msmq_receiver.dll"]
