using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace GliterworldUprising
{
    public class PlaceWorker_ShowAutoRepairerRadius : PlaceWorker
    {
        public override void DrawGhost(
          ThingDef def,
          IntVec3 center,
          Rot4 rot,
          Color ghostCol,
          Thing thing = null)
        {
            CompProperties_AutoRepairer compProperties = def.GetCompProperties<CompProperties_AutoRepairer>();
            if (compProperties == null)
                return;
            GenDraw.DrawRadiusRing(center, compProperties.radius);
        }
    }

    public class CompProperties_AutoRepairer : CompProperties
    {
        public float radius;
        public int repairAmount, rareTicksPerCheck, overclockPowerConsumption, defaultPowerConsumtion, ticksToOverheat;
        public ThingDef moteDef;
        public CompProperties_AutoRepairer() => this.compClass = typeof(CompAutoRepairer);
    }

    public class CompAutoRepairer : ThingComp
    {
        Map map;
        private int repairsBeforeCheck = 0;
        private bool overclocked;
        private float overheating;
        private static HashSet<Thing> repairables = new HashSet<Thing>();
        private static List<Thing> filteredRepairables = new List<Thing>();

        public CompProperties_AutoRepairer Props => (CompProperties_AutoRepairer)this.props;


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            map = this.parent.Map;
            CollectDataForRepair(false);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (overclocked)
                CollectDataForRepair(false);
            if (filteredRepairables.Count > 0 && this.parent.GetComp<CompPowerTrader>().PowerOn)
            {
                if (!overclocked)
                    repairThings();
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        repairThings();
                    }
                }

            }
            repairsBeforeCheck++;
            if (repairsBeforeCheck == this.Props.rareTicksPerCheck)
            {
                repairsBeforeCheck = 0;
                CollectDataForRepair(false);
            }
            if (overclocked)
            {
                this.parent.GetComp<CompPowerTrader>().powerOutputInt = -this.Props.overclockPowerConsumption;
                if (this.parent.GetComp<CompPowerTrader>().PowerOn)
                    overheating++;
                else if (overheating > 0)
                    overheating -= 0.5f;
            }
            else
            {
                this.parent.GetComp<CompPowerTrader>().powerOutputInt = -this.Props.defaultPowerConsumtion;
                if (overheating > 0)
                {
                    overheating -= 0.5f;
                }
            }

            if (overheating >= Props.ticksToOverheat)
            {
                Explode();
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            CompAutoRepairer compAutoRepairer = this;

            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;
            if (DesignatorUtility.FindAllowedDesignator<Designator_ZoneAddStockpile_Resources>() != null)
            {
                Command_Action commandAction = new Command_Action();
                commandAction.action = (Action)(() => this.CollectDataForRepair(true));
                commandAction.defaultLabel = (string)"USH_GU_FindRepairablesLabel".Translate();
                commandAction.defaultDesc = (string)"USH_GU_FindRepairablesDesc".Translate();
                commandAction.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/CollectData");
                yield return (Gizmo)commandAction;

                Command_Toggle commandToggle = new Command_Toggle();
                commandToggle.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/RepairOverclockIcon");
                commandToggle.defaultLabel = (string)"USH_GU_OverclockLabel".Translate();
                commandToggle.defaultDesc = (string)"USH_GU_OverclockDesc".Translate();
                commandToggle.isActive = (() => overclocked);
                commandToggle.toggleAction = delegate
                {
                    overclocked = !overclocked;
                    CollectDataForRepair(false);
                };

                yield return (Gizmo)commandToggle;
            }
        }

        private void CollectDataForRepair(bool shouldFlashCells)
        {
            repairables.Clear();
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.parent.Position, this.Props.radius, true))
            {
                foreach (Thing thing in this.map.thingGrid.ThingsAt(cell))
                {
                    if (thing != null)
                    {
                        if (thing.def.IsBuildingArtificial)
                        {
                            repairables.Add(thing);
                            if (shouldFlashCells)
                                map.debugDrawer.FlashCell(thing.Position, 1, null, 150);
                        }
                    }

                }
            }
            filterRepairables();
        }

        private void filterRepairables()
        {
            filteredRepairables.Clear();
            foreach (Thing thing in repairables)
            {
                if (thing.MaxHitPoints > thing.HitPoints && thing != this.parent)
                {
                    filteredRepairables.Add(thing);
                }
            }
        }

        private void repairThings()
        {
            if (this.parent.MaxHitPoints > this.parent.HitPoints)
            {
                repairItself();
            }
            int index = UnityEngine.Random.Range(0, filteredRepairables.Count);
            Thing thing = filteredRepairables[index];
            if (thing != null)
            {
                if ((thing.MaxHitPoints - thing.HitPoints) < this.Props.repairAmount)
                {
                    thing.HitPoints = thing.MaxHitPoints;
                    goto label_1;
                }
                else
                {
                    thing.HitPoints += this.Props.repairAmount;
                }
                throwMote(thing);
            }
        label_1:
            filterRepairables();
        }

        private void repairItself()
        {
            if (this.parent != null)
            {
                if ((this.parent.MaxHitPoints - this.parent.HitPoints) < this.Props.repairAmount)
                {
                    this.parent.HitPoints = this.parent.MaxHitPoints;
                }
                else
                {
                    this.parent.HitPoints += this.Props.repairAmount;
                }
                throwMote(this.parent);
            }
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

        private void Explode()
        {
            this.parent.Destroy(DestroyMode.Deconstruct);
            GenExplosion.DoExplosion(this.parent.Position, map, this.Props.radius, DamageDefOf.Flame, (Thing)null);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this != null)
            {
                if (this.parent.GetComp<CompPowerTrader>().PowerOn)
                {
                    if (filteredRepairables.Count > 0)
                    {
                        stringBuilder.Append((string)"USH_GU_Repairing".Translate() + "...");
                        stringBuilder.AppendLine();
                    }
                    if (overheating > 0)
                    {
                        if (overclocked)
                            stringBuilder.Append((string)"USH_GU_Overheating".Translate() + ": " + (Math.Round((overheating / Props.ticksToOverheat) * 100f)).ToString() + "%");
                        else
                            stringBuilder.Append((string)"USH_GU_CoolingDown".Translate() + ": " + (Math.Round((overheating / Props.ticksToOverheat) * 100f)).ToString() + "%");
                    }
                }
                else if (overheating > 0)
                    stringBuilder.Append((string)"USH_GU_CoolingDown".Translate() + ": " + (Math.Round((overheating / Props.ticksToOverheat) * 100f)).ToString() + "%");

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
