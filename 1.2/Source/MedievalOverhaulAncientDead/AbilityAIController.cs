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
        public bool dontEngangeInMelee;
    }
}