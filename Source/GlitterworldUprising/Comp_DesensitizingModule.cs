using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using System.Text;

namespace GliterworldUprising
{
    [StaticConstructorOnStartup]
    public class Comp_DesensitizingModule : ThingComp
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
                            foreach (Hediff hediff in pawn.health.hediffSet.GetHediffs<Hediff>())
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
