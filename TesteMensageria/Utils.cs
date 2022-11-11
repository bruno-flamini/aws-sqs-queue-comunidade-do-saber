using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteMensageria
{
    public class Utils
    {
        public static async Task DoStuff(string[] args)
        {
            try
            {
                var queueName = "testQueue";

                var sqsClient = await GetSqsClient();

                var urlQueue = await QueryQueueArn(sqsClient, queueName);

                if (string.IsNullOrEmpty(urlQueue))
                {
                    var createdQueue = await CreateQueue(sqsClient, queueName);
                }

                await SendMessage(sqsClient, urlQueue, "Hello Gestão!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Method to create a queue. Returns the queue URL.
        private static async Task<string> CreateQueue(
          IAmazonSQS sqsClient, string qName, string deadLetterQueueUrl = null,
          string maxReceiveCount = null, string receiveWaitTime = null)
        {
            var attrs = new Dictionary<string, string>();

            // If a dead-letter queue is given, create a message queue
            if (!string.IsNullOrEmpty(deadLetterQueueUrl))
            {
                attrs.Add(QueueAttributeName.ReceiveMessageWaitTimeSeconds, receiveWaitTime);
                attrs.Add(QueueAttributeName.RedrivePolicy,
                  $"{{\"deadLetterTargetArn\":\"{await GetQueueArn(sqsClient, deadLetterQueueUrl)}\"," +
                  $"\"maxReceiveCount\":\"{maxReceiveCount}\"}}");
                // Add other attributes for the message queue such as VisibilityTimeout
            }

            // If no dead-letter queue is given, create one of those instead
            //else
            //{
            //  // Add attributes for the dead-letter queue as needed
            //  attrs.Add();
            //}

            // Create the queue
            CreateQueueResponse responseCreate = await sqsClient.CreateQueueAsync(
                new CreateQueueRequest { QueueName = qName, Attributes = attrs });
            return responseCreate.QueueUrl;
        }

        private static async Task<string> GetQueueArn(IAmazonSQS sqsClient, string qUrl)
        {
            GetQueueAttributesResponse responseGetAtt = await sqsClient.GetQueueAttributesAsync(
              qUrl, new List<string> { QueueAttributeName.QueueArn });
            return responseGetAtt.QueueARN;
        }

        private static async Task<string> QueryQueueArn(IAmazonSQS sqsClient, string qUrl)
        {
            var responseGetAtt = await sqsClient.ListQueuesAsync(qUrl);
            return responseGetAtt.QueueUrls.FirstOrDefault()!;
        }

        private static async Task<IAmazonSQS> GetSqsClient()
        {
            var credenciaisAcessoAws = new BasicAWSCredentials(@"insertYourAccessKey", @"insertYourSecretKey");
            var regionPoint = RegionEndpoint.SAEast1;
            AmazonSQSClient amazonSQSClient = new(credenciaisAcessoAws, regionPoint);
            return await Task.FromResult(amazonSQSClient);
        }

        private static async Task SendMessage(IAmazonSQS sqsClient, string qUrl, string messageBody)
        {
            SendMessageResponse responseSendMsg = await sqsClient.SendMessageAsync(qUrl, messageBody);
            Console.WriteLine($"Message added to queue\n  {qUrl}");
            Console.WriteLine($"HttpStatusCode: {responseSendMsg.HttpStatusCode}");
        }
    }
}
