using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace GliterworldUprising
{
    public class CompProperties_DesensitizingModule : CompProperties
    {
        public ActivateGizmo activateGizmo;
        public int fuelPerDesensitization;
        public FleckDef fleck;
        public ThingDef mote;
        public int moteCount = 3;
        public FloatRange moteOffsetRange = new FloatRange(0.2f, 0.4f);
        public CompProperties_DesensitizingModule() => this.compClass = typeof(CompDesensitizingModule);
    }

    public class ActivateGizmo
    {
        public Texture2D tex;
        public string texPath;
        public string labelKey;
        public string descKey;
    }


    [StaticConstructorOnStartup]
    public class CompDesensitizingModule : ThingComp
    {
        Map map;
        private Command_Action activateAction;

        public CompProperties_DesensitizingModule Props => (CompProperties_DesensitizingModule)this.props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            map = this.parent.Map;

            activateAction = new Command_Action();
            activateAction.defaultLabel = (string)Translator.Translate(this.Props.activateGizmo.labelKey);
            activateAction.defaultDesc = (string)Translator.Translate(this.Props.activateGizmo.descKey);
            activateAction.icon = this.Props.activateGizmo.tex;
            activateAction.action = (Action)(() => this.desensitizeAll());
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            Command_Action activate = activateAction;
            yield return (Gizmo)activate;
            activate = (Command_Action)null;
        }

        public void desensitizeAll()
        {
            if (this.parent.GetComp<CompRefuelable>().Fuel >= this.Props.fuelPerDesensitization && this.parent.GetComp<CompPowerTrader>().PowerOn)
            {
                foreach (Thing building in this.parent.GetComp<CompFacility>().LinkedBuildings)
                {
                    foreach (Thing thing in map.thingGrid.ThingsAt(building.Position))
                    {
                        if (thing is Pawn pawn)
                        {
                            List<Hediff> allHediffs = new List<Hediff>();
                            pawn.health.hediffSet.GetHediffs(ref allHediffs);
                            foreach (Hediff hediff in allHediffs)
                            {
                                if (hediff.def.defName == "Anesthetic")
                                {

                                    //Remove anesthetic hediff
                                    pawn.health.RemoveHediff(hediff);

                                    //Spawn particles
                                    if (this.Props.mote != null || this.Props.fleck != null)
                                    {
                                        Vector3 drawPos = this.parent.DrawPos;
                                        for (int index = 0; index < this.Props.moteCount; ++index)
                                        {
                                            Vector2 vector2 = Rand.InsideUnitCircle * this.Props.moteOffsetRange.RandomInRange * (float)Rand.Sign;
                                            Vector3 loc = new Vector3(drawPos.x + vector2.x, drawPos.y, drawPos.z + vector2.y);
                                            if (this.Props.mote != null)
                                                MoteMaker.MakeStaticMote(loc, this.map, this.Props.mote);
                                            else
                                                FleckMaker.Static(loc, this.map, this.Props.fleck);
                                        }
                                    }

                                    //Consume fuel
                                    this.parent.GetComp<CompRefuelable>().ConsumeFuel(this.Props.fuelPerDesensitization);
                                }
                            }
                        }
                    }
                }
            }
        }


        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this != null)
            {
                stringBuilder.Append((string)"USH_GU_DesensitizeCost".Translate() + ": " + this.Props.fuelPerDesensitization.ToString());
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString().TrimEnd();
        }
    }
}
