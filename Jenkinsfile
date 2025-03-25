pipeline {
    agent any
    environment {
        linuxServiceAccount=credentials("a83b97d0-dbc6-42d9-96c9-f07a7f2dfab5")
        linuxServiceAccountID="3ca1be00-3d9f-42a1-bab2-48a4d7b99fb0"
        database_connectionString=credentials("7ab58922-c647-42e5-ae15-84faa0c1ee7d")
        targetHost="alloces.lan"
    }
    stages {
        stage('clean old'){
            steps{
                sh 'rm -rf bin obj'
            }
        }
        stage('Build') {
            steps {
                sh 'dotnet publish vassago.csproj --configuration Release --os linux'
                archiveArtifacts artifacts: 'bin/Release/net9.0/linux-x64/publish/*'
            }
        }
        stage ('upload') {
            when {
                //now my CI/CD is no longer continuous, it's just... automatic.
                //(which is what I actually want tbh)
                //but that does mean I have to put this condition in every single branch
                branch "release"
            }
            steps{
                withCredentials([sshUserPrivateKey(credentialsId: env.linuxServiceAccountID, keyFileVariable: 'PK')])
                {
                    sh """#!/bin/bash
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'rm -rf temp_deploy & mkdir -p temp_deploy'
                        scp -i \"${PK}\" -r dist ${linuxServiceAccount_USR}@${env.targetHost}:temp_deploy
                    """
                }
            }
        }
        stage ('stop')
        {
            when {
                branch "release"
            }
            steps{
                withCredentials([sshUserPrivateKey(credentialsId: env.linuxServiceAccountID, keyFileVariable: 'PK')])
                {
                    sh """#!/bin/bash
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'systemctl --user stop test274'
                    """
                }
            }
        }
        stage ('update db')
        {
            when {
                branch "release"
            }
            steps{
                //TODO: backup database
                sh """#!/bin/bash
                """
                
                sh """#!/bin/bash
                    dotnet ef database update --connection "${env.database_connectionString}"
                """
                //TODO: if updating the db fails, restore the old one
                sh """#!/bin/bash
                """
            }
        }
        stage ('replace')
        {
            when {
                branch "release"
            }
            steps{
                withCredentials([sshUserPrivateKey(credentialsId: env.linuxServiceAccountID, keyFileVariable: 'PK')])
                {
                    sh """#!/bin/bash
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'mv dist/appsettings.json appsettings.json'
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'rm -rf dist/ && mv temp_deploy/ dist/'
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'mv appsettings.json dist/appsettings.json'
                    """
                }
            }
        }
        stage ('spin up')
        {
            when {
                branch "release"
            }
            steps{
                withCredentials([sshUserPrivateKey(credentialsId: env.linuxServiceAccountID, keyFileVariable: 'PK')])
                {
                    sh """#!/bin/bash
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'systemctl --user start test274'
                    """
                }
            }
        }

    }
}
