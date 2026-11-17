using System.Reflection;
using HarmonyLib;
using Verse;

namespace RimWorld.Sharpice.FixMod;

public class Main : Mod
{
	public Main(ModContentPack content)
		: base(content)
	{
		Log.Message("Hello RimWorld Wrapper Fix Mod!");

		var harmony = new Harmony("RimWorldWrapper.FixMod");
		harmony.PatchAll(Assembly.GetExecutingAssembly());

		Log.Message("RimWorld Wrapper Fix Mod Load Done");
	}
}
