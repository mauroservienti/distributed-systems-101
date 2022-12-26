using System.IO;
using System.Threading.Tasks;
using static Bullseye.Targets;
using static SimpleExec.Command;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var sdk = new DotnetSdkManager();
        var dotnet = await sdk.GetDotnetCliPath();

        Target("default",
            Directory.EnumerateFiles(".", "*.sln", SearchOption.AllDirectories),
            solution => Run(dotnet, $"build \"{solution}\" --configuration Release"));

        await RunTargetsAndExitAsync(args);
    }
}
