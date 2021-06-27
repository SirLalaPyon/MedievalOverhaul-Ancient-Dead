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
	public class JobDriver_Flee : JobDriver
	{
		protected const TargetIndex DestInd = TargetIndex.A;

		protected const TargetIndex DangerInd = TargetIndex.B;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.GetTarget(TargetIndex.A).Cell);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.atomicWithPrevious = true;
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			toil.initAction = delegate
			{
				if (pawn.IsColonist)
				{
					MoteMaker.MakeColonistActionOverlay(pawn, MO_DefOf.DankPyon_Mote_ColonistFleeing);
				}
			};
			yield return toil;
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		}
	}
}
