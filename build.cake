using System.Threading;

var projectPaths = new [] {
    "./Dev/Dev2.Common.Interfaces/Dev2.Common.Interfaces.csproj",
    "./Dev/Dev2.Core/Dev2.Core.csproj",
    "./Dev/Dev2.Server/Dev2.Server.csproj",
};
var target = Argument("target", "Default");
Task("BuildProjectsInParallel")
    .Does(() => 
    {
        BuildInParallel(projectPaths);
    });

Task("Default")
    .IsDependentOn("BuildProjectsInParallel")
    .Does(() => 
    {
    });
    
RunTarget(target);

public void BuildInParallel(
    IEnumerable<string> filePaths,
    int maxDegreeOfParallelism = -1,
    CancellationToken cancellationToken = default(CancellationToken)) 
{
    var actions = new List<Action>();
    foreach (var filePath in filePaths) {
        actions.Add(() =>
            MSBuild(filePath, configurator =>
                configurator.SetConfiguration("Debug")
                    .SetVerbosity(Verbosity.Minimal)
                    .UseToolVersion(MSBuildToolVersion.VS2019)
                    .SetMSBuildPlatform(MSBuildPlatform.x86)
                    .SetPlatformTarget(PlatformTarget.MSIL))
        );                        
    }

    var options = new ParallelOptions {
        MaxDegreeOfParallelism = maxDegreeOfParallelism,
        CancellationToken = cancellationToken
    };

    Parallel.Invoke(options, actions.ToArray());
}
