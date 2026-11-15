using System.Linq;
using RimWorld;
using Verse;

[DefOf]
internal static class AvaliDefOf
{
	public static ThingDef RVFFA_Avali = null!;
	public static XenotypeDef RVFFA_BaseAvaliXenoType = null!;
	public static XenotypeDef RVFFA_Solfeather = null!;

	static AvaliDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(AvaliDefOf));

	public static bool IsValid(XenotypeDef? xeno) => xeno == RVFFA_BaseAvaliXenoType || xeno == RVFFA_Solfeather;

	/// <summary>
	/// 两个 Avali 基因随机抽取
	/// </summary>
	public static XenotypeDef PickRandom() => Rand.Bool ? RVFFA_BaseAvaliXenoType : RVFFA_Solfeather;

	/// <summary>
	/// 抽取基因，但排除两个 Avali 基因
	/// </summary>
	public static XenotypeDef RerollNonAvali(PawnKindDef? kindDef, Faction? faction)
	{
		if (
			kindDef != null
			&& PawnGenerator
				.XenotypesAvailableFor(kindDef, null, faction)
				.Where(kv => !IsValid(kv.Key))
				.TryRandomElementByWeight(kv => kv.Value, out var chosen)
		)
		{
			return chosen.Key;
		}
		return XenotypeDefOf.Baseliner;
	}
}
