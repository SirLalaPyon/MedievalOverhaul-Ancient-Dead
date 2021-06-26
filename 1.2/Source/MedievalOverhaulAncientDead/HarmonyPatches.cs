using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaulAncientDead
{
    [StaticConstructorOnStartup]
    internal static class HarmonyContainer
    {
        public static Harmony harmony;
        static HarmonyContainer()
        {
            harmony = new Harmony("MedievalOverhaulAncientDead.Mod");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(HediffSet), "CalculatePain")]
    public class Patch_CalculatePain
    {
        private static void Postfix(HediffSet __instance, ref float __result)
        {
            if (__instance.pawn.def == MO_DefOf.DankPyon_AncientDeadRace)
            {
                __result = 0f;
            }
        }
    }
}
