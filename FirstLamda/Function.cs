using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using static Amazon.Lambda.SQSEvents.SQSEvent;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FirstLamda;
 public class Message
 {
    public string text {  get; set; }   
    public string operation { get; set; }
 }
public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<string> FunctionHandler(string input, ILambdaContext context)
    {
        //return input.ToUpper();
        var client = new AmazonS3Client();
        GetObjectRequest request = new GetObjectRequest
        {
            BucketName = "my-json-s3-demo",
            Key = "employee.json"
        };

        GetObjectResponse response = await client.GetObjectAsync(request);
        StreamReader reader = new StreamReader(response.ResponseStream);
        string content = reader.ReadToEnd();
        return content;

    }

    public async Task ProcessStringFunction(SQSEvent sqsevent, ILambdaContext context)
    {
        string responseQueueUrl = "https://sqs.us-east-1.amazonaws.com/058000012272/str-response-queue";
        
        if (sqsevent != null)
        {
            Console.WriteLine(sqsevent.ToString());
            var _sqsClient = new AmazonSQSClient();
            foreach (var message in sqsevent.Records)
            {
                
                var processedMessage = JsonConvert.DeserializeObject<Message>(message.Body);

                string processedText = string.Empty;
                if (processedMessage?.operation == "toLower")
                {
                    processedText = processedMessage.text.ToLower();
                }
                else if (processedMessage?.operation == "toUpper")
                {
                    processedText = processedMessage.text.ToUpper();
                }
                // Send result to ResponseQueue or another mechanism
                var responseMessage = new Dictionary<string, string>
                {
                    { "original", processedMessage.text },
                    { "processed", processedText }
                 };
                ;
                await _sqsClient.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = responseQueueUrl,
                    MessageBody = JsonConvert.SerializeObject(responseMessage)
                });

            }
          
        }
        
    }

}
