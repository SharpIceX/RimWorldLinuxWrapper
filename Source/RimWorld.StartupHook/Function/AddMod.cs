using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Verse;

[RimWorldWrapperPatch]
public static class AddMod
{
	private static readonly string BaseDir = Path.GetDirectoryName(typeof(AddMod).Assembly.Location)!;

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
		AddAndActivate("RimWorld.Sharpice.ExtraContent");
		AddAndActivate("RimWorld.Sharpice.FixMod");
	}

	private static void AddAndActivate(string folderName)
	{
		var modPath = Path.GetFullPath(Path.Combine(BaseDir, "..", folderName));

		if (!Directory.Exists(Path.Combine(modPath, "About")))
		{
			Log.Warning($"未找到 {folderName} 目录: {modPath}");
			return;
		}

		var meta = new ModMetaData(modPath, false);

		if (!TryAddMod(meta))
		{
			return;
		}

		ModsConfig.SetActive(meta.PackageId, true);
	}
}
