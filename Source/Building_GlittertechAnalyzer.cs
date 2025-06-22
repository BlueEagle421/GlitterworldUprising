using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Text;

namespace GlitterworldUprising
{

    public class ITab_GlittertechNutritionStorage : ITab_Storage
    {
        public ITab_GlittertechNutritionStorage()
        {
            labelKey = "Nutrition";
        }

    }

    [StaticConstructorOnStartup]
    public class Building_GlittertechAnalyzer : Building_WorkTableAutonomous, IStoreSettingsParent, IThingHolder
    {

        public Bill_Glittertech GlitterBill => ActiveBill as Bill_Glittertech;

        private static readonly Material FormingCycleBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.98f, 0.46f, 0f), false);
        private static readonly Material FormingCycleUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0f, 0f, 0f), false);

        private CompPowerTrader power;
        public CompPowerTrader Power
        {
            get
            {
                if (power == null)
                    power = this.TryGetComp<CompPowerTrader>();

                return power;
            }
        }

        private CompRefuelable refuelable;
        public CompRefuelable Refuelable
        {
            get
            {
                if (refuelable == null)
                    refuelable = this.TryGetComp<CompRefuelable>();

                return refuelable;
            }
        }

        public bool PoweredOn => Power.PowerOn;

        private StorageSettings _storageSettings;
        public StorageSettings GetStoreSettings() => _storageSettings;
        public StorageSettings GetParentStoreSettings() => def.building?.defaultStorageSettings;

        private ThingOwner _nutritionContainer;
        private float _liquifiedNutrition;
        private static readonly List<Thing> tmpItems = new List<Thing>();

        public bool StorageTabVisible => true;

        public Building_GlittertechAnalyzer()
        {
            _nutritionContainer = new ThingOwner<Thing>(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (_storageSettings == null)
            {
                _storageSettings = new StorageSettings(this);
                if (def.building?.defaultStorageSettings != null)
                {
                    _storageSettings.CopyFrom(def.building.defaultStorageSettings);
                }
            }
        }

        public void Notify_SettingsChanged()
        {

        }

        public override void Notify_StartForming(Pawn billDoer)
        {
            DrawPowerFromNet(GlitterBill.GlittertechExt.powerNeeded);
            refuelable.ConsumeFuel(GlitterBill.GlittertechExt.fuelNeeded);

            SoundDefOf.MechGestatorCycle_Started.PlayOneShot(this);
        }


        public override void Notify_FormingCompleted()
        {
            innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            SoundDefOf.MechGestatorBill_Completed.PlayOneShot(this);
        }


        public override void Notify_HauledTo(Pawn hauler, Thing thing, int count)
        {
            SoundDefOf.MechGestator_MaterialInserted.PlayOneShot(this);
            LiquifyNutrition();
        }


        protected override void Tick()
        {
            base.Tick();

            if (activeBill != null && PoweredOn)
                activeBill.BillTick();
        }


        protected override string GetInspectStringExtra()
        {
            if (GlitterBill == null)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.AppendLineIfNotEmpty().Append("Nutrition".Translate()).Append(": ").Append(_liquifiedNutrition.ToStringByStyle(ToStringStyle.FloatMaxOne, ToStringNumberSense.Absolute)).Append(" / ").Append(5f);

            sb.Append(string.Format("{0}: {1}", "Total time left", Mathf.CeilToInt((GlitterBill.recipe.gestationCycles - GlitterBill.GestationCyclesCompleted) * GlitterBill.recipe.formingTicks + GlitterBill.formingTicks * 1f).ToStringTicksToPeriod()));

            return sb.ToString().TrimEnd();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            DrawFormingThing(drawLoc);

            DrawBar(drawLoc);
        }

        private void DrawFormingThing(Vector3 drawLoc)
        {
            if (activeBill == null || activeBill.State == FormingState.Gathering)
                return;

            if (!TryGetMechFormingGraphic(out Graphic graphic))
                return;

            Vector3 loc = drawLoc;
            loc.y += 0.018292684f;
            loc.z += Mathf.PingPong(Find.TickManager.TicksGame * 0.0005f, 0.08f) + 0.7f;

            Material transparentMat = MaterialPool.MatFrom(ActiveBill.recipe.products[0].thingDef.graphicData.texPath, ShaderDatabase.Transparent);
            transparentMat.color = new Color(1f, 1f, 1f, 0.5f);

            Mesh mesh = graphic.MeshAt(Rot4.North);
            Quaternion quat = graphic.QuatFromRot(Rot4.North);

            Graphics.DrawMesh(mesh, loc, quat, transparentMat, 0);
        }

        private void DrawBar(Vector3 drawLoc)
        {
            GenDraw.FillableBarRequest barDrawData = BarDrawData;
            barDrawData.center = drawLoc;
            barDrawData.fillPercent = CurrentBillFormingPercent;
            barDrawData.filledMat = FormingCycleBarFilledMat;
            barDrawData.unfilledMat = FormingCycleUnfilledMat;
            barDrawData.rotation = Rotation;
            GenDraw.DrawFillableBar(barDrawData);
        }


        private bool TryGetMechFormingGraphic(out Graphic graphic)
        {
            graphic = null;

            if (ActiveBill.recipe.products[0] == null)
                return false;

            graphic = ActiveBill.recipe.products[0].thingDef.graphic;
            graphic = graphic.GetCopy(graphic.drawSize * 0.6f, null);

            return true;
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;

            if (!DebugSettings.ShowDevGizmos)
                yield break;

            if (GlitterBill != null && GlitterBill.State != FormingState.Gathering && GlitterBill.State != FormingState.Formed)
            {
                yield return new Command_Action
                {
                    action = new Action(GlitterBill.ForceCompleteAllCycles),
                    defaultLabel = "DEV: Complete all cycles"
                };
            }
        }

        private void LiquifyNutrition()
        {
            tmpItems.AddRange(_nutritionContainer);
            foreach (Thing thing in tmpItems)
            {
                float num = thing.GetStatValue(StatDefOf.Nutrition, true, -1) * (float)thing.stackCount;
                if (num > 0f && !(thing is Pawn))
                {
                    _liquifiedNutrition = Mathf.Min(5f, _liquifiedNutrition + num);
                    thing.Destroy(DestroyMode.Vanish);
                }
            }
            tmpItems.Clear();
        }

        public float RequiredNutritionRemaining
        {
            get
            {
                return Mathf.Max(5f - _liquifiedNutrition, 0f);
            }
        }

        public bool HasNutrition(float nutrition)
        {
            if (_liquifiedNutrition < nutrition)
                return false;

            return true;
        }

        public bool HasStoredPower(float powerNeeded)
        {
            if (DebugSettings.unlimitedPower)
                return true;

            return PowerStoredInNet(power.PowerNet) >= powerNeeded;
        }

        private float PowerStoredInNet(PowerNet powerNet)
        {
            float power = 0;

            if (powerNet == null)
                return 0;

            foreach (CompPowerBattery battery in powerNet.batteryComps)
                power += battery.StoredEnergy;

            return power;
        }

        private void DrawPowerFromNet(float powerToDraw)
        {
            foreach (CompPowerBattery battery in power.PowerNet.batteryComps)
            {
                if (powerToDraw >= battery.StoredEnergy)
                {
                    powerToDraw -= battery.StoredEnergy;
                    battery.DrawPower(battery.StoredEnergy);
                }
                else
                {
                    battery.DrawPower(powerToDraw);
                    break;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref _storageSettings, "_storageSettings", this);
        }
    }
}
