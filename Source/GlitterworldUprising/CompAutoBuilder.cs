using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GliterworldUprising
{
    public class PlaceWorker_ShowAutoBuilderRadius : PlaceWorker
    {
        public override void DrawGhost(
          ThingDef def,
          IntVec3 center,
          Rot4 rot,
          Color ghostCol,
          Thing thing = null)
        {
            CompProperties_AutoBuilder compProperties = def.GetCompProperties<CompProperties_AutoBuilder>();
            if (compProperties == null)
                return;
            GenDraw.DrawRadiusRing(center, compProperties.radius);
        }
    }

    public class CompProperties_AutoBuilder : CompProperties
    {
        public float radius;
        public int workAmount, rareTicksBeforeCheck, rareTickPerCheck;
        public ThingDef moteDef;
        public CompProperties_AutoBuilder() => this.compClass = typeof(CompAutoBuilder);
    }

    public class CompAutoBuilder : ThingComp
    {
        Map map;
        private int rareTicksBeforeCheck = 0;
        private bool buildForbidden;
        private static HashSet<Thing> buildables = new HashSet<Thing>();

        public CompProperties_AutoBuilder Props => (CompProperties_AutoBuilder)this.props;


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            map = this.parent.Map;
            CollectDataForBuilding(false);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (buildables.Count > 0 && this.parent.GetComp<CompPowerTrader>().PowerOn)
            {
                buildThings();
            }
            rareTicksBeforeCheck++;
            if (rareTicksBeforeCheck == this.Props.rareTickPerCheck)
            {
                rareTicksBeforeCheck = 0;
                CollectDataForBuilding(false);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;
            if (DesignatorUtility.FindAllowedDesignator<Designator_ZoneAddStockpile_Resources>() != null)
            {
                Command_Action commandAction = new Command_Action();
                commandAction.action = (Action)(() => this.CollectDataForBuilding(true));
                commandAction.defaultLabel = (string)"USH_GU_FindBuildablesLabel".Translate();
                commandAction.defaultDesc = (string)"USH_GU_FindBuildablesDesc".Translate();
                commandAction.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/CollectData");
                yield return (Gizmo)commandAction;

                Command_Toggle commandToggle = new Command_Toggle();
                commandToggle.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/BuildForbiddenIcon");
                commandToggle.defaultLabel = (string)"USH_GU_BuildForbiddenLabel".Translate();
                commandToggle.defaultDesc = (string)"USH_GU_BuildForbiddenDesc".Translate();
                commandToggle.isActive = (() => buildForbidden);
                commandToggle.toggleAction = delegate
                {
                    buildForbidden = !buildForbidden;
                    CollectDataForBuilding(false);
                };

                yield return (Gizmo)commandToggle;
            }
        }

        private void CollectDataForBuilding(bool shouldFlashCells)
        {
            buildables.Clear();
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.parent.Position, this.Props.radius, true))
            {
                foreach (Thing thing in this.map.thingGrid.ThingsAt(cell))
                {
                    if (thing != null)
                    {
                        if (thing.def.IsFrame)
                        {
                            Frame frame = (Frame)thing;
                            if (frame.MaterialsNeeded().Count == 0)
                            {
                                buildables.Add(thing);
                                if (shouldFlashCells)
                                    map.debugDrawer.FlashCell(thing.Position, 1, null, 150);
                            }
                        }
                    }
                }
            }
        }

        private void buildThings()
        {
            foreach (Thing thing in buildables)
            {
                Frame frame = (Frame)thing;

                if (!frame.GetComp<CompForbiddable>().Forbidden)
                    frame.workDone += Props.workAmount;
                else if (buildForbidden)
                    frame.workDone += Props.workAmount;
                if ((double)frame.workDone >= (double)frame.WorkToBuild)
                {
                    CompleteBuilding(frame);
                    if (thing.Stuff != null)
                        thing.Stuff.stuffProps.soundImpactStuff.PlayOneShot((SoundInfo)new TargetInfo(frame.Position, map));
                    throwMote(thing);
                    goto label_1;
                }
            }
        label_1:
            CollectDataForBuilding(false);
        }

        private void CompleteBuilding(Frame frame)
        {
            if (this.parent.Faction != null)
                QuestUtility.SendQuestTargetSignals(this.parent.Faction.questTags, "BuiltBuilding", this.Named("SUBJECT"));
            List<CompHasSources> list = new List<CompHasSources>();
            for (int index = 0; index < frame.resourceContainer.Count; ++index)
            {
                CompHasSources comp = frame.resourceContainer[index].TryGetComp<CompHasSources>();
                if (comp != null)
                    list.Add(comp);
            }
            frame.resourceContainer.ClearAndDestroyContents();
            frame.Destroy(DestroyMode.Vanish);
            if ((double)frame.GetStatValue(StatDefOf.WorkToBuild) > 150.0 && frame.def.entityDefToBuild is ThingDef && ((ThingDef)frame.def.entityDefToBuild).category == ThingCategory.Building)
                SoundDefOf.Building_Complete.PlayOneShot((SoundInfo)new TargetInfo(frame.Position, map));
            ThingDef entityDefToBuild = frame.def.entityDefToBuild as ThingDef;
            Thing thing = (Thing)null;
            if (entityDefToBuild != null)
            {
                thing = ThingMaker.MakeThing(entityDefToBuild, frame.Stuff);
                thing.SetFactionDirect(frame.Faction);
                CompQuality comp1 = thing.TryGetComp<CompQuality>();
                if (comp1 != null)
                {
                    QualityCategory qualityCreatedByPawn = QualityUtility.GenerateQualityCreatedByPawn(14, false);
                    comp1.SetQuality(qualityCreatedByPawn, ArtGenerationContext.Colony);
                }
                CompArt comp2 = thing.TryGetComp<CompArt>();
                if (comp2 != null)
                {
                    if (comp1 == null)
                        comp2.InitializeArt(ArtGenerationContext.Colony);
                }
                CompHasSources comp3 = thing.TryGetComp<CompHasSources>();
                if (comp3 != null && !list.NullOrEmpty<CompHasSources>())
                {
                    for (int index = 0; index < list.Count; ++index)
                        list[index].TransferSourcesTo(comp3);
                }
                thing.HitPoints = Mathf.CeilToInt((float)frame.HitPoints / (float)frame.MaxHitPoints * (float)thing.MaxHitPoints);
                GenSpawn.Spawn(thing, frame.Position, map, frame.Rotation, WipeMode.FullRefund);
                if (thing is Building b)
                {
                    b.StyleSourcePrecept = frame.StyleSourcePrecept;
                }
                if (entityDefToBuild != null)
                {
                    Color? colorForBuilding = IdeoUtility.GetIdeoColorForBuilding(entityDefToBuild, this.parent.Faction);
                    if (colorForBuilding.HasValue)
                        thing.SetColor(colorForBuilding.Value);
                }
            }
            else
            {
                map.terrainGrid.SetTerrain(frame.Position, (TerrainDef)frame.def.entityDefToBuild);
                FilthMaker.RemoveAllFilth(frame.Position, map);
            }
            if (thing == null || (double)thing.GetStatValue(StatDefOf.WorkToBuild) < 9500.0)
                return;
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
                stringBuilder.Append((string)"USH_GU_DetBuildables".Translate() + ": " + buildables.Count.ToString());
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
