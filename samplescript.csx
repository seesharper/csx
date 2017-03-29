#r "nuget:AutoMapper/6.0.2"
#r "nuget:Newtonsoft.Json/10.0.1"
#r "nuget:System.Runtime.Serialization.Formatters/4.3.0"
#r "nuget:System.ComponentModel.TypeConverter/4.0.0"
using Newtonsoft.Json;
using AutoMapper;
using System;

Console.WriteLine("hello!");

var test = new { hi = "i'm json!" };
Console.WriteLine(JsonConvert.SerializeObject(test));

Console.WriteLine(typeof(MapperConfiguration));