//Cake, Squirrel and Electron
#addin "Cake.Squirrel"
#tool "Squirrel.Windows&version=2.0.1"




enum RuntimeEnum
{
    Win10,
    OSX,
    Linux,
}

var runtimeEnum = RuntimeEnum.Win10;
var runtime = Argument("runtime", runtimeEnum);
var certificateThumbprint = Argument("thumprint", EnvironmentVariable("SignatureCertThumbprint"));
var configuration = Argument("configuration", "Release");
var solutionRoot = Directory("./");
var solution = solutionRoot + File("SquirrelCake.sln");
var applicationDirectory = solutionRoot + Directory("SquirrelCake.Application");
var application = applicationDirectory + File("SquirrelCake.Application.csproj");
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
string GetDeploymentDirectory(RuntimeEnum runtime)
{
	switch (runtime)
	{
		case RuntimeEnum.Win10: return win10DeploymentDirectory;
		case RuntimeEnum.OSX: return macOSeploymentDirectory;
		case RuntimeEnum.Linux: return linuxDeploymentDirectory;
		default: throw new ArgumentException(nameof(runtime), $"Unknown runtime {runtime}");
	}
}
string GetPublishDirectory(RuntimeEnum runtime)
{
	var deploymentDirectory = GetDeploymentDirectory(runtime);
	return deploymentDirectory + Directory("/files");
}

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