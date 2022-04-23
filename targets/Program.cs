using System.IO;
using System.Threading.Tasks;
using static Bullseye.Targets;
using static SimpleExec.Command;

internal class Program
{
    public async static Task Main(string[] args)
    {
        var sdk = new DotnetSdkManager();

        Target("default", DependsOn("volume-01", "volume-02"));

        Target("volume-01",
            Directory.EnumerateFiles("vol-1", "*.sln", SearchOption.AllDirectories),
            solution => Run(sdk.GetDotnetCliPath(), $"build \"{solution}\" --configuration Release"));

        Target("volume-02",
              Directory.EnumerateFiles("vol-2", "*.sln", SearchOption.AllDirectories),
              solution => Run(sdk.GetDotnetCliPath(), $"build \"{solution}\" --configuration Release"));

        await RunTargetsAndExitAsync(args);
    }
}
