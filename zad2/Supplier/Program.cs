using System;
using System.Text.RegularExpressions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static System.Text.Encoding;

const string exchangeName = "GlobalExchange";

Console.Write("Enter supplier name: ");
var name = Console.ReadLine();

Console.Write("Enter available equipment (separated by spaces): ");
var items = Console.ReadLine()!.Split(' ');

// Create connection
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare exchange
channel.ExchangeDeclare(exchangeName, "topic");

// Declare queues
channel.QueueDeclare($"admin.supplier.{name}", false, false, true, null);

// Bind queues
channel.QueueBind($"admin.supplier.{name}", exchangeName, "admin.suppliers");
channel.QueueBind($"admin.supplier.{name}", exchangeName, "admin.everyone");

void OrderReceived(object sender, BasicDeliverEventArgs eventArgs) {
    var client = UTF8.GetString(eventArgs.Body.ToArray());
    var item = Regex.Match(eventArgs.RoutingKey, @"\w+$").Value;
    Console.WriteLine($"< {eventArgs.RoutingKey} {client}");
    var guid = Guid.NewGuid();
    channel.BasicPublish(exchangeName, $"orders.done.{client}", body: UTF8.GetBytes($"{item}: {guid}"));
    Console.WriteLine($"> orders.done.{client} {item}: {guid}");
}

void ReceivedAdminMessage(object sender, BasicDeliverEventArgs eventArgs) {
    Console.WriteLine($"< Admin: {UTF8.GetString(eventArgs.Body.Span)}");    
}

var ordersConsumer = new EventingBasicConsumer(channel);
ordersConsumer.Received += OrderReceived;

foreach (var item in items) {
    channel.QueueDeclare($"orders.new.{item}", false, false, true, null);
    channel.QueueBind($"orders.new.{item}", exchangeName, $"orders.new.{item}");
    channel.BasicConsume($"orders.new.{item}", true, ordersConsumer);
}

var adminConsumer = new EventingBasicConsumer(channel);
adminConsumer.Received += ReceivedAdminMessage;

channel.BasicConsume($"admin.supplier.{name}", true, adminConsumer);

Console.WriteLine("Press ^C to exit");
while (true) { }