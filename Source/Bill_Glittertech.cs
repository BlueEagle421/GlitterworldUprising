using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;
using Verse.AI;

namespace GlitterworldUprising
{
    public class ModExtension_UseGlittertechBill : DefModExtension
    {
        public int powerNeeded;
        public int fuelNeeded;
        public float analyzerOffsetY = 0.7f;
    }

    public class Dialog_GlittertechBillConfig : Dialog_BillConfig
    {
        private static float formingInfoHeight;

        public Dialog_GlittertechBillConfig(Bill_Glittertech bill, IntVec3 billGiverPos)
            : base(bill, billGiverPos)
        {

        }

        protected override void DoIngredientConfigPane(float x, ref float y, float width, float height)
        {
            float y2 = y;

            base.DoIngredientConfigPane(x, ref y2, width, height - formingInfoHeight);

            if (!(bill.billStack.billGiver is Building_GlittertechAnalyzer analyzer) || analyzer.ActiveBill != bill)
                return;

            Rect rect = new Rect(x, y2, width, 9999f);

            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);

            StringBuilder stringBuilder = new StringBuilder();
            listing_Standard.Label("FormerIngredients".Translate() + ":");
            analyzer.ActiveBill.AppendCurrentIngredientCount(stringBuilder);
            listing_Standard.Label(stringBuilder.ToString());

            Bill_Mech bill_Mech = (Bill_Mech)bill;
            listing_Standard.Label(string.Concat("GestationCyclesCompleted".Translate() + ": ", bill_Mech.GestationCyclesCompleted.ToString(), " / ", bill_Mech.recipe.gestationCycles.ToString()));
            listing_Standard.Gap();
            listing_Standard.End();
            formingInfoHeight = listing_Standard.CurHeight;
        }
    }

    public class Bill_Glittertech : Bill_Autonomous
    {
        private Pawn boundPawn;
        private int gestationCycles;
        public Pawn BoundPawn => boundPawn;
        public int GestationCyclesCompleted => gestationCycles;
        public Building_GlittertechAnalyzer Analyzer => (Building_GlittertechAnalyzer)billStack.billGiver;
        public ModExtension_UseGlittertechBill GlittertechExt;

        protected override Color BaseColor
        {
            get
            {
                if (suspended)
                    return base.BaseColor;

                return Color.white;
            }
        }

        protected override string StatusString
        {
            get
            {
                switch (State)
                {
                    case FormingState.Gathering:
                        break;

                    case FormingState.Preparing:
                        if (BoundPawn != null)
                            return "Worker".Translate() + ": " + BoundPawn.LabelShortCap;
                        break;

                    case FormingState.Forming:
                        return "Gestating".Translate();

                    case FormingState.Formed:
                        if (BoundPawn != null)
                            return "WaitingFor".Translate() + ": " + BoundPawn.LabelShortCap;

                        break;
                }
                return null;
            }
        }

        public Bill_Glittertech() { }

        public Bill_Glittertech(RecipeDef recipe, Precept_ThingStyle precept = null) : base(recipe, precept)
        {
            GlittertechExt = recipe.GetModExtension<ModExtension_UseGlittertechBill>();
        }

        protected override Window GetBillDialog()
        {
            return new Dialog_GlittertechBillConfig(this, ((Thing)billStack.billGiver).Position);
        }

        public override void Notify_DoBillStarted(Pawn billDoer)
        {
            base.Notify_DoBillStarted(billDoer);

            if (boundPawn != billDoer)
                boundPawn = billDoer;
        }


        public override void Reset()
        {
            base.Reset();
            gestationCycles = 0;
            boundPawn = null;
        }


        public void ForceCompleteAllCycles()
        {
            gestationCycles = recipe.gestationCycles;
            formingTicks = 0f;
        }


        public override void BillTick()
        {
            if (suspended || state != FormingState.Forming)
                return;

            formingTicks -= 1f;

            if (formingTicks > 0f)
                return;

            gestationCycles++;
            if (gestationCycles >= recipe.gestationCycles)
            {
                state = FormingState.Formed;
                Analyzer.Notify_FormingCompleted();
                return;
            }
            formingTicks = recipe.formingTicks;
            state = FormingState.Preparing;
        }

        public override bool PawnAllowedToStartAnew(Pawn p)
        {
            if (!Analyzer.HasStoredPower(GlittertechExt.powerNeeded))
            {
                JobFailReason.Is("NoPower".Translate(), null);
                return false;
            }

            return base.PawnAllowedToStartAnew(p);
        }

        public override void AppendInspectionData(StringBuilder sb)
        {
            if (State != FormingState.Forming && State != FormingState.Preparing)
                return;

            sb.AppendLine("CurrentGestationCycle".Translate() + ": " + ((int)(formingTicks * 1f)).ToStringTicksToPeriod(true, false, true, true, false));
            sb.AppendLine("RemainingGestationCycles".Translate() + ": " + (recipe.gestationCycles - GestationCyclesCompleted).ToString() + " (" + "OfLower".Translate() + " " + recipe.gestationCycles.ToString() + ")");

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref boundPawn, "boundPawn", false);
            Scribe_Values.Look(ref gestationCycles, "gestationCycles", 0, false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                GlittertechExt = recipe.GetModExtension<ModExtension_UseGlittertechBill>();
        }
    }
}
