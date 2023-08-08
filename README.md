# Wanin_Test

- docker build -t w41203208/test-game-service:v0.1.8 -f ./GameTestServer/Dockerfile .
- docker push w41203208/test-game-service:v0.1.8

# Test

this is test to use Visual Studio Git 1

## problem will happen when let httpclient use self-signed certificate

- https://github.com/dotnet/runtime/issues/47117

### resolution

- configure openssl.cnf?
- add some setting value in httpclient?
