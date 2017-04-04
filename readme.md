## csx

A super simple C# script runner for .Net Core with debug and NuGet support.

### Installing 

```shell
choco install csx --source https://www.nuget.org/api/v2 --pre	
```

### Hello World

Create a new folder somewhere and create a new script file (helloworld.csx).

```csharp
Console.WriteLine("Hello World");
```

Running the script is as simple as 

```shell
csx helloworld.csx
```

### Debugging 

In order to debug a script, open the folder containing the file in [VS Code](http://code.visualstudio.com/).

Then add the following configuration (launch.json)

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Debug Script",
            "type": "coreclr",
            "request": "launch",                        
            "program": "C:/ProgramData/Chocolatey/lib/csx/csx/csx.exe",            
            "args": ["${workspaceRoot}/helloworld.csx"],
            "cwd": "${workspaceRoot}",
            "stopAtEntry": false,
            "console": "internalConsole",
            "requireExactSource": false
        }
    ]
}
```

Now just set a breakpoint in the file and hit F5.

### NuGet 

In addition to referencing other scripts using the #load directive we can also reference NuGet packages.

```csharp
#r "nuget:AutoMapper/6.0.0"
```

