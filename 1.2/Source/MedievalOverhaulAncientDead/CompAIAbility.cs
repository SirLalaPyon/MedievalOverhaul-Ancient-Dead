using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaulAncientDead
{
    public enum AbilityAttackOrder
    {
        Sequence,
        RandomByWeight,
        Random
    }
    public class AbilityAction
    {
        public AbilityDef abilityDef;
        public int cooldownTicks;
        public FloatRange? targetDistanceRange;
        public float weight;
    }
    public class AbilityAIController : DefModExtension
    {
        public AbilityAttackOrder abilityAttackOrder;
        public List<AbilityAction> abilityActions;
    }
    public class CompProperties_AIAbility : CompProperties
    {
        public CompProperties_AIAbility()
        {
            this.compClass = typeof(CompAIAbility);
        }
    }
    public class CompAIAbility : ThingComp
    {
        private Dictionary<AbilityDef, int> cooldownTicks;
        public Pawn Pawn => this.parent as Pawn;

        public AbilityAIController AbilityAIController => Pawn.kindDef.GetModExtension<AbilityAIController>();

        public int lastPerformedAbilityInd;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            var abilityControler = Pawn.kindDef.GetModExtension<AbilityAIController>();
            if (abilityControler != null)
            {
                foreach (var abilityAction in abilityControler.abilityActions)
                {
                    if (Pawn.abilities.GetAbility(abilityAction.abilityDef) is null)
                    {
                        Pawn.abilities.GainAbility(abilityAction.abilityDef);
                    }
                }
            }
        }

        public bool TryGetAbilityToPerformOn(LocalTargetInfo targetInfo, out Ability ability)
        {
            ability = null;
            var abilityControler = AbilityAIController;
            if (abilityControler != null)
            {
                switch (abilityControler.abilityAttackOrder)
                {
                    case AbilityAttackOrder.Sequence: return TryGetAbilityInSequence(targetInfo, out ability);
                    default: return false;
                }
            }
            return false;
        }

        private bool TryGetAbilityInSequence(LocalTargetInfo targetInfo, out Ability ability)
        {
            ability = null;
            var abilityControler = AbilityAIController;
            var abilityAction = abilityControler.abilityActions[lastPerformedAbilityInd];
            var abilityDef = abilityAction.abilityDef;
            if (HasCooldownFor(abilityDef))
            {
                return false;
            }
            if (!CanTarget(abilityAction, targetInfo))
            {
                return false;
            }
            if (lastPerformedAbilityInd >= abilityControler.abilityActions.Count - 1)
            {
                lastPerformedAbilityInd = 0;
            }
            else
            {
                lastPerformedAbilityInd++;
            }
            ability = Pawn.abilities.GetAbility(abilityDef);
            return ability != null;
        }

        private bool CanTarget(AbilityAction abilityAction, LocalTargetInfo targetInfo)
        {
            if (abilityAction.targetDistanceRange.HasValue)
            {
                return abilityAction.targetDistanceRange.Value.Includes(Pawn.Position.DistanceTo(targetInfo.Cell));
            }
            return true;
        }
        public void UseAbilityOn(Ability ability, LocalTargetInfo targetInfo)
        {
            var abilityAction = AbilityAIController.abilityActions.FirstOrDefault(x => x.abilityDef == ability.def);
            lastPerformedAbilityInd = AbilityAIController.abilityActions.IndexOf(abilityAction);
            ability.Activate(targetInfo, targetInfo);
            SetCooldown(ability.def, abilityAction.cooldownTicks);
        }
        public void SetCooldown(AbilityDef key, int toTickCooldown)
        {
            if (cooldownTicks is null)
            {
                cooldownTicks = new Dictionary<AbilityDef, int>();
            }
            cooldownTicks[key] = toTickCooldown;
        }
        public bool HasCooldownFor(AbilityDef key)
        {
            if (cooldownTicks is null)
            {
                cooldownTicks = new Dictionary<AbilityDef, int>();
            }
            if (cooldownTicks.TryGetValue(key, out var tick) && tick > Find.TickManager.TicksGame)
            {
                return true;
            }
            return false;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref cooldownTicks, "cooldownTicks", LookMode.Def, LookMode.Value, ref abilityDefKeys, ref intValues);
            Scribe_Values.Look(ref lastPerformedAbilityInd, "lastPerformedAbilityInd");
        }

        private List<AbilityDef> abilityDefKeys;
        private List<int> intValues;
    }
}