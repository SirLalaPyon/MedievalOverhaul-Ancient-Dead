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
    public class JobGiver_Flee : ThinkNode_JobGiver
    {
		protected override Job TryGiveJob(Pawn pawn)
		{
			var enemies = GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, 15f, true).OfType<Pawn>().Where(x => x.HostileTo(pawn));
			if (enemies.Any())
            {
				return MO_Utils.JobFlee(pawn, enemies.First(), 50, enemies.Cast<Thing>().ToList());
            }
			return null;
		}

	}
}
