using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;

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
            MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Silver, Rand.Range(10, 50));
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
            GenerateFriendlyAncient_Patch.GiveRandomLootInventoryForTombPawn(pawn);
            __result = pawn;
            return false;
        }
    }

    [HarmonyPatch(typeof(ThingSetMaker_MapGen_AncientPodContents), "GenerateScarabs")]
    public static class GenerateScarabs_Patch
    {
        public static bool Prefix(ref List<Thing> __result)
        {
            __result = new List<Thing>();
            int num = Rand.Range(3, 6);
            for (int i = 0; i < num; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(MO_DefOf.DankPyon_AncientAuxiliary,
                    Find.FactionManager.FirstFactionOfDef(MO_DefOf.DankPyon_AncientDeadFaction), PawnGenerationContext.NonPlayer, -1,
                    forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: true, 1f,
                    forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true));
                __result.Add(pawn);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ThingSetMaker_MapGen_AncientPodContents), "GenerateSlave")]
    public static class GenerateSlave_Patch
    {
        public static bool Prefix(ref Pawn __result)
        {
            __result = PawnGenerator.GeneratePawn(new PawnGenerationRequest(MO_DefOf.DankPyon_AncientAuxiliary,
                Find.FactionManager.FirstFactionOfDef(MO_DefOf.DankPyon_AncientDeadFaction), PawnGenerationContext.NonPlayer, -1,
                forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: true, 1f,
                forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true));
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

    [HarmonyPatch(typeof(SymbolResolver_SinglePawn), "Resolve")]
    public static class Resolve_SinglePawn_Patch
    {
        public static bool Prefix(SymbolResolver_SinglePawn __instance, ResolveParams rp)
        {
            if (rp.faction == Faction.OfMechanoids)
            {
                Log.Message("Preventing SymbolResolver_SinglePawn");
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SymbolResolver_AncientCryptosleepCasket), "Resolve")]
    public static class Resolve_SymbolResolver_AncientCryptosleepCasket_Patch
    {
        public static bool Prefix(SymbolResolver_AncientCryptosleepCasket __instance, ResolveParams rp)
        {
            int groupID = rp.ancientCryptosleepCasketGroupID ?? Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID();

            PodContentsType value = rp.podContentsType ?? Gen.RandomEnumValue<PodContentsType>(disallowFirstValue: true);
            Rot4 rot = rp.thingRot ?? Rot4.North;
            Building_AncientSarcophagus building_AncientCryptosleepCasket = (Building_AncientSarcophagus)ThingMaker.MakeThing(MO_DefOf.DankPyon_AncientSarcophagus);
            building_AncientCryptosleepCasket.groupID = groupID;
            ThingSetMakerParams parms = default(ThingSetMakerParams);
            parms.podContentsType = value;
            List<Thing> list = ThingSetMakerDefOf.MapGen_AncientPodContents.root.Generate(parms);
            for (int i = 0; i < list.Count; i++)
            {
                if (!building_AncientCryptosleepCasket.TryAcceptThing(list[i], allowSpecialEffects: false))
                {
                    GenPlace.TryPlaceThing(list[i], rp.rect.RandomCell, BaseGen.globalSettings.map, ThingPlaceMode.Near);
                }
            }
            var faction = Find.FactionManager.FirstFactionOfDef(MO_DefOf.DankPyon_AncientDeadFaction);
            GenSpawn.Spawn(building_AncientCryptosleepCasket, rp.rect.RandomCell, BaseGen.globalSettings.map, rot);
            var pawns = list.OfType<Pawn>().Where(x => x.Faction == faction);
            if (pawns.Any())
            {
                bool foundLord = false;
                var lords = BaseGen.globalSettings.map.lordManager.lords.Where(x => x.LordJob is LordJob_DefendPoint defend && x.faction == faction);
                if (lords.Any())
                {
                    foreach (var lord in lords)
                    {
                        var defend = lord.LordJob as LordJob_DefendPoint;
                        var point = Traverse.Create(defend).Field("point").GetValue<IntVec3>();
                        if (rp.rect.Cells.Any(y => y.GetRoom(BaseGen.globalSettings.map) == point.GetRoom(BaseGen.globalSettings.map)))
                        {
                            AssignPawns(lord, pawns);
                            foundLord = true;
                        }
                        else
                        {
                            Log.Message("Didn't found a point for " + lord.LordJob + " - " + point);
                        }
                    }
                }
                if (!foundLord)
                {
                    var point = rp.rect.RandomElement();
                    LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(point, 6, false, false), BaseGen.globalSettings.map, pawns);
                }
            }
            return false;
        }

        private static void AssignPawns(Lord lord, IEnumerable<Pawn> pawns)
        {
            foreach (var pawn in pawns)
            {
                lord.AddPawn(pawn);
            }
        }
    }
    [HarmonyPatch(typeof(SymbolResolver_Interior_AncientTemple), "Resolve")]
    public static class SymbolResolver_Interior_AncientTemple_Patch
    {
        public static bool preventHives;
        public static void Prefix()
        {
            preventHives = true;
        }
        public static void Postfix()
        {
            preventHives = false;
        }
    }

    [HarmonyPatch(typeof(SymbolStack), "Push", new Type[] {typeof(string), typeof(ResolveParams), typeof(string) })]
    public static class SymbolStack_Patch
    {
        public static bool Prefix(string symbol, ResolveParams resolveParams, string customNameForPath = null)
        {
            if (symbol == "hives" && SymbolResolver_Interior_AncientTemple_Patch.preventHives)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    public static class SpawnSetup_Patch
    {
        public static void Postfix(Pawn __instance)
        {
            if (__instance.def == MO_DefOf.DankPyon_AncientDeadRace)
            {
                __instance.skills.GetSkill(SkillDefOf.Melee).levelInt = 12;
                __instance.skills.GetSkill(SkillDefOf.Shooting).levelInt = 12;
                __instance.story.adulthood = null;
                __instance.story.traits.allTraits.Clear();
            }
        }
    }

    [HarmonyPatch(typeof(ITab_Pawn_Character), "IsVisible", MethodType.Getter)]
    public static class IsVisible_Patch
    {
        public static void Postfix(ITab_Pawn_Character __instance, ref bool __result)
        {
            var pawn = PawnToShowInfoAbout();
            if (pawn != null && pawn.def == MO_DefOf.DankPyon_AncientDeadRace)
            {
                __result = false;
            }
        }

        private static Pawn PawnToShowInfoAbout()
        {
            Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
            if (pawn is null && Find.Selector.SingleSelectedThing is Corpse corpse)
            {
                if (corpse != null)
                {
                    pawn = corpse.InnerPawn;
                }
            }
            if (pawn == null)
            {
                return null;
            }
            return pawn;
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class AddHumanlikeOrders_Patch
    {
        public static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            if (pawn.equipment != null)
            {
                List<Thing> thingList = c.GetThingList(pawn.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] is Pawn victim && victim.def == MO_DefOf.DankPyon_AncientDeadRace)
                    {
                        TaggedString toCheck = "Capture".Translate(victim.LabelCap, victim);
                        FloatMenuOption floatMenuOption = opts.FirstOrDefault((FloatMenuOption x) => x.Label.Contains(toCheck));
                        if (floatMenuOption != null)
                        {
                            opts.Remove(floatMenuOption);
                        }
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(Building_Grave), "HasCorpse", MethodType.Getter)]
    public static class HasCorpse_Patch
    {
        public static void Postfix(Building_Grave __instance, ref bool __result)
        {
            if (__instance is Building_AncientSarcophagus ancientSarcophagus && ancientSarcophagus.Ancient != null)
            {
                __result = true;
            }
        }
    }

}
