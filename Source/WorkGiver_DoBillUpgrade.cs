using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace USH_GE;

public class WorkGiver_DoBillUpgrade : WorkGiver_Scanner
{
    private class DefCountList
    {
        private readonly List<ThingDef> defs = [];

        private readonly List<float> counts = [];

        public int Count => defs.Count;

        public float this[ThingDef def]
        {
            get
            {
                int num = defs.IndexOf(def);
                if (num < 0)
                {
                    return 0f;
                }
                return counts[num];
            }
            set
            {
                int num = defs.IndexOf(def);
                if (num < 0)
                {
                    defs.Add(def);
                    counts.Add(value);
                    num = defs.Count - 1;
                }
                else
                {
                    counts[num] = value;
                }
                CheckRemove(num);
            }
        }

        public float GetCount(int index)
        {
            return counts[index];
        }

        public void SetCount(int index, float val)
        {
            counts[index] = val;
            CheckRemove(index);
        }

        public ThingDef GetDef(int index)
        {
            return defs[index];
        }

        private void CheckRemove(int index)
        {
            if (counts[index] == 0f)
            {
                counts.RemoveAt(index);
                defs.RemoveAt(index);
            }
        }

        public void Clear()
        {
            defs.Clear();
            counts.Clear();
        }

        public void GenerateFrom(List<Thing> things)
        {
            Clear();

            for (int i = 0; i < things.Count; i++)
                this[things[i].def] += things[i].stackCount;
        }
    }

    private readonly List<ThingCount> chosenIngThings = [];

    private static readonly List<IngredientCount> missingIngredients = [];

    private static readonly List<Thing> tmpMissingUniqueIngredients = [];

    private static readonly IntRange ReCheckFailedBillTicksRange = new(500, 600);

    private static readonly List<Thing> relevantThings = [];

    private static readonly HashSet<Thing> processedThings = [];

    private static readonly List<Thing> newRelevantThings = [];

    private static readonly DefCountList availableCounts = new();

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override ThingRequest PotentialWorkThingRequest
    {
        get
        {
            if (def.fixedBillGiverDefs != null && def.fixedBillGiverDefs.Count == 1)
            {
                return ThingRequest.ForDef(def.fixedBillGiverDefs[0]);
            }
            return ThingRequest.ForGroup(ThingRequestGroup.Everything);
        }
    }

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Some;
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        var list = pawn.Map.GetComponent<MapComponent_Upgradables>().AllUpgradables;

