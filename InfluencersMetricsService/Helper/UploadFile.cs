using Amazon.ElasticTranscoder;
using Amazon.ElasticTranscoder.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InfluencersMetricsService.Helper
{
    class UploadFile
    {
        private const string bucketName = "influencersmetrics";//"*** provide bucket name ***";
        private static string keyName = "*** provide a name for the uploaded object ***";
        private static string filePath = "*** provide the full path name of the file to upload ***";
        private static readonly string UrlPublic = "https://s3.amazonaws.com/influencersmetrics/";
        private static readonly Amazon.RegionEndpoint bucketRegion = Amazon.RegionEndpoint.USEast1;//Amazon.RegionEndpoint.SAEast1;
        private static IAmazonS3 s3Client;
        private static string OndeEstou;

        public static string UploadFileLoad(string _fileName, string _keyname = "")
        {
            try
            {
                OndeEstou = "Inicio";
                keyName = _keyname;
                filePath = _fileName;
                s3Client = new AmazonS3Client(bucketRegion);
                OndeEstou = "Antes do Upload";
                var result = UploadFileAsync().Result;
                OndeEstou = "Após Upload";
                return result;
            }
            catch
            (Exception ex)
            {
                throw (new Exception("UploadFileLoad::" + "("+OndeEstou+")" + ex.Message));
            }
        }

        private static async Task<string> UploadFileAsync()
        {
            try
            {
                if (wfFileExists(bucketName, keyName))
                {
                    return UrlPublic + keyName;
                }

                var fileTransferUtility = new TransferUtility(s3Client);

                using (var fileToUpload = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        ContentType = keyName.Contains(".jpg") ? "image/jpeg" : "audio/mpeg",
                        InputStream = fileToUpload,
                        BucketName = bucketName,
                        //FilePath = filePath,
                        StorageClass = S3StorageClass.StandardInfrequentAccess,
                        PartSize = 10291456,
                        Key = keyName,
                        CannedACL = S3CannedACL.PublicRead
                    };
                    
                    await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);

                    return UrlPublic + keyName;

                }
            }
            catch (AmazonS3Exception e)
            {
                Library.WriteErrorLog(" "+e.Message);
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
                return "";
            }
            catch (Exception e)
            {
                Library.WriteErrorLog(" " + e.Message);
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
                return "";
            }
        }

        public static bool wfFileExists(String pBucketName, String pKeyName)
        {
            bool retVal = false;
            try
            {
                OndeEstou = "fileExists - Inicio";
                if (new Amazon.S3.IO.S3FileInfo(s3Client, pBucketName, pKeyName).Exists)
                {
                    OndeEstou = "fileExists - Existe";
                    retVal = true;
                }
                OndeEstou = "fileExists - Final";
            }
            catch (AmazonS3Exception s3Exception)
            {
                Library.WriteErrorLog(" " + s3Exception.Message);
                Console.WriteLine(s3Exception.Message, s3Exception.InnerException);

                throw (s3Exception);
            }
            catch (Exception e)
            {
                Library.WriteErrorLog("" + e.Message);
                throw (e);
            }

            return retVal;
        }

        public static string UploadVideoToLocation(Stream fs, String folder, String subFolder, String filename)
        {
            filename = filename.Replace("+", "");
            String filePath = folder.Replace("+", "") + "/" + subFolder.Replace("+", "") + "/" + Guid.NewGuid() + filename;
            if (string.IsNullOrEmpty(Path.GetExtension(filePath)))
            {
                filePath += ".mp4";
            }

            var client = s3Client;
            {
                PutObjectRequest request = new PutObjectRequest {
                    BucketName = "videoToconvert",
                    CannedACL = S3CannedACL.PublicRead,
                    Key = filePath,
                    InputStream = fs
                };
                client.PutObject(request);
            }
            String finalOriginalPath = UrlPublic + filePath;
            finalOriginalPath = finalOriginalPath.Replace("+", "%2B");

            var etsClient = new AmazonElasticTranscoderClient(bucketRegion);

            var notifications = new Notifications()
            {
                Completed = "arn:aws:sns:us-west-2:277579135337:Transcode",
                Error = "arn:aws:sns:us-west-2:277579135337:Transcode",
                Progressing = "arn:aws:sns:us-west-2:277579135337:Transcode",
                Warning = "arn:aws:sns:us-west-2:277579135337:Transcode"
            };

            var pipeline = new Pipeline();
            if (etsClient.ListPipelines().Pipelines.Count == 0)
            {
                pipeline = etsClient.CreatePipeline(new CreatePipelineRequest()
                {
                    Name = "MyTranscodedVideos",
                    InputBucket = "videoToconvert",
                    OutputBucket = "videoToconvert",
                    Notifications = notifications,
                    Role = "arn:aws:iam::277579135337:role/Elastic_Transcoder_Default_Role",
                }).Pipeline; //createpipelineresult
            }
            else
            {
                pipeline = etsClient.ListPipelines().Pipelines.First();
            }

            etsClient.CreateJob(new CreateJobRequest()
            {
                PipelineId = pipeline.Id,
                Input = new JobInput()
                {
                    AspectRatio = "auto",
                    Container = "mp4", //H.264
                    FrameRate = "auto",
                    Interlaced = "auto",
                    Resolution = "auto",
                    Key = filePath
                },
                Output = new CreateJobOutput()
                {
                    ThumbnailPattern = "thumbnnail{count}",
                    Rotate = "0",
                    PresetId = "1351620000001-000010", //Generic-720 px
                    Key = finalOriginalPath
                }
            });

            var delClient = s3Client;
            {
                //delClient.DeleteObject("VideoToConvert", filePath);
            }

            return finalOriginalPath;
        }

    }
}
