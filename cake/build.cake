///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionFile = @"..\src\AzureServiceFabric.Demo.Diagnostics.sln";
var packageProject = @"..\src\ServiceFabric.Logging\ServiceFabric.Logging.csproj";
var artifactsDirectory = Directory("./artifacts");
var nugetApiKey = EnvironmentVariable("NugetApiKey");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDirectory);

        if(BuildSystem.IsLocalBuild)
        {
            CleanDirectories(GetDirectories(@"..\src\**\obj"));
            CleanDirectories(GetDirectories(@"..\src\**\bin"));
        }
    });

Task("build")
    .IsDependentOn("clean")
    .Does(() =>
    {
        var msBuildSettings = new DotNetCoreMSBuildSettings()
            .SetMaxCpuCount(1);

        var path = MakeAbsolute(new DirectoryPath(solutionFile));

        DotNetCoreBuild(path.FullPath, new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            NoRestore = false,
            DiagnosticOutput = true,
            MSBuildSettings = msBuildSettings,
            Verbosity = DotNetCoreVerbosity.Minimal
        });
    });

Task("package")
    .IsDependentOn("build")
    .Does(() =>
    {
        var settings = new DotNetCorePackSettings
        {
            Configuration = configuration,
            NoRestore = true,
            NoBuild = true,
            OutputDirectory = artifactsDirectory
        };

        DotNetCorePack(packageProject, settings);
    });

Task("publish")
    .IsDependentOn("package")
    .WithCriteria(!string.IsNullOrEmpty(nugetApiKey))
    .Does(() =>
    {
        var packages = GetFiles(string.Concat(artifactsDirectory, @"\*.nupkg"));
        foreach(var package in packages)
        {
            NuGetPush(package, new NuGetPushSettings {
                Source = "https://www.nuget.org/api/v2/package",
                ApiKey = nugetApiKey
            });  
        }      
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("default")
    .IsDependentOn("build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);