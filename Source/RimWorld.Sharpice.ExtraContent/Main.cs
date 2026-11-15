using System.Reflection;
using HarmonyLib;
using Verse;

public class Main : Mod
{
	public Main(ModContentPack content)
		: base(content)
	{
		Log.Message("Hello RimWorld Wrapper Extra Content Mod!");

		Log.Message("RimWorld Wrapper Fix Extra Content Load Done");
	}
}
