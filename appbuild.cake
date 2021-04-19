//Cake, Squirrel and Electron
#addin "Cake.Squirrel"
#tool "Squirrel.Windows&version=2.0.1"

var solutionRoot = Directory("./");
var solutionFolder = solutionRoot + File("SquirrelCake.sln");

Task("CleanUp")
	.Does(() => {
		DotNetCoreClean(solutionFolder);
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

var runtimeArgument = Argument<RuntimeEnum>("runtime", RuntimeEnum.Win10);
var applicationDirectory = solutionRoot + Directory("SquirrelCake.Application");
var application = applicationDirectory + File("SquirrelCake.Application.csproj");
Task("Publish")
	.IsDependentOn("Test")
	.Does(() => {
		var publishFolder = GetPublishFolder(runtimeArgument);
		CleanDirectory(publishFolder);
		
		Information($"Publishing {runtimeArgument}");
		
		var targetRuntimeId = GetRuntimeID(runtimeArgument);
		var settings = new DotNetCorePublishSettings
		{
			Configuration = configuration,
			SelfContained = true,
			Runtime = targetRuntimeId,
			OutputDirectory = publishFolder,
			///Can't use this when using Runtime
			// NoRestore = true,
			// NoBuild = true,
		};
		DotNetCorePublish(application, settings);
	});

var deploymentDirectory = Directory("./deployment");
var win10DeploymentDirectory = deploymentDirectory + Directory("win10-x64");
var macOSeploymentDirectory = deploymentDirectory + Directory("macOS-x64");
var linuxDeploymentDirectory = deploymentDirectory + Directory("linux-x64");

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

const string NuSpecFileTemplate = "\t<file src=\".\\files\\{1}{0}\" target=\"lib\\net45\\{1}{0}\" />";

var nuSpec = win10DeploymentDirectory + File("AutoMasshTik.nuspec");
var nuSpecTemplate = win10DeploymentDirectory + File("AutoMasshTik.nuspec.Template");
var win10PublishDirectory = win10DeploymentDirectory + Directory("files");
//var macOSPublishDirectory = macOSeploymentDirectory + Directory("files");
Task("NuSpec")
	.IsDependentOn("Publish")
	.Does(() => {
		CopyFile(File(nuSpecTemplate), nuSpec);

		var namespaceDic = new Dictionary<string, string>
		{
			{"ns", "http://schemas.microsoft.con/packaging/2010/07/nuspec.xsd"}
		};
		var settings = new XmlPokeSettings
		{
			Namespaces = namespaceDic,
		};
		XmlPoke(nuSpec, "/ns:package/ns:metadata/ns:version", GetVersion(), settings);

		var files = new List<string>();
		files.Add(GetExe(win10PublishDirectory));
		files.AddRange(GetFiles(win10PublishDirectory, "*"));

		var relativeFiles = files.Select(f => string.Format(NuSpecFileTemplate, f, string.Empty)).ToArray();
		XmlPoke(nuSpec, "/ns:package/ns:files", $"\n{string.Join("\n", relativeFiles)}\n", settings);
	});

string GetVersion()
{
	return XmlPeek(application, "/Project/PropertyGroup/AssemblyVersion");
}

string GetExe(string releaseDirectory)
{
	var nameExes = from f in System.IO.Directory.GetFiles(releaseDirectory, "*.exe")
					let name = System.IO.Path.GetFileName(f)
					orderby name
					select name;
	return nameExes.Single();
}

List<string> GetFiles(string releaseDirectory, string extension)
{
	var files = from f in System.IO.Directory.GetFiles(releaseDirectory, $"*.{extension}")
				let name = System.IO.Path.GetFileName(f)
				where !string.Equals(System.IO.Path.GetExtension(name), ".exe", StringComparison.OrdinalIgnoreCase)
				orderby name
				select name;
	
	return files.ToList();
}


var win10NupgkDirectory = win10DeploymentDirectory + Directory("nupkg");
Task("NuPkg")
	.IsDependentOn("NuSpec")
	.Does(() => {
		NuGetPack(nuSpec, new NuGetPackSettings{ OutputDirectory = win10NupgkDirectory});
	});

var win10OutputDirectory = win10DeploymentDirectory + Directory("output");
Task("Squirrel")
	.IsDependentOn("NuPkg")
	.Does(() => {
		FilePath nuPkgPath = win10NupgkDirectory + File(FormatNupkgName(GetVersion()));

		Squirrel(nuPkgPath, new SquirrelSettings
		{
			NoMsi = true,
			ReleaseDirectory = win10OutputDirectory,
		});

		SignFiles(new FilePath[]{ win10OutputDirectory + File("Setup.exe") });
	});

string FormatNupkgName(string version)
{
	return $"AutoMasshTik.{version}.nupkg";
}

var certificateThumbprint = Argument("thumprint", EnvironmentVariable("SignatureCertThumbprint"));
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
    Sign(validFiles, new SignToolSignSettings 
	{
            TimeStampUri = new Uri("http://timestamp.digicert.com"),
            CertThumbprint = certificateThumbprint.Replace(" ", "")
    });
}






var target = Argument("target", "ExecuteBuild");
Task("ExecuteBuild");