## csx

A super simple C# script runner for .Net Core with debug and NuGet support.

### Installing 

```shell
choco install csx --source https://www.nuget.org/api/v2 --pre	
```







### Vision

* Self contained scripts
* Cross platform 
* Debug support 
* NuGet support 



### Self contained scripts

Self contained scripts means that a script should not need anything else beside the script itself to specify dependencies to other scripts or to other NuGet packages.  

```c#
#r "nuget:AutoMapper/6.0.0"		
```

No additional files such as *packages.config, project.json or csproj*  are needed. In other words, no project system. The script itself is the project.



### Cross platform

Scripts should be able to run on all platforms and relies only on the dotnet cli to execute the script.

This means that scripts run in the context of an .Net Core application and we really don't care about full .Net or Mono anymore.



### Debug Support 

We should be able to debug scripts in Visual Studio Code as this is a cross platform editor that works on Windows, Mac and Linux. Visual Studio Code can only debug .Net Core applications and hence we will restrict the scripting runtime to .Net Core. 



### NuGet Support

Roslyn scripting has built-in support for referencing other script files and other assemblies, but lacks the ability to reference a NuGet package. There is an [open issue](https://github.com/dotnet/roslyn/issues/6900) on GitHub and the syntax for referencing NuGet packages is based on this issue. Basically we need a custom [MetadataReferenceResolver](https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/MetadataReference/MetadataReferenceResolver.cs) that is able to download and reference assemblies from NuGet packages. Communication with NuGet servers are implemented using the [NuGet.Protocol.Core.V3](https://www.nuget.org/packages?q=NuGet.Protocol.Core.v3) package and this package gives us access to just about everything in NuGet except actually installing packages. This functionality lives in the [NuGet.PackageManagement](https://www.nuget.org/packages/NuGet.PackageManagement) package that is not yet available for .Net Core. This means that we rely on NuGet.exe to actually download and install a package and then use the API provided through NuGet.Protocol.Core.V3 to figure out dependencies and resolving framework specific assemblies. 

### OmniSharp

As mentioned we need a custom MetadataReferenceResolver to handle NuGet package references. The nice thing about that is we can reuse the same resolver both in the script runner and in the [OmniSharp.Script](https://github.com/OmniSharp/omnisharp-roslyn/tree/dev/src/OmniSharp.Script) project. It should be possible to configure the runner and OmniSharp.Script that we want to use a custom resolver. This custom resolver should be able to be plugged into the resolve pipeline as a decorator around the default resolver.



Microsoft.CodeAnalysis.Nuget

NuGetMetadataReferenceResolver 



Dotnet.Script.NuGetMetadataResolver

















