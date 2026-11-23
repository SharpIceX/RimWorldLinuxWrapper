using System.Linq;
using LudeonTK;
using Verse;
using Verse.AI;

public static class ThreatPrinterTool
{
	[DebugAction(
		"General",
		"Print Map Threats",
		actionType = DebugActionType.Action,
		allowedGameStates = AllowedGameStates.PlayingOnMap
	)]
	public static void PrintMapThreats()
	{
		Map map = Find.CurrentMap;
		var threats = map.attackTargetsCache.TargetsHostileToColony.Where(t =>
			t?.Thing != null && !t.ThreatDisabled(null)
		);

		Log.Message(
			$"[ThreatPrinter] 地图危险度: {map.dangerWatcher.DangerRating} | 有效敌对目标数: {threats.Count()}"
		);

		foreach (IAttackTarget target in threats)
		{
			Thing thing = target.Thing;
			Log.Message(
				$"[ThreatPrinter] -> 实体: {thing.LabelCap} | Def: {thing.def.defName} | 派系: {thing.Faction?.Name ?? "无"} | 坐标: {thing.Position}"
			);
		}
	}
}
