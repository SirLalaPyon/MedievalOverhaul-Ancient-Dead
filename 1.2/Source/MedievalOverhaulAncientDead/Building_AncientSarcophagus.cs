using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

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
            if (ContainedThing is null)
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
            }
            return base.Accepts(thing);
        }

        public override void Open()
        {
            base.Open();
            Log.Message("TEST");
        }
        public override void EjectContents()
        {
            bool contentsKnown = base.contentsKnown;
            List<Thing> list = null;
            if (!contentsKnown)
            {
                list = new List<Thing>();
                list.AddRange(innerContainer);
                list.AddRange(UnopenedCasketsInGroup().SelectMany((Building_AncientSarcophagus c) => c.innerContainer));
                list.RemoveDuplicates();
            }
            Log.Message("2 TEST: " + contentsKnown);
            base.EjectContents();
            Log.Message("2 TEST: " + contentsKnown);
            if (!contentsKnown)
            {
                SetFaction(null);
                foreach (Building_AncientSarcophagus item in UnopenedCasketsInGroup())
                {
                    item.contentsKnown = true;
                    item.EjectContents();
                }
                var faction = Find.FactionManager.FirstFactionOfDef(MO_DefOf.DankPyon_AncientDeadFaction);
                IEnumerable<Pawn> enumerable = from p in list.OfType<Pawn>().ToList() where p.def == MO_DefOf.DankPyon_AncientDeadRace 
                                               && p.GetLord() == null && p.Faction == faction select p;
                if (enumerable.Any())
                {
                    LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(this.Position, 6, false, false), this.Map, enumerable);
                }
            }
        }

        private IEnumerable<Building_AncientSarcophagus> UnopenedCasketsInGroup()
        {
            yield return this;
            if (groupID != -1)
            {
                foreach (Thing item in base.Map.listerThings.ThingsOfDef(this.def))
                {
                    Building_AncientSarcophagus building_AncientCryptosleepCasket = item as Building_AncientSarcophagus;
                    if (building_AncientCryptosleepCasket.groupID == groupID && !building_AncientCryptosleepCasket.contentsKnown)
                    {
                        yield return building_AncientCryptosleepCasket;
                    }
                }
            }
        }


        public int groupID = -1;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref groupID, "groupID", 0);
        }
    }
}
