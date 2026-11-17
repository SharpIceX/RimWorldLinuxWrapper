using System;
using HarmonyLib;
using Verse;

namespace RimWorld.Sharpice.FixMod.Patches.Core;

[HarmonyPatch(typeof(PawnRenderNode_Fur), nameof(PawnRenderNode_Fur.GraphicFor))]
public static class PawnRenderNode_Fur_GraphicFor_NullGuard
{
	[HarmonyPrefix]
	[HarmonyPriority(Priority.First)]
	public static bool Prefix(Pawn pawn, ref Graphic? __result)
	{
		var furDef = pawn?.story?.furDef;
		if (furDef == null || string.IsNullOrEmpty(furDef.GetFurBodyGraphicPath(pawn)))
		{
			__result = null;
			return false;
		}
		return true;
	}

	[HarmonyFinalizer]
	public static Exception? Finalizer(Exception __exception, ref Graphic? __result)
	{
		if (__exception is ArgumentNullException or NullReferenceException)
		{
			__result = null;
			return null;
		}
		return __exception;
	}
}
