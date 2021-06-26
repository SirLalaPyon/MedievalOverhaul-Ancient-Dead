using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaulAncientDead
{
	public class CompAbilityEffect_SpawnGreenFog : CompAbilityEffect_WithDuration
	{
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (target.HasThing)
			{
				base.Apply(target, dest);
				GenSpawn.Spawn(MO_DefOf.DankPyon_GreenFog, target.Cell, this.parent.pawn.Map);
			}
		}
	}
}
