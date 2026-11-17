using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace RimWorld.Sharpice.FixMod.Patches.VEF;

[HarmonyPatch]
public static class VEF_PawnRenderNode_Fur_NullFurDef
{
	private static readonly MethodInfo? Target = AccessTools.Method(
		"VEF.Genes.VanillaExpandedFramework_PawnRenderNode_Fur_GraphicFor_Patch:Postfix"
	);

	private static readonly FieldInfo? StoryField = AccessTools.Field(typeof(Pawn), "story");

	private static readonly FieldInfo? FurDefField = StoryField is null
		? null
		: AccessTools.Field(StoryField.FieldType, "furDef");

	internal static bool Prepare() =>
		Target is not null
		&& Target.GetParameters() is [_, { ParameterType: var pawnType }, ..]
		&& pawnType == typeof(Pawn)
		&& StoryField is not null
		&& FurDefField is not null;

	internal static MethodBase TargetMethod() => Target!;

	internal static IEnumerable<CodeInstruction> Transpiler(
		IEnumerable<CodeInstruction> instructions,
		ILGenerator generator
	)
	{
		var codes = instructions.ToList();
		var ret = codes.FindLastIndex(x => x.opcode == OpCodes.Ret);

		if (ret < 0)
		{
			Log.Error("Patch_VEF_PawnRenderNode_Fur_NullFurDef 无法找到返回指令 (ret)");
			return codes;
		}

		var returnLabel = generator.DefineLabel();
		codes[ret].labels.Add(returnLabel);

		var guard = new[]
		{
			// pawn == null
			new CodeInstruction(OpCodes.Ldarg_1),
			new CodeInstruction(OpCodes.Brfalse, returnLabel),
			// pawn.story == null

			new CodeInstruction(OpCodes.Ldarg_1),
			new CodeInstruction(OpCodes.Ldfld, StoryField!),
			new CodeInstruction(OpCodes.Brfalse, returnLabel),
			// pawn.story.furDef == null
			new CodeInstruction(OpCodes.Ldarg_1),
			new CodeInstruction(OpCodes.Ldfld, StoryField!),
			new CodeInstruction(OpCodes.Ldfld, FurDefField!),
			new CodeInstruction(OpCodes.Brfalse, returnLabel),
		};

		// 保留原有的入口的标签和异常块边界
		guard[0].labels.AddRange(codes[0].labels);
		guard[0].blocks.AddRange(codes[0].blocks);

		codes[0].labels.Clear();
		codes[0].blocks.Clear();

		return guard.Concat(codes);
	}
}
