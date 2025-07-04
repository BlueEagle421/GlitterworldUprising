
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace USH_GE;

[StaticConstructorOnStartup]
public class ITab_ContentsBiocoder : ITab_ContentsBase
{
    private readonly List<Thing> listInt = [];
    public override IList<Thing> container
    {
        get
        {
            listInt.Clear();

            if (SelThing is Building_Biocoder targeter && targeter.ContainedThing != null)
                listInt.Add(targeter.ContainedThing);

            return listInt;
        }
    }

    public ITab_ContentsBiocoder()
    {
        labelKey = "TabCasketContents";
        containedItemsKey = "ContainedItems";
        canRemoveThings = false;
    }
}

[StaticConstructorOnStartup]
public class Building_Biocoder : Building_TurretRocket, IThingHolder, ISearchableContents
{
    public int VerbIndex { get; set; }
    public override Verb AttackVerb => GunCompEq.AllVerbs[VerbIndex];
    public override Material TurretTopMaterial => def.building.turretTopMat;


    protected ThingOwner innerContainer;
    public bool HasAnyContents => innerContainer.Count > 0;
    public Thing ContainedThing
    {
        get
        {
            if (innerContainer.Count != 0)
            {
                return innerContainer[0];
            }
            return null;
        }
    }


    public ThingOwner SearchableContents => innerContainer;

    public Building_Biocoder()
    {
        innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return innerContainer;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
        if (Faction == Faction.OfPlayer && innerContainer.Count > 0 && def.building.isPlayerEjectable)
        {
            Command_Action command_Action = new()
            {
                action = EjectContents,
                defaultLabel = "CommandPodEject".Translate(),
                defaultDesc = "CommandPodEjectDesc".Translate()
            };

            if (innerContainer.Count == 0)
                command_Action.Disable("CommandPodEjectFailEmpty".Translate());

            command_Action.hotKey = KeyBindingDefOf.Misc8;
            command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
            yield return command_Action;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
    }

    public virtual bool Accepts(Thing thing) => innerContainer.CanAcceptAnyOf(thing);

    public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
    {
        if (!Accepts(thing))
            return false;

        bool success;
        if (thing.holdingOwner != null)
        {
            success = innerContainer.TryAddOrTransfer(thing);

            if (allowSpecialEffects && success)
                PlayEnteredSound();
        }
        else
        {
            success = innerContainer.TryAdd(thing);
            if (allowSpecialEffects && success)
                PlayEnteredSound();
        }

        return success;
    }

    private void PlayEnteredSound()
    {
        SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(Position, Map));
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        base.Destroy(mode);
        KillAndDropContent(mode);
    }

    public void KillAndDropContent(DestroyMode mode = DestroyMode.Vanish)
    {
        if (innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
        {
            if (mode != DestroyMode.Deconstruct)
                foreach (Thing t in innerContainer)
                    if (t is Pawn p)
                        HealthUtility.DamageUntilDowned(p);

            innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
        }
        innerContainer.ClearAndDestroyContents();
    }

    public void BurnContainedPawn()
    {
        if (!HasAnyContents)
            return;

        foreach (Thing t in innerContainer)
            if (t is Pawn p)
                HealthUtility.DamageUntilDead(p, DamageDefOf.Burn);

        innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
        innerContainer.ClearAndDestroyContents();
    }

    public virtual void EjectContents()
    {
        innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
    }

    protected override void BeginBurst()
    {
        base.BeginBurst();

        BurnContainedPawn();
    }

    public override string GetInspectString()
    {
        string baseStr = base.GetInspectString();
        string str = innerContainer.ContentsString;

        if (!baseStr.NullOrEmpty())
            baseStr += "\n";

        return baseStr + ("Contains".Translate() + ": " + str.CapitalizeFirst());
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
    {
        if (myPawn.IsQuestLodger())
            yield return new FloatMenuOption("CannotUseReason".Translate("CryptosleepCasketGuestsNotAllowed".Translate()), null);

        foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(myPawn))
            yield return floatMenuOption;

        if (innerContainer.Count != 0)
            yield break;

        if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);

        JobDef jobDef = USH_DefOf.USH_EnterBiocoder;
        string label = "USH_GE_EnterBiocoder".Translate();

        void action()
        {
            if (ModsConfig.BiotechActive)
            {
                if (myPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) is not Hediff_PsychicBond hediff_PsychicBond || !ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson(myPawn, hediff_PsychicBond))
                {
                    myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, this), JobTag.Misc);
                }
                else
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("PsychicBondDistanceWillBeActive_Cryptosleep".Translate(myPawn.Named("PAWN"), ((Pawn)hediff_PsychicBond.target).Named("BOND")), delegate
                    {
                        myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, this), JobTag.Misc);
                    }, destructive: true));
                }
            }
            else
            {
                myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, this), JobTag.Misc);
            }
        }
        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), myPawn, this);
    }

    public static Building_Biocoder FindBiocoderFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
    {

        bool queuing = KeyBindingDefOf.QueueOrder.IsDownEvent;
        Building_Biocoder biocoder = (Building_Biocoder)GenClosest.ClosestThingReachable(p.PositionHeld, p.MapHeld, ThingRequest.ForDef(USH_DefOf.USH_GlittertechTargeter), PathEndMode.InteractionCell, TraverseParms.For(traveler), 9999f, Validator);

        if (biocoder != null)
            return biocoder;

        bool Validator(Thing x)
        {
            if (!((Building_Biocoder)x).HasAnyContents && (!queuing || !traveler.HasReserved(x)))
            {
                return traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations);
            }
            return false;
        }

        return null;
    }

    //disable rendering of the turret
    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {

    }
}
