using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaulAncientDead
{
    public class Gas_GreenFog : Gas
    {
        public override void Tick()
        {
            base.Tick();
            if (this.Map != null && Find.TickManager.TicksGame % Rand.RangeInclusive(40, 80) == 0)
            {
                foreach (var pos in GenRadial.RadialCellsAround(this.Position, 5f, true))
                {
                    if (GenGrid.InBounds(pos, this.Map))
                    {
                        var list = this.Map.thingGrid.ThingsListAt(pos);
                        for (int num = list.Count - 1; num >= 0; num--)
                        {
                            if (list[num] is Pawn pawn && !pawn.Dead && pawn.RaceProps.IsFlesh)
                            {
                                pawn.TakeDamage(new DamageInfo(MO_DefOf.DankPyon_ToxicBurn, 1f));
                            }
                            else if (list[num] is Plant plant)
                            {
                                plant.TakeDamage(new DamageInfo(MO_DefOf.DankPyon_ToxicBurn, 1f));
                            }
                        }
                    }
                }
            }
        }
    }
}
