using System;
using System.IO;
using System.Threading.Tasks;
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
        public static void Process(StreamReader reader, string fileName)
        {
            retStr = string.Empty;

            AWSCredentials creds = new AnonymousAWSCredentials();
            AmazonS3Config config = new AmazonS3Config
            {
                ProxyHost = "localhost",
                ProxyPort = 4572,
                RegionEndpoint = RegionEndpoint.GetBySystemName("us-east-1"),
                ServiceURL = "http://localhost:4572",
                UseHttp = true,
                ForcePathStyle = true,
            };
            s3Client = new AmazonS3Client(creds, config);
            PrepareBucketAsync().Wait();
            S3BucketIOWorker(reader, fileName).Wait();
            //ITransferUtility fileTransferUtility = new TransferUtility(s3Client);
            //var tranferReq = new TransferUtilityUploadRequest { };
            //tranferReq.BucketName = bucketName;
            //tranferReq.FilePath = "/home/tve/Documents/job_oneidentity/ms_dotnet/myApp/bin/Debug/netcoreapp3.1/input.txt";
            //fileTransferUtility.UploadAsync(tranferReq).Wait();


        }
        static async Task S3BucketIOWorker(StreamReader reader, string fileName)
        {
            MemoryStream outStream = new MemoryStream();
            try
            {
                PutObjectRequest putObjReq = new PutObjectRequest
                {
                    BucketName = inBucketName,
                    Key = fileName,
                    InputStream = outStream,
                    ContentType = "text/plain",
                };
                using (StreamWriter writer = new StreamWriter(outStream))
                {
                    string line;
                    PutObjectResponse objResp=(await s3Client.PutObjectAsync(putObjReq));
                    objResp.HttpStatusCode = System.Net.HttpStatusCode.Processing;
                    while ((line = reader.ReadLine()) != null)
                    {
                        writer.WriteLine(line);
                    }
                    objResp.HttpStatusCode = System.Net.HttpStatusCode.OK;

                }

            }
            catch (AmazonS3Exception e)
            {
                retStr += "Error encountered during S3 streaming. Message:" + e.Message;
            }
            catch (Exception e)
            {
                retStr += "Unknown error encountered during S3 streaming. Message:" + e.Message;
            }

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
            }
            catch (Exception e)
            {
                retStr += "Unknown error encountered on server when writing an object. Message:" + e.Message;
            }
        }


    }
}
