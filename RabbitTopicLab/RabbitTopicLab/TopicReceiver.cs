using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;


namespace RabbitTopicLab
{
	internal class TopicReceiver
	{
		private readonly string mTopic;


		public TopicReceiver(IModel model, string queueName, string topic)
		{
			mTopic = topic;

			var consumer = new EventingBasicConsumer(model);
			consumer.Received += Receive;
			model.BasicConsume(queueName, true, consumer);
		}


		private void Receive(object sender, BasicDeliverEventArgs ea)
		{
			var body = ea.Body;
			var message = Encoding.UTF8.GetString(body);
			Console.WriteLine(" [x] Received message '{0}' on topic {1}", message, mTopic);
		}
	}
}
