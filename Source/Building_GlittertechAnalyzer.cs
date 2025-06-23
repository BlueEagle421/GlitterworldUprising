using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Text;
using System.Linq;

namespace GlitterworldUprising
{
    [StaticConstructorOnStartup]
    public class Building_GlittertechAnalyzer : Building_WorkTableAutonomous
    {

        public Bill_Glittertech GlitterBill => ActiveBill as Bill_Glittertech;

        private static readonly Material FormingCycleBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.98f, 0.46f, 0f), false);
        private static readonly Material FormingCycleUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0f, 0f, 0f), false);
        public bool PoweredOn => PowerTrader.PowerOn;
        private CompPowerTrader _powerTrader;
        public CompPowerTrader PowerTrader
        {
            get
            {
                if (_powerTrader == null)
                    _powerTrader = this.TryGetComp<CompPowerTrader>();

                return _powerTrader;
            }
        }

        private EffecterHandler _electricEffecterHandler;

        public override void PostMake()
        {
            base.PostMake();
            _electricEffecterHandler = new EffecterHandler(this, USH_DefOf.USH_ElectricForming);
        }

        public override void PostMapInit()
        {
            base.PostMapInit();
            _electricEffecterHandler = new EffecterHandler(this, USH_DefOf.USH_ElectricForming);
        }

        public override void Notify_StartForming(Pawn billDoer)
        {
            if (!HasStoredPower(GlitterBill.GlittertechExt.powerNeeded))
                return;

            base.Notify_StartForming(billDoer);

            DrawPowerFromNet(GlitterBill.GlittertechExt.powerNeeded);

            if (Spawned && Map != null)
                _electricEffecterHandler.StartMaintaining(360, GlitterBill.GlittertechExt.analyzerOffsetY);

            SoundDefOf.MechGestatorCycle_Started.PlayOneShot(this);
        }

        public override void Notify_FormingCompleted()
        {
            innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            SoundDefOf.MechGestatorBill_Completed.PlayOneShot(this);
        }

        public override void Notify_HauledTo(Pawn hauler, Thing thing, int count)
        {
            thing.def.soundDrop.PlayOneShot(this);
        }

        protected override void Tick()
        {
            base.Tick();

            if (activeBill != null && PoweredOn)
                activeBill.BillTick();

            _electricEffecterHandler.Tick();
        }

        protected override string GetInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();

            if (GlitterBill == null)
                return sb.ToString().TrimEnd();

            if (GlitterBill.State == FormingState.Gathering)
            {
                if (HasStoredPower(GlitterBill.GlittertechExt.powerNeeded))
                    sb.AppendLine($"Will draw {GlitterBill.GlittertechExt.powerNeeded} W stored power from net".Colorize(Color.cyan));
                else
                    sb.AppendLine($"Needs {GlitterBill.GlittertechExt.powerNeeded} W power stored to start forming".Colorize(Color.red));
            }

            if (GlitterBill.State != FormingState.Gathering && GlitterBill.State != FormingState.Formed)
                sb.AppendLine(string.Format("{0}: {1}", "Total time left", GetTotalTimeForActiveBill().ToStringTicksToPeriod()));

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

            if (!TryGetFormingGraphic(out Graphic graphic))
                return;

            Vector3 loc = drawLoc;
            loc.y += 0.018292684f;
            loc.z += Mathf.PingPong(Find.TickManager.TicksGame * 0.0005f, 0.08f) + GlitterBill.GlittertechExt.analyzerOffsetY;

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


        private bool TryGetFormingGraphic(out Graphic graphic)
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

        private int GetTotalTimeForActiveBill()
        {
            float wholeCycleTicks = (GlitterBill.recipe.gestationCycles - GlitterBill.GestationCyclesCompleted) * GlitterBill.recipe.formingTicks;
            float currentCycleTicks = GlitterBill.formingTicks;

            return Mathf.CeilToInt(wholeCycleTicks + currentCycleTicks);
        }

        public bool HasStoredPower(float powerNeeded)
        {
            if (DebugSettings.unlimitedPower)
                return true;

            return PowerStoredInNet(PowerTrader.PowerNet) >= powerNeeded;
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
            foreach (CompPowerBattery battery in PowerTrader.PowerNet.batteryComps)
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
    }
}
