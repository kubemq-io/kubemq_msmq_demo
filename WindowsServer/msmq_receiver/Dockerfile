FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

COPY . .
RUN dotnet restore msmq_receiver.csproj
WORKDIR /src
RUN dotnet build msmq_receiver.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish msmq_receiver.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "msmq_receiver.dll"]
