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
    public static class MO_Utils
    {
		public static Job JobFlee(Pawn pawn, Thing danger, int radius, List<Thing> dangers)
		{
			Job job = null;
			if (pawn.CurJobDef != JobDefOf.Flee)
            {
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
				}
			}
			return job;
		}
	}
}
