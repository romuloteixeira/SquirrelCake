//Cake, Squirrel and Electron
#addin "Cake.Squirrel"
#tool "Squirrel.Windows&version=2.0.1"

var solutionRoot = Directory("./");
var solutionFolder = solutionRoot + File("SquirrelCake.sln");

Task("CleanUp")
	.Does(() => {
		DotNetCoreClean(solutionFolder)
	});

Task("Restore")
	.IsDependentOn("CleanUp")
	.Does(() => {
		DotNetCoreRestore(solutionFolder);
	});

var configuration = Argument("configuration", "Release");
Task("Build")
	.IsDependentOn("Restore")
	.Does(() => {
		var settings = new DotNetCoreBuildSettings
		{
			Configuration = configuration,
			NoRestore = true,
		};
		DotNetCoreBuild(solutionFolder, settings);
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() => {
		// For now, has no test.

		/*
		var settings = new DotNetCoreTestSettings
		{
			NoRestore = true,
			Configuration = configuration,
			NoBuild = true,
		};
		DotNetCoreTest(solutionFolder, settings);
		*/
	});

enum RuntimeEnum
{
    Win10,
    OSX,
    Linux,
}

var currentRuntime = RuntimeEnum.Win10;
var applicationDirectory = solutionRoot + Directory("SquirrelCake.Application");
var application = applicationDirectory + File("SquirrelCake.Application.csproj");
Task("Publish")
	.IsDependentOn("Test")
	.Does(() => {
		var publishFolder = GetPublishFolder(currentRuntime);
		CleanDirectory(publishFolder);
		
		Information($"Publishing {currentRuntime}");
		
		var targetRuntimeId = GetRuntimeID(currentRuntime);
		var settings = new DotNetCorePublishSettings
		{
			Configuration = configuration,
			SelfContained = true,
			Runtime = targetRuntimeId,
			OutpubDirectory = publishFolder,
			///Can't use this when using Runtime
			// NoRestore = true,
			// NoBuild = true,
		};
		DotNetCorePublish(application, settings);

		if (currentRuntime == RuntimeEnum.Win10)
		{
			AddIcon();
		}
	});

void Addicon()
{
	// var settings = new ProcessSettings
	// {
	// 	WorkingDirectory = 
	// }
}

string GetRuntimeID(RuntimeEnum runtime)
{
	switch (runtime)
	{
		case RuntimeEnum.Win10: return "win10-x64";
		case RuntimeEnum.OSX: return "osx-x64";
		case RuntimeEnum.Linux: return "linux-x64";
		default: throw new ArgumentException(nameof(runtime), $"Unknown runtime {runtime}");
	}
}

string GetPublishFolder(RuntimeEnum runtime)
{
	var deploymentDirectory = GetDeploymentFolder(runtime);
	return deploymentDirectory + Directory("/files");
}

string GetDeploymentFolder(RuntimeEnum runtime)
{
	switch (runtime)
	{
		case RuntimeEnum.Win10: return win10DeploymentDirectory;
		case RuntimeEnum.OSX: return macOSeploymentDirectory;
		case RuntimeEnum.Linux: return linuxDeploymentDirectory;
		default: throw new ArgumentException(nameof(runtime), $"Unknown runtime {runtime}");
	}
}


var runtime = Argument("runtime", currentRuntime);
var certificateThumbprint = Argument("thumprint", EnvironmentVariable("SignatureCertThumbprint"));
var deploymentDirectory = Directory("./deployment");
var win10DeploymentDirectory = deploymentDirectory + Directory("win10-x64");
var macOSeploymentDirectory = deploymentDirectory + Directory("macOS-x64");
var linuxDeploymentDirectory = deploymentDirectory + Directory("linux-x64");
var nuSpecTemplate = win10DeploymentDirectory + File("AutoMasshTik.nuspec.Template");
var nuSpec = win10DeploymentDirectory + File("AutoMasshTik.nuspec");
var win10PublishDirectory = win10DeploymentDirectory + Directory("files");
var win10OutputDirectory = win10DeploymentDirectory + Directory("output");
var win10Nupgk = win10DeploymentDirectory + Directory("nupkg");
var macOSPublishDirectory = macOSeploymentDirectory + Directory("files");
const string NuspecFileTemplate = "\t<file src=\".\\files\\{1}{0}\" target=\"lib\\net45\\{1}{0}\" />";


void SignFiles(IEnumerable<FilePath> files)
{
	if (string.IsNullOrWhiteSpace(certificateThumbprint))
	{
		Information("No thumbprint to sign");
		return;
	}
	var validFiles = files.Where(f => 
                                    string.Equals(f.GetExtension(), ".exe", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(f.GetExtension(), ".dll", StringComparison.OrdinalIgnoreCase));
    Sign(validFiles, new SignToolSignSettings {
            TimeStampUri = new Uri("http://timestamp.digicert.com"),
            CertThumbprint = certificateThumbprint.Replace(" ", "")
    });
}



var target = Argument("target", "ExecuteBuild");
Task("ExecuteBuild");