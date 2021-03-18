using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static System.Text.Encoding;

const string exchangeName = "GlobalExchange";

Console.Write("Enter crew name: ");
var name = Console.ReadLine();

// Create connection
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare exchange
channel.ExchangeDeclare(exchangeName, "topic");

// Declare queues
channel.QueueDeclare($"orders.done.{name}", false, false, true, null);
channel.QueueDeclare($"admin.crew.{name}", false, false, true, null);
    
// Bind queues
channel.QueueBind($"orders.done.{name}", exchangeName, $"orders.done.{name}");
channel.QueueBind($"admin.crew.{name}", exchangeName, "admin.crews");
channel.QueueBind($"admin.crew.{name}", exchangeName, "admin.everyone");

void Order(string item) {
 channel.BasicPublish(exchangeName, $"orders.new.{item}", body: UTF8.GetBytes(name!));
 Console.WriteLine($"> orders.new.{item} {name}");
}

void OrderProcessed(object sender, BasicDeliverEventArgs eventArgs) {
 var message = UTF8.GetString(eventArgs.Body.Span);
 Console.WriteLine($"< {eventArgs.RoutingKey} {message}");
}

void ReceivedAdminMessage(object sender, BasicDeliverEventArgs eventArgs) {
 Console.WriteLine($"< Admin: {UTF8.GetString(eventArgs.Body.Span)}");    
}

var consumer = new EventingBasicConsumer(channel);
consumer.Received += OrderProcessed;

var adminConsumer = new EventingBasicConsumer(channel);
adminConsumer.Received += ReceivedAdminMessage;

channel.BasicConsume($"orders.done.{name}", true, consumer);
channel.BasicConsume($"admin.crew.{name}", true, adminConsumer);

Console.WriteLine("Press ^C to exit");
while (true) {
 Order(Console.ReadLine());
}