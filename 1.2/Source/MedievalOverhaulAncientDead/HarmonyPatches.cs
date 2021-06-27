using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    [HarmonyPatch(typeof(SymbolResolver_AncientCryptosleepCasket), "Resolve")]
    public static class Resolve_Patch
    {
        public static void Prefix(out ThingDef __state)
        {
            __state = ThingDefOf.AncientCryptosleepCasket;
            ThingDefOf.AncientCryptosleepCasket = MO_DefOf.DankPyon_AncientSarcophagus;
        }
        public static void Postfix(ThingDef __state)
        {
            ThingDefOf.AncientCryptosleepCasket = __state;
        }
    }

    [HarmonyPatch(typeof(ThingSetMaker_MapGen_AncientPodContents), "GenerateFriendlyAncient")]
    public static class GenerateFriendlyAncient_Patch
    {
        public static bool Prefix(ref Pawn __result)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Rand.Bool ? MO_DefOf.DankPyon_AncientLegionary : MO_DefOf.DankPyon_AncientAuxiliary, 
                Find.FactionManager.FirstFactionOfDef(MO_DefOf.DankPyon_AncientDeadFaction), PawnGenerationContext.NonPlayer, -1, 
                forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: true, 1f, 
                forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true));
            GiveRandomLootInventoryForTombPawn(pawn);
            __result = pawn;
            return false;
        }

        public static void GiveRandomLootInventoryForTombPawn(Pawn p)
        {
            if (Rand.Value < 0.65f)
            {
                MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Gold, Rand.Range(10, 50));
            }
            else
            {
                MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Plasteel, Rand.Range(10, 50));
            }
            if (Rand.Value < 0.7f)
            {
                MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.ComponentIndustrial, Rand.Range(-2, 4));
            }
            else
            {
                MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.ComponentSpacer, Rand.Range(-2, 4));
            }
        }

        public static void MakeIntoContainer(ThingOwner container, ThingDef def, int count)
        {
            if (count > 0)
            {
                Thing thing = ThingMaker.MakeThing(def);
                thing.stackCount = count;
                container.TryAdd(thing);
            }
        }
    }

    [HarmonyPatch(typeof(ThingSetMaker_MapGen_AncientPodContents), "GenerateIncappedAncient")]
    public static class GenerateIncappedAncient_Patch
    {
        public static bool Prefix(ref Pawn __result)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Rand.Bool ? MO_DefOf.DankPyon_AncientLegionary : MO_DefOf.DankPyon_AncientAuxiliary,
                Find.FactionManager.FirstFactionOfDef(MO_DefOf.DankPyon_AncientDeadFaction), PawnGenerationContext.NonPlayer, -1,
                forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: true, 1f,
                forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true));
            HealthUtility.DamageUntilDowned(pawn);
            GenerateFriendlyAncient_Patch.GiveRandomLootInventoryForTombPawn(pawn);
            __result = pawn;
            return false;
        }
    }

    [HarmonyPatch(typeof(ThingSetMaker_MapGen_AncientPodContents), "GenerateAngryAncient")]
    public static class GenerateAngryAncient_Patch
    {
        public static bool Prefix(ref Pawn __result)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Rand.Bool ? MO_DefOf.DankPyon_AncientLegionary : MO_DefOf.DankPyon_AncientAuxiliary,
                Find.FactionManager.FirstFactionOfDef(MO_DefOf.DankPyon_AncientDeadFaction), PawnGenerationContext.NonPlayer, -1,
                forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: true, 1f,
                forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true));
            GenerateFriendlyAncient_Patch.GiveRandomLootInventoryForTombPawn(pawn);
            __result = pawn;
            return false;
        }
    }

    [HarmonyPatch(typeof(ThingSetMaker_MapGen_AncientPodContents), "GenerateHalfEatenAncient")]
    public static class GenerateHalfEatenAncient_Patch
    {
        public static bool Prefix(ref Pawn __result)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Rand.Bool ? MO_DefOf.DankPyon_AncientLegionary : MO_DefOf.DankPyon_AncientAuxiliary,
                Find.FactionManager.FirstFactionOfDef(MO_DefOf.DankPyon_AncientDeadFaction), PawnGenerationContext.NonPlayer, -1,
                forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: true, 1f,
                forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true));
            int num = Rand.Range(6, 10);
            for (int i = 0; i < num; i++)
            {
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Bite, Rand.Range(3, 8), 0f, -1f, pawn));
            }
            GenerateFriendlyAncient_Patch.GiveRandomLootInventoryForTombPawn(pawn);
            __result = pawn;
            return false;
        }
    }

    [HarmonyPatch]
    public class MechSpawn_Patch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(SymbolResolver_RandomMechanoidGroup).GetNestedTypes(AccessTools.all)
                                                                 .First(t => t.GetMethods(AccessTools.all)
                                                                           .Any(mi => mi.ReturnType == typeof(bool) && mi.GetParameters()[0].ParameterType == typeof(PawnKindDef)))
                                                                 .GetMethods(AccessTools.all)
                                                                 .First(mi => mi.ReturnType == typeof(bool));

            yield return typeof(MechClusterGenerator).GetNestedTypes(AccessTools.all).MaxBy(t => t.GetMethods(AccessTools.all).Length).GetMethods(AccessTools.all)
                                                  .First(mi => mi.ReturnType == typeof(bool) && mi.GetParameters()[0].ParameterType == typeof(PawnKindDef));
        }

        [HarmonyPostfix]
        public static void Postfix(PawnKindDef __0, ref bool __result)
        {
            __result = !__0.RaceProps.IsMechanoid;
        }
    }
}
