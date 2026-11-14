using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Verse;

[RimWorldWrapperPatch]
public static class AddFixMod
{
	private static readonly string ModPath = Path.GetFullPath(
		Path.Combine(Path.GetDirectoryName(typeof(AddFixMod).Assembly.Location)!, "..", "RimWorld.Sharpice.FixMod")
	);

	private static readonly Func<ModMetaData, bool> TryAddMod =
		(Func<ModMetaData, bool>)
			Delegate.CreateDelegate(
				typeof(Func<ModMetaData, bool>),
				typeof(ModLister).GetMethod("TryAddMod", BindingFlags.NonPublic | BindingFlags.Static)!
			);

	public static void Initialize()
	{
		var harmony = new Harmony("RimWorldWrapper.AddFixMod");

		harmony.Patch(((Action)ModLister.RebuildModList).Method, postfix: new HarmonyMethod(PostfixRebuildModList));
	}

	private static void PostfixRebuildModList()
	{
		if (!Directory.Exists(Path.Combine(ModPath, "About")))
		{
			Log.Warning($"未找到 FixMod 目录: {ModPath}");
			return;
		}

		var meta = new ModMetaData(ModPath, false);

		if (!TryAddMod(meta))
		{
			return;
		}

		ModsConfig.SetActive(meta.PackageId, true);
	}
}
