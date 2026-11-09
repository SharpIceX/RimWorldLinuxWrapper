using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

internal static class StartupHook
{
	public static void Initialize()
	{
		string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
		string harmonyPath = Path.Combine(pluginDir, "0Harmony.dll");

		if (!AssemblyLoadContext.Default.Assemblies.Any(a => a.GetName().Name == "0Harmony"))
			AssemblyLoadContext.Default.LoadFromAssemblyPath(harmonyPath);

		DisableDLCCheck.Initialize();
	}
}
