using System;
using HarmonyLib;
using Verse;

namespace RimWorld.StartupHook.Patches;

[RimWorldWrapperPatch]
public static class DisablePseudoTranslation
{
	public static void Initialize()
	{
		var harmony = new Harmony("RimWorldWrapper.DisablePseudoTranslation");

		var targetMethod =
			AccessTools.DeclaredMethod(typeof(Translator), "PseudoTranslated", [typeof(string)])
			?? throw new InvalidOperationException("找不到 Translator.PseudoTranslated(string) 方法");

		harmony.Patch(
			targetMethod,
			prefix: new HarmonyMethod(typeof(DisablePseudoTranslation), nameof(PrefixPseudoTranslated))
		);
	}

	public static bool PrefixPseudoTranslated(string original, ref string __result)
	{
		__result = original;
		return false;
	}
}
