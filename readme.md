## csx

A super simple C# script runner for .Net Core with debug and NuGet support.

### Installing 

```shell
choco install csx --source https://www.nuget.org/api/v2
```

### Visual Studio Code 

Intellisense for C# script files is provided by [OmniSharp](https://github.com/OmniSharp/omnisharp-roslyn) 

There is still an [open pull request](https://github.com/OmniSharp/omnisharp-roslyn/pull/813) that enables intellisense for scripts with NuGet references. 

Until that PR gets merged, download this [prebuilt version of OmniSharp](https://github.com/seesharper/omnisharp-roslyn/releases/tag/v1.21.0-Nuget) that you can use to enable this functionality.

> Note: Any editor can be used to edit script files, but we need VS Code to be able to debug.



#### Windows

Simply extract the [OmniSharp.zip](https://github.com/seesharper/omnisharp-roslyn/files/1065706/OmniSharp.zip) file anywhere on your drive and update VS Code settings.

```json
{
"omnisharp.path": "PATH_TO_OMNISHARP_FOLDER/Omnisharp.exe"
}
```



### Hello World

Create a new folder somewhere and from within that folder issue the following command.

```shell
csx init
```

This command creates 4 files

| Name                |                                          |
| ------------------- | ---------------------------------------- |
| .vscode/tasks.json  | Enables the script to be executed within VS Code. |
| .vscode/launch.json | Configures script debugging              |
| omnisharp.json      | Enables NuGet references in scripts (Intellisense) |
| helloworld.csx      | A minimal script that outputs "Hello world!" to the console. |

> Note: Only the actual script file is needed for script execution. Other files are just for VS Code awesomeness. 



### Running the script

#### Command Line

```shell
csx helloworld.csx
Hello world!
```

#### VS Code

Ctrl+Shift+B or "Debug->Start Without Debugging "

### Debugging

Set a breakpoint anywhere in your code and hit F5

### NuGet 

In addition to referencing other scripts using the #load directive we can also reference NuGet packages.

```csharp
#r "nuget:AutoMapper,6.0.0"
```

