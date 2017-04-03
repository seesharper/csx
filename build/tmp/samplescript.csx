#load "anotherscript.csx"
#r "nuget:AutoMapper/6.0.2"

using System;
using System.Text.RegularExpressions;
using System.Linq;
using AutoMapper;

Console.WriteLine("hello!");

// Call a method in 'anotherscript';
Test();


Console.WriteLine(typeof(MapperConfiguration));


