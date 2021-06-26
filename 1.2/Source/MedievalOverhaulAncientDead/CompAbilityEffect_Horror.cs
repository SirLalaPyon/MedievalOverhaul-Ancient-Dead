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
						if (x.story.traits != null)
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
						.Where(x => predicate(x) && x.Faction.HostileTo(this.parent.pawn.Faction)).Take(2);
					foreach (var pawn in pawns)
					{
						MakeFlee(pawn, this.parent.pawn, 25, new List<Thing> { this.parent.pawn });
					}
                }
			}
		}

		public void MakeFlee(Pawn pawn, Thing danger, int radius, List<Thing> dangers)
		{
			Job job = null;
			IntVec3 intVec;
			if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Flee)
			{
				intVec = pawn.CurJob.targetA.Cell;
			}
			else
			{
				intVec = CellFinderLoose.GetFleeDest(pawn, dangers, 24f);
			}

			if (intVec == pawn.Position)
			{
				intVec = GenRadial.RadialCellsAround(pawn.Position, radius, radius * 2).RandomElement();
			}
			if (intVec != pawn.Position)
			{
				job = JobMaker.MakeJob(JobDefOf.Flee, intVec, danger);
			}
			if (job != null)
			{
				job.expiryInterval = 5 * 60;
				pawn.jobs.TryTakeOrderedJob(job);
			}
		}
	}
}
