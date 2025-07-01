using System.Collections.Generic;
using RimWorld;
using Verse;

namespace USH_GE;

public class ModExtension_RecipeUpgrade : DefModExtension
{

}

public class Bill_Upgrade(RecipeDef recipe, Precept_ThingStyle precept = null) : Bill_Production(recipe, precept)
{
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
}