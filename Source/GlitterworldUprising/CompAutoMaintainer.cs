using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace GliterworldUprising
{
    public class PlaceWorker_ShowAutoMaintainerRadius : PlaceWorker
    {
        public override void DrawGhost(
          ThingDef def,
          IntVec3 center,
          Rot4 rot,
          Color ghostCol,
          Thing thing = null)
        {
            CompProperties_AutoMaintainer compProperties = def.GetCompProperties<CompProperties_AutoMaintainer>();
            if (compProperties == null)
                return;
            GenDraw.DrawRadiusRing(center, compProperties.radius);
        }
    }

    public class CompProperties_AutoMaintainer : CompProperties
    {
        public float radius;
        public int rareTickPerCheck, rareTicksPerMaintain;
        public ThingDef moteDef;
        public CompProperties_AutoMaintainer() => this.compClass = typeof(CompAutoMaintainer);
    }

    public class CompAutoMaintainer : ThingComp
    {
        Map map;
        private int rareTicksBeforeCheck, rareTicksBeforeMaintain, maintainedAlready;
        private static HashSet<Thing> maintainables = new HashSet<Thing>();
        private static List<Thing> brokenThings = new List<Thing>();

        public CompProperties_AutoMaintainer Props => (CompProperties_AutoMaintainer)this.props;


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            map = this.parent.Map;
            CollectDataForMaintain(false);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();

            rareTicksBeforeCheck++;
            if (rareTicksBeforeCheck == this.Props.rareTickPerCheck)
            {
                rareTicksBeforeCheck = 0;
                CollectDataForMaintain(false);
            }

            rareTicksBeforeMaintain++;
            if (rareTicksBeforeMaintain == this.Props.rareTicksPerMaintain)
            {
                rareTicksBeforeMaintain = 0;
                checkForBroken();
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            CompAutoMaintainer compAutoMaintainer = this;

            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;
            if (DesignatorUtility.FindAllowedDesignator<Designator_ZoneAddStockpile_Resources>() != null)
            {
                Command_Action commandAction = new Command_Action();
                commandAction.action = (Action)(() => this.CollectDataForMaintain(true));
                commandAction.defaultLabel = (string)"USH_GU_FindBreakdownablesLabel".Translate();
                commandAction.defaultDesc = (string)"USH_GU_FindBreakdownablesDesc".Translate();
                commandAction.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/CollectData");
                yield return (Gizmo)commandAction;

                Command_Action commandAction1 = new Command_Action();
                commandAction1.action = (Action)(() => this.CollectAndMaintain());
                commandAction1.defaultLabel = (string)"USH_GU_MaintainNowLabel".Translate();
                commandAction1.defaultDesc = (string)"USH_GU_MaintainNowDesc".Translate();
                commandAction1.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/MaintainNowIcon");
                yield return (Gizmo)commandAction1;
            }
        }

        private void CollectDataForMaintain(bool shouldFlashCells)
        {
            maintainables.Clear();
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.parent.Position, this.Props.radius, true))
            {
                foreach (Thing thing in this.map.thingGrid.ThingsAt(cell))
                {
                    if (thing != null)
                    {
                        if (thing.TryGetComp<CompBreakdownable>() != null)
                        {
                            maintainables.Add(thing);
                            if (shouldFlashCells)
                                map.debugDrawer.FlashCell(thing.Position, 1, null, 150);
                        }
                    }
                }
            }
        }

        private void checkForBroken()
        {
            brokenThings.Clear();
            foreach (Thing thing in maintainables)
            {
                if (thing.TryGetComp<CompBreakdownable>().BrokenDown)
                {
                    brokenThings.Add(thing);
                }
            }
            if (brokenThings.Count > 0)
            {
                MaintainThings();
            }
        }

        private void MaintainThings()
        {
            foreach (Thing thing in brokenThings)
            {
                if (this.parent.GetComp<CompPowerTrader>().PowerOn)
                {
                    thing.TryGetComp<CompBreakdownable>().Notify_Repaired();
                    throwMote(thing);
                    maintainedAlready++;
                }
            }
            CollectDataForMaintain(false);
        }

        private void CollectAndMaintain()
        {
            CollectDataForMaintain(false);
            MaintainThings();
        }

        private void throwMote(Thing thing)
        {
            MoteThrown newThing = (MoteThrown)ThingMaker.MakeThing(this.Props.moteDef);
            newThing.Scale = 0.8f;
            newThing.rotationRate = (float)Rand.Range(-1, 1);
            newThing.exactPosition = thing.Position.ToVector3();
            MoteThrown moteThrown1 = newThing;
            moteThrown1.exactPosition = moteThrown1.exactPosition + new Vector3(0.85f, 0.0f, 0.85f);
            MoteThrown moteThrown2 = newThing;
            moteThrown2.exactPosition = moteThrown2.exactPosition + new Vector3(Rand.Value, 0.0f, Rand.Value) * 0.1f;
            newThing.SetVelocity(Rand.Range(30f, 60f), Rand.Range(0.35f, 0.55f));
            GenSpawn.Spawn((Thing)newThing, thing.Position, map);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this != null)
            {
                stringBuilder.Append((string)"USH_GU_Maintained".Translate() + ": " + maintainedAlready.ToString());
                stringBuilder.AppendLine();

            }

            return stringBuilder.ToString().TrimEnd();
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(this.parent.Position, this.Props.radius);
        }

    }
}
