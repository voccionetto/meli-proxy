using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace PROXY_MELI_AWS.SQS
{
    public class SqsClient
    {
        public SqsClient()
        {
            var awsCreds = new BasicAWSCredentials(AccessKey, SecretKey);
            AmazonSQSClient = new AmazonSQSClient(awsCreds, Amazon.RegionEndpoint.USEast1);
        }

        public async Task PostMessage(object obj)
        {
            try
            {

                var sendRequest = new SendMessageRequest
                {
                    QueueUrl = QueueUrl
                };

                var msg = JsonConvert.SerializeObject(obj);

                sendRequest.MessageBody = msg;

                await AmazonSQSClient.SendMessageAsync(sendRequest);
            }
            catch(AmazonSQSException ex)
            {
                //TODO: logar
                throw new Exception(ex.Message);
            }
        }

        public IList<Message> ReadMessages()
        {
            var receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.QueueUrl = QueueUrl;
            var response = AmazonSQSClient.ReceiveMessageAsync(receiveMessageRequest).Result;
            if (response.Messages.Any())
            {
                return response.Messages;
                foreach (var message in response.Messages)
                {
                    //var deleteMessageRequest = new DeleteMessageRequest();
                    //deleteMessageRequest.QueueUrl = queueUrl;s
                    //deleteMessageRequest.ReceiptHandle = message.ReceiptHandle;
                    //var result = amazonSQSClient.DeleteMessageAsync(deleteMessageRequest).Result;
                }
            }

            return new List<Message>();
        }

        public AmazonSQSClient AmazonSQSClient { get; set; }
        public string QueueUrl { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}
