# Deploy

- docker build -t w41203208/test-game-service:v0.3.0 -f ./GameTestServer/Dockerfile .
- docker push w41203208/test-game-service:v0.3.1

## problem will happen when let httpclient use self-signed certificate

- https://github.com/dotnet/runtime/issues/47117

### resolution

- configure openssl.cnf?
- To Research this package [builder.Services.AddAuthentication]
