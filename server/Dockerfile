# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:5.0-focal

WORKDIR /app
COPY . .

RUN apt update && apt install -y libatomic1

ENTRYPOINT ["/app/start_server.sh"]