        return list.EnumerableNullOrEmpty();
    }

    public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
    {
        var comp = thing.TryGetComp<CompUpgradable>();

        if (comp == null)
            return null;

        if (comp is not IBillGiverUpgrade billGiver)
            return null;

        if (!ThingIsUsableBillGiver(thing))
            return null;

        if (!billGiver.BillStack.AnyShouldDoNow)
            return null;

        if (!pawn.CanReserve(thing, 1, -1, null, forced))
            return null;

        if (thing.IsBurning())
            return null;

        billGiver.BillStack.RemoveIncompletableBills();
        return StartOrResumeBillJob(pawn, billGiver, forced);
    }

    private Job StartOrResumeBillJob(Pawn pawn, IBillGiverUpgrade giver, bool forced = false)
    {
        bool flag = FloatMenuMakerMap.makingFor == pawn;
        for (int i = 0; i < giver.BillStack.Count; i++)
        {
            Bill bill = giver.BillStack[i];
            if ((bill.recipe.requiredGiverWorkType != null && bill.recipe.requiredGiverWorkType != def.workType) || (Find.TickManager.TicksGame <= bill.nextTickToSearchForIngredients && FloatMenuMakerMap.makingFor != pawn) || !bill.ShouldDoNow() || !bill.PawnAllowedToStartAnew(pawn))
            {
                continue;
            }
            SkillRequirement skillRequirement = bill.recipe.FirstSkillRequirementPawnDoesntSatisfy(pawn);
            if (skillRequirement != null)
            {
                JobFailReason.Is("UnderRequiredSkill".Translate(skillRequirement.minLevel), bill.Label);
                continue;
            }
            List<IngredientCount> list = null;
            if (flag)
            {
                list = missingIngredients;
                list.Clear();
                tmpMissingUniqueIngredients.Clear();
            }
            if (!TryFindBestBillIngredients(bill, pawn, giver, chosenIngThings, list) || !tmpMissingUniqueIngredients.NullOrEmpty())
            {
                if (FloatMenuMakerMap.makingFor != pawn)
                {
                    bill.nextTickToSearchForIngredients = Find.TickManager.TicksGame + ReCheckFailedBillTicksRange.RandomInRange;
                }
                else if (flag)
                {
                    string text = list.Select(missing => missing.Summary).Concat(tmpMissingUniqueIngredients.Select(t => t.Label)).ToCommaList();
                    JobFailReason.Is("MissingMaterials".Translate(text), bill.Label);

                    flag = false;
                }
                chosenIngThings.Clear();
                continue;
            }
            flag = false;
            Job result = TryStartNewDoBillJob(pawn, bill, giver, chosenIngThings, out Job haulOffJob);
            chosenIngThings.Clear();
            return result;
        }
        chosenIngThings.Clear();
        return null;
    }

    public static Job TryStartNewDoBillJob(Pawn pawn, Bill bill, IBillGiverUpgrade giver, List<ThingCount> chosenIngThings, out Job haulOffJob, bool dontCreateJobIfHaulOffRequired = true)
    {
        haulOffJob = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, giver, null);
        if (haulOffJob != null && dontCreateJobIfHaulOffRequired)
        {
            return haulOffJob;
        }
        Job job = JobMaker.MakeJob(JobDefOf.DoBill, giver.BillThingSource);
        job.targetQueueB = new List<LocalTargetInfo>(chosenIngThings.Count);
        job.countQueue = new List<int>(chosenIngThings.Count);
        for (int i = 0; i < chosenIngThings.Count; i++)
        {
            job.targetQueueB.Add(chosenIngThings[i].Thing);
            job.countQueue.Add(chosenIngThings[i].Count);
        }
        if (bill.xenogerm != null)
        {
            job.targetQueueB.Add(bill.xenogerm);
            job.countQueue.Add(1);
        }
        job.haulMode = HaulMode.ToCellNonStorage;
        job.bill = bill;
        return job;
    }

    public bool ThingIsUsableBillGiver(Thing thing)
    {
        if (def.fixedBillGiverDefs != null && def.fixedBillGiverDefs.Contains(thing.def))
            return true;

        if (thing is Pawn pawn)
        {
            if (def.billGiversAllHumanlikes && pawn.RaceProps.Humanlike)
                return true;

            if (def.billGiversAllMechanoids && pawn.RaceProps.IsMechanoid)
                return true;

            if (def.billGiversAllAnimals && pawn.IsAnimal)
                return true;
        }

        return false;
    }

    private static bool IsUsableIngredient(Thing t, Bill bill)
    {
        if (!bill.IsFixedOrAllowedIngredient(t))
        {
            return false;
        }
        foreach (IngredientCount ingredient in bill.recipe.ingredients)
        {
            if (ingredient.filter.Allows(t))
            {
                return true;
            }
        }
        return false;
    }

    private static bool TryFindBestBillIngredients(Bill bill, Pawn pawn, IBillGiverUpgrade billGiver, List<ThingCount> chosen, List<IngredientCount> missingIngredients)
    {
        return TryFindBestIngredientsHelper(t => IsUsableIngredient(t, bill), foundThings => TryFindBestBillIngredientsInSet(foundThings, bill, chosen, GetBillGiverRootCell(billGiver), billGiver is Pawn, missingIngredients), bill.recipe.ingredients, pawn, billGiver.BillThingSource, chosen, bill.ingredientSearchRadius);
    }

    private static bool TryFindBestIngredientsHelper(Predicate<Thing> thingValidator, Predicate<List<Thing>> foundAllIngredientsAndChoose, List<IngredientCount> ingredients, Pawn pawn, Thing billGiver, List<ThingCount> chosen, float searchRadius)
    {
        chosen.Clear();
        newRelevantThings.Clear();
        if (ingredients.Count == 0)
        {
            return true;
        }
        IntVec3 billGiverRootCell = GetBillGiverRootCell(billGiver);
        Region rootReg = billGiverRootCell.GetRegion(pawn.Map);
        if (rootReg == null)
        {
            return false;
        }
        relevantThings.Clear();
        processedThings.Clear();
        bool foundAll = false;
        float radiusSq = searchRadius * searchRadius;
        bool baseValidator(Thing t) => t.Spawned && thingValidator(t) && (t.Position - billGiver.Position).LengthHorizontalSquared < radiusSq && !t.IsForbidden(pawn) && pawn.CanReserve(t);
        if (billGiver is Building_WorkTableAutonomous building_WorkTableAutonomous)
        {
            relevantThings.AddRange(building_WorkTableAutonomous.innerContainer);
            if (foundAllIngredientsAndChoose(relevantThings))
            {
                relevantThings.Clear();
                return true;
            }
        }
        TraverseParms traverseParams = TraverseParms.For(pawn);
        RegionEntryPredicate entryCondition = null;
        if (Math.Abs(999f - searchRadius) >= 1f)
        {
            entryCondition = delegate (Region from, Region r)
            {
                if (!r.Allows(traverseParams, isDestination: false))
                {
                    return false;
                }
                CellRect extentsClose = r.extentsClose;
                int num = Math.Abs(billGiver.Position.x - Math.Max(extentsClose.minX, Math.Min(billGiver.Position.x, extentsClose.maxX)));
                if (num > searchRadius)
                {
                    return false;
                }
                int num2 = Math.Abs(billGiver.Position.z - Math.Max(extentsClose.minZ, Math.Min(billGiver.Position.z, extentsClose.maxZ)));
                return !(num2 > searchRadius) && (num * num + num2 * num2) <= radiusSq;
            };
        }
        else
        {
            entryCondition = (from, r) => r.Allows(traverseParams, isDestination: false);
        }
        int adjacentRegionsAvailable = rootReg.Neighbors.Count(region => entryCondition(rootReg, region));
        int regionsProcessed = 0;
        processedThings.AddRange(relevantThings);
        foundAllIngredientsAndChoose(relevantThings);
        bool regionProcessor(Region r)
        {
            List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                if (!processedThings.Contains(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.ClosestTouch, pawn) && baseValidator(thing) && !thing.def.IsMedicine)
                {
                    newRelevantThings.Add(thing);
                    processedThings.Add(thing);
                }
            }
            int num = regionsProcessed + 1;
            regionsProcessed = num;
            if (newRelevantThings.Count > 0 && regionsProcessed > adjacentRegionsAvailable)
            {
                relevantThings.AddRange(newRelevantThings);
                newRelevantThings.Clear();
                if (foundAllIngredientsAndChoose(relevantThings))
                {
                    foundAll = true;
                    return true;
                }
            }
            return false;
        }
        RegionTraverser.BreadthFirstTraverse(rootReg, entryCondition, regionProcessor, 99999);
        relevantThings.Clear();
        newRelevantThings.Clear();
        processedThings.Clear();
        return foundAll;
    }

    private static IntVec3 GetBillGiverRootCell(Thing t)
    {
        var comp = t.TryGetComp<CompUpgradable>();

        if (comp is not IBillGiverUpgrade giver)
            throw new Exception($"{nameof(GetBillGiverRootCell)} called for a thing without {nameof(CompUpgradable)}");

        return GetBillGiverRootCell(giver);
    }

    private static IntVec3 GetBillGiverRootCell(IBillGiverUpgrade billGiver)
    {
        if (billGiver is Building building)
        {
            if (building.def.hasInteractionCell)
                return building.InteractionCell;

        }
        return billGiver.BillInteractionCell;
    }

    private static bool TryFindBestBillIngredientsInSet(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, List<IngredientCount> missingIngredients)
    {
        if (bill.recipe.allowMixingIngredients)
        {
            return TryFindBestBillIngredientsInSet_AllowMix(availableThings, bill, chosen, rootCell, missingIngredients);
        }
        return TryFindBestBillIngredientsInSet_NoMix(availableThings, bill, chosen, rootCell, alreadySorted, missingIngredients);
    }

    private static bool TryFindBestBillIngredientsInSet_NoMix(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, List<IngredientCount> missingIngredients)
    {
        return TryFindBestIngredientsInSet_NoMixHelper(availableThings, bill.recipe.ingredients, chosen, rootCell, alreadySorted, missingIngredients, bill);
    }

    private static bool TryFindBestIngredientsInSet_NoMixHelper(List<Thing> availableThings, List<IngredientCount> ingredients, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, List<IngredientCount> missingIngredients, Bill bill = null)
    {
        if (!alreadySorted)
        {
            int comparison(Thing t1, Thing t2)
            {
                float num7 = (t1.PositionHeld - rootCell).LengthHorizontalSquared;
                float value = (t2.PositionHeld - rootCell).LengthHorizontalSquared;
                return num7.CompareTo(value);
            }
            availableThings.Sort(comparison);
        }
        chosen.Clear();
        availableCounts.Clear();
        missingIngredients?.Clear();
        availableCounts.GenerateFrom(availableThings);
        for (int num = 0; num < ingredients.Count; num++)
        {
            IngredientCount ingredientCount = ingredients[num];
            bool flag = false;
            for (int num2 = 0; num2 < availableCounts.Count; num2++)
            {
                float num3 = (bill != null) ? ingredientCount.CountRequiredOfFor(availableCounts.GetDef(num2), bill.recipe, bill) : ingredientCount.GetBaseCount();
                if ((bill != null && !bill.recipe.ignoreIngredientCountTakeEntireStacks && num3 > availableCounts.GetCount(num2)) || !ingredientCount.filter.Allows(availableCounts.GetDef(num2)) || (bill != null && !ingredientCount.IsFixedIngredient && !bill.ingredientFilter.Allows(availableCounts.GetDef(num2))))
                {
                    continue;
                }
                for (int num4 = 0; num4 < availableThings.Count; num4++)
                {
                    if (availableThings[num4].def != availableCounts.GetDef(num2))
                    {
                        continue;
                    }
                    int num5 = availableThings[num4].stackCount - ThingCountUtility.CountOf(chosen, availableThings[num4]);
                    if (num5 > 0)
                    {
                        if (bill != null && bill.recipe.ignoreIngredientCountTakeEntireStacks)
                        {
                            ThingCountUtility.AddToList(chosen, availableThings[num4], num5);
                            return true;
                        }
                        int num6 = Mathf.Min(Mathf.FloorToInt(num3), num5);
                        ThingCountUtility.AddToList(chosen, availableThings[num4], num6);
                        num3 -= (float)num6;
                        if (num3 < 0.001f)
                        {
                            flag = true;
                            float count = availableCounts.GetCount(num2);
                            count -= num3;
                            availableCounts.SetCount(num2, count);
                            break;
                        }
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            if (!flag)
            {
                if (missingIngredients == null)
                {
                    return false;
                }
                missingIngredients.Add(ingredientCount);
            }
        }
        if (missingIngredients != null)
        {
            return missingIngredients.Count == 0;
        }
        return true;
    }

    private static bool TryFindBestBillIngredientsInSet_AllowMix(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, List<IngredientCount> missingIngredients)
    {
        chosen.Clear();
        missingIngredients?.Clear();
        availableThings.SortBy(t => bill.recipe.IngredientValueGetter.ValuePerUnitOf(t.def), t => (t.Position - rootCell).LengthHorizontalSquared);
        for (int num = 0; num < bill.recipe.ingredients.Count; num++)
        {
            IngredientCount ingredientCount = bill.recipe.ingredients[num];
            float num2 = ingredientCount.GetBaseCount();
            for (int num3 = 0; num3 < availableThings.Count; num3++)
            {
                Thing thing = availableThings[num3];
                if (ingredientCount.filter.Allows(thing) && (ingredientCount.IsFixedIngredient || bill.ingredientFilter.Allows(thing)))
                {
                    float num4 = bill.recipe.IngredientValueGetter.ValuePerUnitOf(thing.def);
                    int num5 = Mathf.Min(Mathf.CeilToInt(num2 / num4), thing.stackCount);
                    ThingCountUtility.AddToList(chosen, thing, num5);
                    num2 -= num5 * num4;
                    if (num2 <= 0.0001f)
                    {
                        break;
                    }
                }
            }
            if (num2 > 0.0001f)
            {
                if (missingIngredients == null)
                {
                    return false;
                }
                missingIngredients.Add(ingredientCount);
            }
        }
        if (missingIngredients != null)
        {
            return missingIngredients.Count == 0;
        }
        return true;
    }
}
