### Full anatomy of an AWS S3 -  Lambda project
Full anatomy of an AWS S3 -  Lambda project on .NET Core C# platform, heavily utilizing the currently accessible local development aids (amazonlinux2 in Docker, lambci/lambda in Docker and localstack in Docker) Those frustrated on the exclusive programming language option, I am to inform that this project is part of my homework in an employment application process to a company advertising a .NET Core job.
To make it more interesting, I decided to stay in front of Ubuntu not breaking my daily routine. What it involves is:
- using as much command-line tools as possible to accomplish the task,
- using MonoDevelop development environment.

For the cloud implementation I have deliberately chosen S3 and Lambda from the AWS showcase.

I have prepared a clean implementation of Arab to Roman numeral converter function, that is first developed to a console application with a sole command line parameter of the input file path string, with fallback to local directory search for “ARAB.IN” file. This solution can be found in the “console” folder of the repository. Nothing fancy, just to show the function in the purest form. It accepts a sole file, and the result always “./ROMAI.OUT” regardless of the pre-existence of the file.

Second part is the development of a lambda function that answers to the AWS S3 bucket event. The payload in this case is the S3 event that contains the S3 bucket and file name. Streaming the object the S3 event points to, the converter function’s body almost exactly the same as in the first case. 
The lambda binaries can be deployed via MS VS, AWS website or from command line: “dotnet lambda deploy-function”. *

The third part is to develop a webapp that is capable of 
- converting the uploaded files, 
- communicating with an AWS S3 bucket.*

The webapp main page offers two choices
1. uploading a file – or multiple files at once to the server to get the converted files and the downloadable links to them. 
2. Uploading a file to an AWS S3 bucket that was prepared to trigger a Lambda function with an "ObjectCreated" event, which is to convert the files as described above.
The converted files can be later downloaded from the bucket that stores the resulted files.

In order to deploy the serverless lambda service locally, the docker-update.yml file has to start the CloudFormation service. Its default port is 4581, and the name should be entered as “cloudformation”, no capitals. As node easier to maintain locally rather than in docker, just navigate the “lambdaS3/src/lambdaS3” folder in a terminal, and enter the following commands.
To install serverless:
npm install -g serverless 
npm install --save-dev serverless-localstack 

Lambda deployment steps:
dotnet build –force 
dotnet lambda package 
sls deploy --stage local --region us-east-1 
sls invoke local -f lambdaS3 --stage local --path event_payload.json --region us-east-1 

the “sls deploy --stage local --region us-east-1” command deploys the lambda to localstack based on the instructions given in “serverless.yml”: 

```
service: lambdaS3
plugins:
    - serverless-localstack
custom:
    bucket: tutorial
    localstack:
        stages:
        - local
        host: http://localhost
        endpointFile: endpts.json
        lambda:
        mountCode: True
package:
    individually: true
functions:
    lambdaS3:
        handler: lambdaS3::lambdaS3.Function::FunctionHandler
        package:
            artifact: bin/Release/netcoreapp2.1/lambdaS3.zip
        events:
            - s3:
                bucket: atorinbucket
                event: s3:ObjectCreated:*
provider:
    name: aws
    runtime: dotnetcore2.1 
```
To produce a proper payload, the serverless offers a template which can be saved like: 
```
sls generate-event --type aws:s3 --stage local>>event_payload.json 
```
Then the file have to be edited similar to this:
```
{
    "Records":
    [
    
        {
            "eventVersion":"2.0",
        "eventSource":"aws:s3",
        "awsRegion":"us-east-1",
        "eventTime":"2016-09-25T05:15:44.261Z",
        "eventName":"ObjectCreated:Put",
        "userIdentity": {
            "principalId": "AWS:AROAW5CA2KAGZPAWYRL7K:cli"
        },
        "requestParameters": {
            "sourceIPAddress": "222.24.107.21"
        },
        "responseElements": {
            "x-amz-request-id": "00093EEAA5C7G7F2",
            "x-amz-id-2": "9tTklyI/OEj4mco12PgsNksgxAV3KePn7WlNSq2rs+LXD3xFG0tlzgvtH8hClZzI963KYJgVnXw="
        },
        "s3": {
            "s3SchemaVersion": "1.0",
            "configurationId": "151dfa64-d57a-4383-85ac-620bce65f269",
            "bucket": {
                "name": "atorinbucket",
                "ownerIdentity": {
                    "principalId": "A3QLJ3P3P5QY05"
                },
                "arn": "arn:aws:lambda:us-east-1:000000000000:function:lambdaS3-local-lambdaS3"
            },
            "object": {
                "key": "input.txt"
            }
        }
    }
]
}
```
*The current hard-coded settings are comply with the localstack docker development, that is the URL of the AWS local S3 service is “http://localhost:4572”. In order to test it against a real AWS account, the URL must be re-written in the code accordingly.


