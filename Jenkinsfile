pipeline {
  agent { node { label "Main" } }
  environment { 
    IMAGE_NANE = 'w41203208/test-game-service'
    IMAGE_VERSION = 'v0.3.6'
    DOCKERFILE_PATH = './GameTestServer/Dockerfile'
  }
  tools {
    nodejs "NodeJS-21.2.0"
    dockerTool "Docker-default"
  }
  stages {
    stage('Build') {
      steps {
        echo "---------- Build docker image -----------"
        sh ("docker build -t ${IMAGE_NANE}:${IMAGE_VERSION} -f ${DOCKERFILE_PATH} .")
      }
    }
    stage('Push') {
      steps {
        echo "---------- Push docker image -----------"
        sh ("docker push ${IMAGE_NANE}:${IMAGE_VERSION}")
      }
    }
    stage('Deploy') {
      steps {
        withEnv(["BRANCH_TYPE=main"]) {
          echo "----------- BRANCH_TYPE=${BRANCH_TYPE} -----------"
          echo "Execute ${BRANCH_TYPE}"
        }
        sshPublisher(publishers: [sshPublisherDesc(configName: 'w41203208_wanin@webrtc', transfers: [sshTransfer(cleanRemote: false, excludes: '', execCommand: '''cd test/test-dotnet-server 
        sudo echo "${IMAGE_NANE}:${IMAGE_VERSION}"
        sudo export IMAGE_NAME=${IMAGE_NANE}:${IMAGE_VERSION}
        sudo sed -i 's/${DOCKER_IMAGE}/'"$IMAGE_NAME"'/g' ./docker-compose.pro.yml
        sudo docker compose -f docker-compose.pro.yml -f docker-compose.override.pro.yml up -d
        ''', execTimeout: 120000, flatten: false, makeEmptyDirs: false, noDefaultExcludes: false, patternSeparator: '[, ]+', remoteDirectory: 'test/test-dotnet-server', remoteDirectorySDF: false, removePrefix: '', sourceFiles: '*.yml')], usePromotionTimestamp: false, useWorkspaceInPromotion: false, verbose: false)])
      }
    }
  }
}