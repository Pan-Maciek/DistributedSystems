using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static System.Text.Encoding;

const string exchangeName = "GlobalExchange";

// Create connection
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare exchange
channel.ExchangeDeclare(exchangeName, "topic");

// Declare queues
channel.QueueDeclare("admin", false, false, true, null);

// Bind queues
channel.QueueBind("admin", exchangeName, "#");
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, eventArgs) => {
    var message = UTF8.GetString(eventArgs.Body.Span);
    Console.WriteLine($"< {eventArgs.RoutingKey}: {message}");
};

channel.BasicConsume("admin", true, consumer);

Console.WriteLine("Prefix message with (e!, c!, s!) to specify target");
Console.WriteLine("Press ^C to exit");

var modes = new Dictionary<string, string> {
    {"e", "admin.everyone"},
    {"c", "admin.crews"},
    {"s", "admin.suppliers"}
};

while (true) {
    switch (Regex.Split(Console.ReadLine() ?? "", @"^\s*([ecs])!\s*")) {
       case var input when input.Length == 3:
           var mode = input[1];
           var payload = UTF8.GetBytes(input[2]);
           
           channel.BasicPublish(exchangeName, modes[mode], body: payload);
           Console.WriteLine($"> {modes[mode]} {input[2]}");
           break;
       default:
           Console.WriteLine("Invalid message prefix.");
           break;
    }
}