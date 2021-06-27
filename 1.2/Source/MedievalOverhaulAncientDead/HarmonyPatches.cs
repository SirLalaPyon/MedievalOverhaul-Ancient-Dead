using HarmonyLib;
using RimWorld;
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
            foreach (var humanlike in DefDatabase<ThingDef>.AllDefs.Where(x => x.race?.Humanlike ?? false))
            {
                if (humanlike.comps is null)
                {
                    humanlike.comps = new List<CompProperties>();
                }
                humanlike.comps.Add(new CompProperties_AIAbility());
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "TryGetAttackVerb")]
    public static class Patch_TryGetAttackVerb
    {
        public static void Postfix(ref Verb __result, Pawn __instance, Thing target, bool allowManualCastWeapons = false)
        {
            var abilityControl = __instance.kindDef.GetModExtension<AbilityAIController>();
            var comp = __instance.TryGetComp<CompAIAbility>();
            if (abilityControl != null && comp != null && target != null)
            {
                if (comp.TryGetAbilityToPerformOn(target, out var ability))
                {
                    comp.UseAbilityOn(ability, target);
                }

                if (__result != null)
                {
                    if (__result.IsMeleeAttack && abilityControl.dontEngangeInMelee && __instance.Position.DistanceTo(target.Position) > 2)
                    {
                        __result = null;
                    }
                }
            }


        }
    }

    [HarmonyPatch(typeof(RaceProperties), "IsFlesh", MethodType.Getter)]
    public static class IsFlesh_Patch
    {
        public static void Postfix(RaceProperties __instance, ref bool __result)
        {
            if (__instance.FleshType == MO_DefOf.DankPyon_AncientDead)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_StoryTracker), "TitleShort", MethodType.Getter)]
    public static class TitleShort_Patch
    {
        public static void Postfix(Pawn ___pawn, ref string __result)
        {
            if (___pawn.def == MO_DefOf.DankPyon_AncientDeadRace)
            {
                __result = "";
            }
        }
    }

    [HarmonyPatch(typeof(PawnComponentsUtility), "CreateInitialComponents")]
    public static class CreateInitialComponents_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.def == MO_DefOf.DankPyon_AncientDeadRace)
            {
                if (pawn.relations == null)
                {
                    pawn.relations = new Pawn_RelationsTracker(pawn);
                }
                if (pawn.psychicEntropy == null)
                {
                    pawn.psychicEntropy = new Pawn_PsychicEntropyTracker(pawn);
                }
                pawn.skills.GetSkill(SkillDefOf.Melee).levelInt = 12;
                pawn.skills.GetSkill(SkillDefOf.Shooting).levelInt = 12;
                pawn.story.adulthood = null;
            }
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


    [HarmonyPatch(typeof(IndividualThoughtToAdd), "Add")]
    public static class Add_Patch
    {
        public static bool Prefix(IndividualThoughtToAdd __instance, Pawn ___otherPawn)
        {
            if (__instance.addTo.def == MO_DefOf.DankPyon_AncientDeadRace)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SituationalThoughtHandler), "TryCreateThought")]
    public static class TryCreateThought_Patch
    {
        public static bool Prefix(SituationalThoughtHandler __instance, Thought_Situational __result, ThoughtDef def)
        {
            if (__instance.pawn.def == MO_DefOf.DankPyon_AncientDeadRace)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SituationalThoughtHandler), "TryCreateSocialThought")]
    public static class TryCreateSocialThought_Patch
    {
        public static bool Prefix(SituationalThoughtHandler __instance, Thought_SituationalSocial __result, ThoughtDef def, Pawn otherPawn)
        {
            if (__instance.pawn.def == MO_DefOf.DankPyon_AncientDeadRace)
            {
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", new Type[]
    {
        typeof(Thought_Memory),
        typeof(Pawn)
    })]
    public static class TryGainMemory_Patch
    {
        private static bool Prefix(MemoryThoughtHandler __instance, ref Thought_Memory newThought, Pawn otherPawn)
        {
            if (__instance.pawn.def == MO_DefOf.DankPyon_AncientDeadRace)
            {
                return false;
            }
            return true;
        }
    }
}
