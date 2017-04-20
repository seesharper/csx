
#load "common.csx"

string pathToBuildDirectory = @"tmp/";
private string version = "1.0.0-beta2";

WriteLine("csx version {0}" , version);

Execute(() => InitializBuildDirectories(), "Preparing build directories");
Execute(() => BuildAllFrameworks(), "Building all frameworks");
Execute(() => CreateNugetPackages(), "Creating NuGet packages");

private void CreateNugetPackages()
{			
	Choco.Pack("csx.nuspec", pathToBuildDirectory);
	//NuGet.CreatePackage("csx.nuspec",pathToBuildDirectory);	
    // string myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    // RoboCopy(pathToBuildDirectory, myDocumentsFolder, "*.nupkg");		
}

private void BuildAllFrameworks()
{		
	BuildDotNet();
}

private void BuildDotNet()
{		
	string pathToProjectFile = Path.Combine(pathToBuildDirectory, @"csx.csproj");
	DotNet.Build(pathToProjectFile);
	DotNet.Publish(pathToProjectFile);
}

private void InitializBuildDirectories()
{
	DirectoryUtils.Delete(pathToBuildDirectory);			
	Execute(() => InitializeNugetBuildDirectory("NETCOREAPP11"), "NetCoreApp1.1");	    						
}

private void InitializeNugetBuildDirectory(string frameworkMoniker)
{	
    CreateDirectory(pathToBuildDirectory);
	RoboCopy("../src", pathToBuildDirectory, "/e /XD bin obj .vs NuGet TestResults packages /XF project.lock.json");											  
}
 
