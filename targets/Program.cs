using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Bullseye.Targets;
using static SimpleExec.Command;

class Program
{
    public static async Task Main(string[] args)
    {
        Target("default",
            Directory.EnumerateFiles(".", "*.sln", SearchOption.AllDirectories).Order(),
            solution => Run("dotnet", $"build \"{solution}\" --configuration Release"));

        Target("test",
            dependsOn: ["default"],
            Directory.EnumerateFiles(".", "*.sln", SearchOption.AllDirectories).Order(),
            solution => Run("dotnet", $"test \"{solution}\" --configuration Release --no-build"));

        await RunTargetsAndExitAsync(args);
    }
}
