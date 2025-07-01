using System.Collections.Generic;
using RimWorld;
using Verse;

namespace USH_GE;

public class ModExtension_RecipeUpgrade : DefModExtension
{

}

public class Bill_Upgrade : Bill_Production
{
    public ModExtension_RecipeUpgrade UpgradeExt;

    public Bill_Upgrade() { }
    public Bill_Upgrade(RecipeDef recipe, Precept_ThingStyle precept = null) : base(recipe, precept)
    {
        UpgradeExt = recipe.GetModExtension<ModExtension_RecipeUpgrade>();
    }
    public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
    {
        if (repeatMode == BillRepeatModeDefOf.RepeatCount)
        {
            if (repeatCount > 0)
                repeatCount--;

            if (repeatCount == 0)
                Messages.Message("MessageBillComplete".Translate(LabelCap), (billStack.billGiver as IBillGiverUpgrade).BillThingSource, MessageTypeDefOf.TaskCompletion);

        }
        recipe.Worker.Notify_IterationCompleted(billDoer, ingredients);
    }

    public override void ExposeData()
    {
        base.ExposeData();

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            UpgradeExt = recipe.GetModExtension<ModExtension_RecipeUpgrade>();
    }
}