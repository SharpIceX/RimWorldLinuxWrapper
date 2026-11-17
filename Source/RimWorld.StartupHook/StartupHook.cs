using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace RimWorld.StartupHook;

internal static class StartupHook
{
	public static void Initialize()
	{
		string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
		string harmonyPath = Path.Combine(pluginDir, "0Harmony.dll");

		if (!AssemblyLoadContext.Default.Assemblies.Any(a => a.GetName().Name == "0Harmony"))
			AssemblyLoadContext.Default.LoadFromAssemblyPath(harmonyPath);

		var assembly = Assembly.GetExecutingAssembly();

		foreach (var type in assembly.GetTypes().Where(t => t.IsDefined(typeof(RimWorldWrapperPatchAttribute), false)))
		{
			var method =
				type.GetMethod(
					"Initialize",
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
					null,
					Type.EmptyTypes,
					null
				) ?? throw new InvalidOperationException($"补丁类 {type.FullName} 缺少 Initialize() 方法");

			method.Invoke(null, null);
		}
	}
}
