using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

public static class DisableDLCCheck
{
	public static void Initialize()
	{
		var harmony = new Harmony("RimWorldWrapper.DisableDLCCheck");

		var targetMethod =
			AccessTools.DeclaredMethod(typeof(ModLister), "TryAddMod", [typeof(ModMetaData)])
			?? throw new MissingMethodException(typeof(ModLister).FullName, "TryAddMod");

		var transpiler =
			AccessTools.Method(typeof(DisableDLCCheck), nameof(TranspileTryAddMod))
			?? throw new MissingMethodException(typeof(DisableDLCCheck).FullName, nameof(TranspileTryAddMod));

		harmony.Patch(targetMethod, transpiler: new HarmonyMethod(transpiler));
	}

	public static IEnumerable<CodeInstruction> TranspileTryAddMod(IEnumerable<CodeInstruction> instructions)
	{
		var codes = new List<CodeInstruction>(instructions);

		var officialGetter =
			AccessTools.PropertyGetter(typeof(ModMetaData), nameof(ModMetaData.Official))
			?? throw new MissingMethodException(typeof(ModMetaData).FullName, "get_Official");

		for (int i = 0; i < codes.Count - 1; i++)
		{
			if (codes[i].operand is not MethodInfo method || method != officialGetter)
				continue;

			if (codes[i].opcode != OpCodes.Call && codes[i].opcode != OpCodes.Callvirt)
				continue;

			var branch = codes[i + 1];

			if (branch.opcode != OpCodes.Brfalse && branch.opcode != OpCodes.Brfalse_S)
				continue;

			var pop = new CodeInstruction(OpCodes.Pop);

			pop.labels.AddRange(branch.labels);
			pop.blocks.AddRange(branch.blocks);

			branch.labels.Clear();
			branch.blocks.Clear();

			codes[i + 1] = new CodeInstruction(OpCodes.Br, branch.operand);
			codes.Insert(i + 1, pop);

			return codes;
		}

		throw new InvalidOperationException("找不到检查 DLC 所有权的代码");
	}
}
