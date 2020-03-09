using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.Runtime;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Hosting;



namespace mvc_ator_webapp
{
    public static class RegisteredS3
    {
        public static string retStr = string.Empty;
        public static IWebHostEnvironment env;
        private const string inBucketName = "atorinbucket";
        private const string outBucketName = "atoroutbucket";
        private static IAmazonS3 s3Client;
        public static void Process(MemoryStream inStream, string fileName)
        {
            retStr = string.Empty;

            AWSCredentials creds = new AnonymousAWSCredentials();
            AmazonS3Config config = new AmazonS3Config
            {
                ProxyHost = "aws-localstack",
                ProxyPort = 4572,
                RegionEndpoint = RegionEndpoint.GetBySystemName("us-east-1"),
                ServiceURL = "http://aws-localstack:4572",
                UseHttp = true,
                ForcePathStyle = true,
            };
            s3Client = new AmazonS3Client(creds, config);
            PrepareBucketAsync().Wait();
            S3BucketIOWorker(inStream, fileName).Wait();
        }
        static async Task S3BucketIOWorker(MemoryStream inStream, string fileName)
        {
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();
            InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = inBucketName,
                Key = fileName
            };
            InitiateMultipartUploadResponse initResponse =
            await s3Client.InitiateMultipartUploadAsync(initiateRequest);

            try
            {

                const long chunkSize = 3145728;
                long sLen = inStream.Length;
                long partSize;
                if (sLen > 0)
                    for (int iii = 1; sLen > (iii - 1) * chunkSize; iii++)
                    {
                        if (sLen <= iii * chunkSize)
                        {
                            partSize = sLen % (long)chunkSize;
                            if (partSize == 0)
                                break;
                        }
                        else
                        {
                            partSize = chunkSize;
                        }
                        UploadPartRequest uploadRequest = new UploadPartRequest
                        {
                            BucketName = inBucketName,
                            Key = fileName,
                            UploadId = initResponse.UploadId,
                            PartNumber = iii,
                            PartSize = partSize,
                            InputStream = inStream,
                            
                        };
                        retStr += partSize.ToString() + "\n";
                        uploadResponses.Add(await s3Client.UploadPartAsync(uploadRequest));

                    }

                CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = inBucketName,
                    Key = fileName,
                    UploadId = initResponse.UploadId,
                };
                completeRequest.AddPartETags(uploadResponses);

                CompleteMultipartUploadResponse completeUploadResponse =
                    await s3Client.CompleteMultipartUploadAsync(completeRequest);

                retStr += completeUploadResponse.HttpStatusCode.ToString();

            }
            catch (Exception exception)
            {
                retStr+="An AmazonS3Exception was thrown: " + exception.Message;

                // Abort the upload.
                AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                {
                    BucketName = inBucketName,
                    Key = fileName,
                    UploadId = initResponse.UploadId
                };
                await s3Client.AbortMultipartUploadAsync(abortMPURequest);
            }
            //PutObjectRequest putObjReq = new PutObjectRequest
            //{
            //    BucketName = inBucketName,
            //    Key = fileName,
            //    InputStream = outStream,
            //    ContentType = "text/plain",
            //};
            //using (StreamWriter writer = new StreamWriter(outStream))
            //{
            //    string line;
            //    PutObjectResponse objResp=(await s3Client.PutObjectAsync(putObjReq));
            //    objResp.HttpStatusCode = System.Net.HttpStatusCode.Processing;
            //    while ((line = reader.ReadLine()) != null)
            //    {
            //        writer.WriteLine(line);
            //    }
            //    objResp.HttpStatusCode = System.Net.HttpStatusCode.OK;

            //}



        }

        static async Task PrepareBucketAsync()
        {
            try
            {
                ListBucketsResponse bucketList = await s3Client.ListBucketsAsync();
                bool hasInBucket = false;
                bool hasOutBucket = false;
                var putBucketRequest = new PutBucketRequest { };
                /// DoesS3BucketExistV2Async would be here in an ideal world
                /// This loop is necessary because DoesS3BucketExistV2Async throws exception
                /// with localstack
                foreach (var iiBucket in bucketList.Buckets)
                {
                    if (!hasOutBucket)
                    {
                        if (iiBucket.BucketName.Equals(inBucketName))
                        {
                            hasInBucket = true;
                            continue;
                        }
                        if (iiBucket.BucketName.Equals(outBucketName))
                        {
                            hasOutBucket = true;
                            continue;
                        }
                    }
                    if (hasOutBucket&&hasInBucket)
                    {
                        break;
                    }
                }
                if (!hasInBucket)
                {
                    putBucketRequest.BucketName = inBucketName;
                    await s3Client.PutBucketAsync(putBucketRequest);
                }
                if (!hasOutBucket)
                {
                    putBucketRequest.BucketName = outBucketName;
                    await s3Client.PutBucketAsync(putBucketRequest);
                }

            }
            catch (AmazonS3Exception e)
            {
                retStr+= "Error encountered on server when writing an object. Message:" + e.Message;
                return;
            }
            catch (Exception e)
            {
                retStr += "Unknown error encountered on server when writing an object. Message:" + e.Message;
                return;
            }
        }


    }
}
