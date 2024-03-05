using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StringOperation.Models;
using System.Diagnostics;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StringOperation.Controllers
{
    public class ProcessedMessage
    {
        public string Original { get; set; }
        public string Processed { get; set; }
    }
    public class OutputModel
    {
        public string LabelText { get; set; }
       
    }
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {

            return View();
        }
        public async Task<IActionResult> ToLowerAsync(string textBoxValue)
        {
            var sqsClient = new AmazonSQSClient();
            string requestQueueUrl = "https://sqs.us-east-1.amazonaws.com/058000012272/str-request-queue";
            string responseQueueUrl = "https://sqs.us-east-1.amazonaws.com/058000012272/str-response-queue";

            OutputModel outputModel = new OutputModel();

            var messageBody = new { text = textBoxValue, operation = "toLower" };

            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = requestQueueUrl,
                MessageBody = JsonConvert.SerializeObject(messageBody)  
            };
            var response = await sqsClient.SendMessageAsync(sendMessageRequest);

            if(response.HttpStatusCode==HttpStatusCode.OK)
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = responseQueueUrl,
                    MaxNumberOfMessages = 1
                };
                var responseMessage = await sqsClient.ReceiveMessageAsync(request);
                foreach (var message in responseMessage.Messages)
                {
                    var processedMessage = JsonConvert.DeserializeObject<ProcessedMessage>(message.Body);
                    var deleteRequest = new DeleteMessageRequest
                    {
                        QueueUrl = responseQueueUrl,
                        ReceiptHandle = message.ReceiptHandle
                    };
                    var deleteResponse = await sqsClient.DeleteMessageAsync(deleteRequest);
                    if (deleteResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        outputModel.LabelText= string.Format("Input text :{0} Processed Message : {1}", processedMessage.Original, processedMessage.Processed);
                        

                      

                    }

                    // Process the received message
                }

            }
            return Content(outputModel.LabelText);
          


        }
        public async Task<IActionResult> ToUpperAsync(string textBoxValue)
        {
            var sqsClient = new AmazonSQSClient();
            string queueUrl = "https://sqs.us-east-1.amazonaws.com/058000012272/str-request-queue";
            string responseQueueUrl = "https://sqs.us-east-1.amazonaws.com/058000012272/str-response-queue";
            var messageBody = new { text = textBoxValue, operation = "toUpper" };
            OutputModel outputModel = new OutputModel();
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = JsonConvert.SerializeObject(messageBody)  
            };
            var response = await sqsClient.SendMessageAsync(sendMessageRequest);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = responseQueueUrl,
                    MaxNumberOfMessages = 1
                };
                var responseMessage = await sqsClient.ReceiveMessageAsync(request);
                foreach (var message in responseMessage.Messages)
                {
                    var processedMessage = JsonConvert.DeserializeObject<ProcessedMessage>(message.Body);
                    var deleteRequest = new DeleteMessageRequest
                    {
                        QueueUrl = responseQueueUrl,
                        ReceiptHandle = message.ReceiptHandle
                    };
                    var deleteResponse = await sqsClient.DeleteMessageAsync(deleteRequest);
                    if (deleteResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        outputModel.LabelText = string.Format("Input text :{0} Processed Message : {1}", processedMessage.Original, processedMessage.Processed);
                    }

                    // Process the received message
                }

            }

            return Content(outputModel.LabelText);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
