pipeline {
    agent any

    stages {
        stage('clean old'){
            steps{
                sh 'rm -rf bin obj'
            }
        }
        stage('Build') {
            steps {
                sh 'dotnet publish directors-assistant.csproj --configuration Release --os linux'
                archiveArtifacts artifacts: 'bin/Release/net7.0/linux-x64/publish/*'
            }
        }
    }
}