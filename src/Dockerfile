ARG DOTNET_VERSION=7.0

FROM mcr.microsoft.com/dotnet/runtime-deps:${DOTNET_VERSION}-jammy as app

RUN apt update && apt install ffmpeg
RUN mkdir -p /data /media /download \
&& chmod 777 /data


FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} as builder
WORKDIR /repo
COPY . .
RUN dotnet publish AVOne.Server --disable-parallel --configuration Release --output="/avone" --self-contained --use-current-runtime -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugSymbols=false -p:DebugType=none

FROM app
ENV HEALTHCHECK_URL=http://localhost:8099/health
COPY --from=builder /avone /avone
ENV ASPNETCORE_URLS=http://+:8099
EXPOSE 8099
VOLUME /data /media /download
WORKDIR /avone
ENTRYPOINT ["./avone", \
    "--data-dir", "/data", \
    "--ffmpeg", "/usr/bin/ffmpeg"]
HEALTHCHECK --interval=30s --timeout=30s --start-period=10s --retries=3 \
     CMD curl -Lk -fsS "${HEALTHCHECK_URL}" || exit 1