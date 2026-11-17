using HarmonyLib;
using Verse;

namespace RimWorld.Sharpice.FixMod.Patches.Rimvali;

// 在抽取异种阶段修正
[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GetXenotypeForGeneratedPawn))]
public static class Patch_GetXenotypeForGeneratedPawn
{
	[HarmonyPriority(Priority.Last)]
	public static void Postfix(PawnGenerationRequest request, ref XenotypeDef __result)
	{
		if (!ModsConfig.BiotechActive)
			return;

		// 排除开局选人，尊重玩家自定义
		if (request.Context == PawnGenerationContext.PlayerStarter)
			return;

		bool isAvaliRace = request.KindDef?.race == AvaliDefOf.RVFFA_Avali;
		bool isAvaliXeno = AvaliDefOf.IsValid(__result);

		if (isAvaliRace && !isAvaliXeno)
		{
			// 正向隔离：Avali 种族抽到了非 Avali 异种，强制修正为随机 Avali 异种
			__result = AvaliDefOf.PickRandom();
		}
		else if (!isAvaliRace && isAvaliXeno)
		{
			// 反向隔离：非 Avali 种族抽到了 Avali 异种，剔除 Avali 基因后按原权重重抽
			__result = AvaliDefOf.RerollNonAvali(request.KindDef, request.Faction);
		}
	}
}

// 兜底：防止 AdjustXenotypeForFactionlessPawn（无派系随机池）或其他 Mod 事件在中途篡改基因
[HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
public static class Patch_GenerateGenes
{
	[HarmonyPriority(Priority.Last)]
	public static void Postfix(Pawn? pawn, PawnGenerationRequest request)
	{
		if (!ModsConfig.BiotechActive || pawn?.genes is null)
			return;

		// 排除：
		// 1. 开局选人
		// 2. 自然生育的新生儿
		if (request.Context == PawnGenerationContext.PlayerStarter || request.AllowedDevelopmentalStages.Newborn())
			return;

		bool isAvaliRace = pawn.def == AvaliDefOf.RVFFA_Avali;
		bool isAvaliXeno = AvaliDefOf.IsValid(pawn.genes.Xenotype);

		if (isAvaliRace && (!isAvaliXeno || pawn.genes.xenotypeName != null))
		{
			// 正向隔离兜底：清除可能被写入的自定义异种名称与图标，强制恢复为合法 Avali 异种
			pawn.genes.xenotypeName = null;
			pawn.genes.iconDef = null;
			pawn.genes.SetXenotype(AvaliDefOf.PickRandom());
		}
		else if (!isAvaliRace && isAvaliXeno)
		{
			// 反向隔离兜底：非 Avali 种族混入了 Avali 异种，按原权重重抽
			pawn.genes.SetXenotype(AvaliDefOf.RerollNonAvali(pawn.kindDef, pawn.Faction));
		}
	}
}
