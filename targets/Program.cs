using System.IO;
using static Bullseye.Targets;
using static SimpleExec.Command;

internal class Program
{
    public static void Main(string[] args)
    {
        var sdk = new DotnetSdkManager();

        Target("default", DependsOn("volume-01", "volume-02"));

        Target("volume-01",
            Directory.EnumerateFiles("vol-1", "*.sln", SearchOption.AllDirectories),
            solution => Run(sdk.GetDotnetCliPath(), $"build \"{solution}\" --configuration Release"));

	  Target("volume-02",
            Directory.EnumerateFiles("vol-2", "*.sln", SearchOption.AllDirectories),
            solution => Run(sdk.GetDotnetCliPath(), $"build \"{solution}\" --configuration Release"));
        
        RunTargetsAndExit(args);
    }
}
