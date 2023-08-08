@ECHO OFF

set tempImage=w41203208/test-game-service:test

@REM delete docker container
docker ps | grep "%tempImage%" | awk '{print $1}' > image.txt 


for /f "tokens=1" %%a in (image.txt) do (
	if NOT %%a=="" (
		docker rm -f %%a
		docker image rm %tempImage%

	)
  
)

rm image.txt




