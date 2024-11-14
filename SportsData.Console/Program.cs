// See https://aka.ms/new-console-template for more information

using SportsData.Faker;
using System.Text.Json;

var eventsFaker = SportsDataFaker.GetSportEventFaker();
var fakeEvents = eventsFaker.Generate(100);

var randomEvent = fakeEvents[new Random().Next(100)];

var json = JsonSerializer.Serialize(randomEvent, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine(json);