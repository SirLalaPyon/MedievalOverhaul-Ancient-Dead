using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaulAncientDead
{

    public class Building_AncientSarcophagus : Building_Sarcophagus
    {
        public Pawn Ancient
        {
            get
            {
                var thing = this.ContainedThing;
                if (thing is Pawn pawn && pawn.def == MO_DefOf.DankPyon_AncientDeadRace)
                {
                    return pawn;
                }
                else if (thing is Corpse corpse && corpse.InnerPawn.def == MO_DefOf.DankPyon_AncientDeadRace)
                {
                    return corpse.InnerPawn;
                }
                return null;
            }
        } 
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (HasCorpse && Ancient is null)
            {
                if (base.Tile != -1)
                {
                    string value = GenDate.DateFullStringAt(GenDate.TickGameToAbs(Corpse.timeOfDeath), Find.WorldGrid.LongLatOf(base.Tile));
                    stringBuilder.AppendLine();
                    stringBuilder.Append("DiedOn".Translate(value));
                }
            }
            else if (AssignedPawn != null)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("AssignedColonist".Translate());
                stringBuilder.Append(": ");
                stringBuilder.Append(AssignedPawn.LabelCap);
            }
            return stringBuilder.ToString();
        }
        public override bool Accepts(Thing thing)
        {
            if (thing is Pawn pawn)
            {
                if (pawn.def == MO_DefOf.DankPyon_AncientDeadRace)
                {
                    return true;
                }
            }
            else if (thing is Corpse corpse)
            {
                if (corpse.InnerPawn.def == MO_DefOf.DankPyon_AncientDeadRace)
                {
                    return true;
                }
            }
            return base.Accepts(thing);
        }
    }
}
