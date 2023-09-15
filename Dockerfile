ARG DOTNET_VERSION=7.0
FROM mcr.microsoft.com/dotnet/runtime-deps:${DOTNET_VERSION}-jammy as app

RUN apt update && apt install -y ffmpeg locales locales-all
RUN mkdir -p /data /download \
&& chmod 777 /data


FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} as builder
WORKDIR /repo
COPY . .
RUN dotnet publish ./src/AVOne.Server --disable-parallel --configuration Release --output="/avone" --self-contained --use-current-runtime -p:DebugSymbols=false -p:DebugType=none

FROM app
ENV HEALTHCHECK_URL=http://localhost:8099/health
COPY --from=builder /avone /avone
ENV ASPNETCORE_URLS=http://+:8099
ENV LANG en_US.UTF-8
EXPOSE 8099
VOLUME /data /download
WORKDIR /avone
ENTRYPOINT ["./AVOneServer", \
    "--data-dir", "/data", \
    "--ffmpeg", "/usr/bin/ffmpeg"]
HEALTHCHECK --interval=30s --timeout=30s --start-period=10s --retries=3 \
     CMD curl -Lk -fsS "${HEALTHCHECK_URL}" || exit 1
