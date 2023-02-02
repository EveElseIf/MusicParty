FROM bitnami/dotnet-sdk:6
COPY . /src
WORKDIR /src
RUN dotnet build -c release -o /build
WORKDIR /build
ENTRYPOINT [ "dotnet" , "MusicParty.dll" , "--urls" , "http://0.0.0.0:5000" ]