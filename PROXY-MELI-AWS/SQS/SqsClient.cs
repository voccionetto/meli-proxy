using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace PROXY_MELI_AWS.SQS
{
    public class SqsClient
    {
        public SqsClient()
        {
        }

        public void PostMessage(object msg)
        {
            var queueUrl = "https://sqs.us-east-1.amazonaws.com/541644236698/proxy_meli";
            var awsCreds = new BasicAWSCredentials("AKIAX4HEDE6NN3KPHNV2", "WT+YiEag/xTe+wN0XQLHmvxpCUCiNEPr3t1V5tmO");
            //Create a client to talk to SQS
            var amazonSQSClient = new AmazonSQSClient(awsCreds, Amazon.RegionEndpoint.USEast1);
            //Create the request to send
            var sendRequest = new SendMessageRequest();
            sendRequest.QueueUrl = queueUrl;
            sendRequest.MessageBody = "{ 'message' : 'hello world' }";

            var sendMessageResponse = amazonSQSClient.SendMessageAsync(sendRequest).Result;

            var receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.QueueUrl = queueUrl;
            var response = amazonSQSClient.ReceiveMessageAsync(receiveMessageRequest).Result;
            if (response.Messages.Any())
            {
                foreach (var message in response.Messages)
                {
                    var deleteMessageRequest = new DeleteMessageRequest();
                    deleteMessageRequest.QueueUrl = queueUrl;
                    deleteMessageRequest.ReceiptHandle = message.ReceiptHandle;
                    var result = amazonSQSClient.DeleteMessageAsync(deleteMessageRequest).Result;
                }
            }

        }
    }
}
