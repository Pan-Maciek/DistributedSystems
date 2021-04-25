using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using zad4.Dispatcher;
using zad4.Satellite;
using zad4.Station;

const string s1Name = "Station1";
const string s2Name = "Station2";
const string s3Name = "Station3";

const string configFile = "dispatcher.conf";
var config = ConfigurationFactory.ParseString(File.ReadAllText(configFile));

var system = ActorSystem.Create("AstraLinkSystem", config);

var satellites = new Dictionary<int, IActorRef>();
for (var id = 100; id < 200; id++) 
    satellites[id] = system.ActorOf(Satellite.Props(id), $"Satellite{id}");

var dispatcherRef = system.ActorOf(Dispatcher.Props(satellites), "Dispatcher");

var s1Ref = system.ActorOf(Station.Props(s1Name, dispatcherRef), s1Name);
var s2Ref = system.ActorOf(Station.Props(s2Name, dispatcherRef), s2Name);
var s3Ref = system.ActorOf(Station.Props(s3Name, dispatcherRef), s3Name);

var rng = new Random();
foreach (var sRef in new []{s1Ref, s2Ref, s3Ref}) {
    sRef.Tell(new QueryRequest {
        FirstSatelliteId = 100 + rng.Next(50),
        Range = 50,
        Timeout = TimeSpan.FromMilliseconds(300)
    });
    sRef.Tell(new QueryRequest {
        FirstSatelliteId = 100 + rng.Next(50),
        Range = 50,
        Timeout = TimeSpan.FromMilliseconds(300)
    });
}

while (true) {
    Console.ReadLine();
}