pipeline {
    agent any
    environment {
        linuxServiceAccount=credentials("a83b97d0-dbc6-42d9-96c9-f07a7f2dfab5")
        linuxServiceAccountID="3ca1be00-3d9f-42a1-bab2-48a4d7b99fb0"
        database_password_prod=credentials("7ab58922-c647-42e5-ae15-84faa0c1ee7d")
        database_password_test=credentials("7ab58922-c647-42e5-ae15-84faa0c1ee7d")
        targetHost="alloces.lan"
    }
    stages {
         stage("environment setup") { //my environment, here on the jenkins agent. as opposed to the service's environment.
            steps {
                script {
                    sh """#!/bin/bash
                        function testcmd(){
                            if ! command -v \$1 2>&1 >/dev/null
                            then
                                echo "this agent doesn't have \$1"
                                exit 1
                            fi    
                        }

                        testcmd bash 
                        testcmd mktemp
                        testcmd curl
                        testcmd git
                        testcmd rsync
                        testcmd sed
                        testcmd ssh
                        testcmd ssh-keyscan
                        testcmd ssh-keygen
                        testcmd scp
                        testcmd dotnet
                        testcmd make

                        dotnet tool install --local dotnet-ef
                    """
                }
            }
        }
        stage('clean old'){
            steps{
                sh "make clean configuration=Release databasename=vassago pw_database=$database_password_prod"
                sh 'rm -rf dist'
            }
        }
        stage('Build') {
            steps {
                sh '''#!/bin/bash
                    if ! make build configuration=Release databasename=vassago pw_database=${database_password_prod}
                    then
                        echo "build fail"
                        exit 1
                    fi
                '''
            }
        }
        stage('Test') {
            steps{
                sh '''#!/bin/bash
                # the irony of saying "database is _test" and "password is _prod" on the same line...
                # whatever, 2 separate dbs. and they'd both be stored in jenkins, triggered by push, i.e., compromise 1 you've gotten both anyway, whooooo caaaaaares
                echo "db-setuptest, with ${database_password_prod}. i refuse to believe that passing to make clears variables."
                    if ! make db-setuptest databasename=vassago pw_database=${database_password_prod}
                    then
                        echo "fail setting up test db"
                        exit 1
                    fi

                    if ! bash -c make test configuration=Release databasename=vassago pw_database=${database_password_prod}
                    then
                        echo "fail running tests"
                        exit 1
                    fi
                '''

                archiveArtifacts artifacts: 'TestResults/testsresults.html'
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
                    archiveArtifacts artifacts: 'dist/*'
                    sh """#!/bin/bash
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'rm -rf temp_deploy'
                        rsync -e \"ssh -i \"${PK}\"\" -a dist/ ${linuxServiceAccount_USR}@${env.targetHost}:temp_deploy/
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
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'systemctl --user stop vassago'
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
                sh """#!/bin/bash
                    make db-dump configuration=Release databasename=vassago pw_database=${database_password_prod}
                """
                
                sh """#!/bin/bash
                    make db-update pw_database=$database_password_prod
                """
                //dotnet ef database update --connection "Host=localhost;Database=vassago_prod;Username=vassago;Password=$database_password_prod;IncludeErrorDetail=true;"
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
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'cp -r dist oldgood-\$(mktemp -u XXXX)'
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'mv dist/appsettings.json appsettings.json'
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'rm -rf dist'
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'rsync -r temp_deploy/ dist/'
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'rm -rf temp_deploy'
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
                        ssh -i \"${PK}\" -tt ${linuxServiceAccount_USR}@${targetHost} 'systemctl --user start vassago'
                    """
                }
            }
        }

    }
}
