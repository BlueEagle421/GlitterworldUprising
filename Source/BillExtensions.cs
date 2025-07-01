
using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace USH_GE;

public static class BillUpgradeExtensions
{
    public const int MAX_BILLS = 1;
    public static Bill DoListingUpgrade(this BillStack stack, Rect rect, Func<List<FloatMenuOption>> recipeOptionsMaker, ref Vector2 scrollPosition, ref float viewHeight)
    {
        Bill result = null;
        Widgets.BeginGroup(rect);
        Text.Font = GameFont.Small;
        Rect rectButton = new(0f, 0f, 150f, 29f);

        if (stack.Count < MAX_BILLS)
        {
            if (Widgets.ButtonText(rectButton, "AddBill".Translate()))
            {
                Find.WindowStack.Add(new FloatMenu(recipeOptionsMaker()));
            }

            UIHighlighter.HighlightOpportunity(rectButton, "AddBill");
        }
        else
            Widgets.ButtonText(rectButton, "USH_MaxUpgrades".Translate(), true, false, false);


        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;
        Rect outRect = new Rect(0f, 35f, rect.width, rect.height - 35f);
        Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        float num = 0f;
        for (int i = 0; i < stack.Count; i++)
        {
            Bill bill = stack.Bills[i];
            Rect rect3 = bill.DoInterface(0f, num, viewRect.width, i);
            if (!bill.DeletedOrDereferenced && Mouse.IsOver(rect3))
            {
                result = bill;
            }

            num += rect3.height + 6f;
        }

        if (Event.current.type == EventType.Layout)
        {
            viewHeight = num + 60f;
        }

        Widgets.EndScrollView();
        Widgets.EndGroup();
        return result;
    }
}