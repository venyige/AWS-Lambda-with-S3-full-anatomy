### Full anatomy of an AWS S3 -  Lambda project
Full anatomy of an AWS S3 -  Lambda project on .NET Core C# platform, heavily utilizing the currently accessible local development aids (amazonlinux2 in Docker, lambci/lambda in Docker and localstack in Docker) Those frustrated on the exclusive programming language option, I am to inform that this project is part of my homework in an employment application process to a company advertising a .NET Core job.
To make it more interesting, I decided to stay in front of Ubuntu not breaking my daily routine. What it involves is:
- using as much command-line tools as possible to accomplish the task,
- using MonoDevelop development environment.

For the cloud implementation I have deliberately chosen S3 and Lambda from the AWS showcase.

I have prepared a clean implementation of Arab to Roman numeral converter function, that is first developed to a console application with a sole command line parameter of the input file path string, with fallback to local directory search for “ARAB.IN” file. This solution can be found in the “console” folder of the repository.

Second part is the development of a lambda function that answers to the AWS S3 bucket event. The payload in this case is the S3 event that contains the S3 bucket and file name. Streaming the object carried by the S3 event, the converter function’s body almost exactly the same as in the first case.


Third part is to develop a webapp that is capable of 
- converting the uploaded files, 
- communicating with an AWS S3 bucket (in this case no conversion on the server side).

The webapp main page offers two choices
1. uploading a file – or multiple files at once to the server to get the converted files and the downloadable links to them. 
2. Uploading a file to an AWS S3 bucket that was prior triggered with a Lambda function, which is to convert the files as described above.
The converted files can be later downloaded from the bucket that stores the resulted files.

The lambda binaries can be deployed via MS VS, AWS website or from command line: “dotnet lambda deploy-function”.
