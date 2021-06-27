using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace MedievalOverhaulAncientDead
{
	public class CompAbilityEffect_Horror : CompAbilityEffect_WithDuration
	{
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (target.HasThing)
			{
				base.Apply(target, dest);
				if (Rand.Chance(0.5f))
                {
					Predicate<Pawn> predicate = delegate (Pawn x)
					{
						if (x.story?.traits != null)
						{
							var nerves = x.story.traits.GetTrait(TraitDefOf.Nerves);
							if (nerves != null && nerves.Degree == 2)
                            {
								return false;
                            }
						}
						return true;
					};
					var pawns = GenRadial.RadialDistinctThingsAround(target.Cell, this.parent.pawn.Map, 15f, true).OfType<Pawn>()
						.Where(x => predicate(x) && x.Faction is null || x.Faction.HostileTo(this.parent.pawn.Faction)).Take(2);
					foreach (var pawn in pawns)
					{
						pawn.mindState.mentalStateHandler.TryStartMentalState(MO_DefOf.DankPyon_PanicFlee);
						Log.Message(pawn + " is fleeing");
					}
                }
			}
		}


	}
}
