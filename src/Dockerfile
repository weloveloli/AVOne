ARG DOTNET_VERSION=7.0

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as app

ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y ffmpeg libbrotli1 libmbedtls12 
RUN mkdir -p /data /media /download \
&& chmod 777 /data


FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} as builder
WORKDIR /repo
COPY . .
RUN dotnet publish AVOne.Server --disable-parallel --configuration Release --output="/avone" -p:DebugSymbols=false -p:DebugType=none
ENV LC_ALL en_US.UTF-8
ENV LANG en_US.UTF-8
ENV LANGUAGE en_US:en

FROM app
ENV HEALTHCHECK_URL=http://localhost:8099/health
COPY --from=builder /avone /avone
ENV ASPNETCORE_URLS=http://+:8099
EXPOSE 8099
VOLUME /data /media /download
WORKDIR /avone
ENTRYPOINT ["dotnet","./avone.dll", \
    "--data-dir", "/data", \
    "--ffmpeg", "/usr/bin/ffmpeg"]
HEALTHCHECK --interval=30s --timeout=30s --start-period=10s --retries=3 \
     CMD curl -Lk -fsS "${HEALTHCHECK_URL}" || exit 1