using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Encryption;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace lambdaS3
{
    struct RomanDigit
    {
        public bool pT; //isPrimaryType;//is it ten exponential that is in the set of {I, X, C, M}
        public string d; //digit;
        public int n;
    }

    public class Function
    {
        private const string inBucketName = "atorinbucket";
        private const string outBucketName = "atoroutbucket";
        // Specify your bucket region (an example region is shown).
        private IAmazonS3 s3Client; IAmazonS3 S3Client { get; set; }
        private string bktKey = String.Empty;
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
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
        }
        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        public Function(IAmazonS3 s3Client)
        {
            this.S3Client = s3Client;
        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            var s3Event = evnt.Records?[0].S3;

            if(s3Event == null)
            {
                return "Empty event";
            }
            try
            {
                bktKey = s3Event.Object.Key;
                context.Logger.LogLine($"Getting object {s3Event.Object.Key} from bucket: {s3Event.Bucket.Name}.");

               ProcessObjectDataAsync(context).Wait();
                context.Logger.LogLine($"Returned from data processing");
                //GetObjectMetadataRequest mdReq = new GetObjectMetadataRequest
                //{
                //    BucketName = s3Event.Bucket.Name,
                //    Key = s3Event.Object.Key,
                //};
                //var response = await this.S3Client.GetObjectMetadataAsync(mdReq);

                return "Success";
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
                context.Logger.LogLine(e.Message);
                context.Logger.LogLine(e.StackTrace);
                return "Exception";
            }
        }
        async Task ProcessObjectDataAsync(ILambdaContext  context)
        {
            List<int> arabNums = new List<int> { };
            try
            {
                context.Logger.LogLine($"GetObjectAsync, key: {this.bktKey}, bucket: {inBucketName}.");

                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = inBucketName,
                    Key = this.bktKey,

                };
                using (GetObjectResponse response = (await s3Client.GetObjectAsync(request)))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            int numToAdd = Int32.Parse(line);
                            if (numToAdd > 0 && numToAdd < 4000)
                            {
                                arabNums.Add(numToAdd);
                            }
                            else
                            {
                                context.Logger.LogLine("The input file contains number(s) out of range 0 - 4000 n:" + numToAdd);

                            }

                        }
                        catch (FormatException e)
                        {
                            context.Logger.LogLine("The input file contains non integer-convertible line(s) {0}"+ e.Message);
                            return;
                        }
                    }
                    try
                    {
                        string convertedNumStr = string.Empty;
                        PutObjectRequest pReq = new PutObjectRequest
                        {
                            BucketName = outBucketName,
                            ContentBody = convertedNumStr,
                            Key= this.bktKey,
                        };

                        foreach (int iia in arabNums)
                        {
                            convertedNumStr += ArabToRoman(iia) + "\\n";
                        }
                        PutObjectResponse pRes= await s3Client.PutObjectAsync(pReq);
                 
                    }
                    catch (Exception e)
                    {
                        context.Logger.LogLine("Error writing the output bucket file" + e.Message);
                        return;
                    }
                }

            }
            catch (AmazonS3Exception e)
            {
                context.Logger.LogLine("Error reading object. Message:" + e.Message);
                return;

            }
            catch (Exception e)
            {
                context.Logger.LogLine("Unknown error reading object. Message:" + e.Message);
                return;
            }
        }
         

        public static string ArabToRoman(int arabNum)
        {
            List<RomanDigit> romanDigits = new List<RomanDigit>() {
             new RomanDigit{pT=true,  n=1000, d="M" },
             new RomanDigit{pT=false, n=500,  d="D" },
             new RomanDigit{pT=true,  n=100,  d="C" },
             new RomanDigit{pT=false, n=50,   d="L" },
             new RomanDigit{pT=true,  n=10,   d="X" },
             new RomanDigit{pT=false, n=5,    d="V" },
             new RomanDigit{pT=true,  n=1,    d="I" } };
            var retStr = string.Empty;
            if (arabNum > 0 && arabNum <= 4000)
            {
                while (arabNum > 0)
                {
                    int iiPri = romanDigits.FindIndex(x => x.n <= arabNum); /// Primary index
                    if (iiPri > 0)
                    {
                        int iiNeg = iiPri + (romanDigits[iiPri].pT ? 0 : 1); /// Index of the first element of the reduced number
                        int neg = romanDigits[iiNeg].n;/// The first element of the reduced number
                        int iiRed = romanDigits.FindIndex(x => x.n - neg <= arabNum);/// Index of the largest reduced number 
                        if (iiRed < iiPri)/// If the reduced number is more significant
                        {
                            iiPri = iiRed;
                            arabNum -= (romanDigits[iiPri].n - neg);
                            retStr += (romanDigits[iiNeg].d + romanDigits[iiPri].d);
                        }
                        else
                        {
                            arabNum -= romanDigits[iiPri].n;
                            retStr += romanDigits[iiPri].d;
                        }

                    }
                    else /// If 1000
                    {
                        arabNum -= romanDigits[iiPri].n;
                        retStr += romanDigits[iiPri].d;
                    }

                }
            }
            else
            {
                /// With the actual Main() function, it is never processed
                retStr = "ERROR: The number must be between 0 and 4000";

            }
            return retStr;
        }

    }
}
