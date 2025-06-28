using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace USH_GE
{
    public class CompProperties_MountainRaiser : CompProperties
    {
        public float ticksToPlace;
        public FleckDef fleckDef;
        public SoundDef soundDef;
        public List<WallRecipe> recipes = [];

        public CompProperties_MountainRaiser() => compClass = typeof(CompMountainRaiser);
    }

    public class WallRecipe
    {
        public ThingDef ingredient, product;
    }

    public class CompMountainRaiser : ThingComp
    {
        Map _currentMap;
        private int _ticksPassed;

        public CompProperties_MountainRaiser Props => (CompProperties_MountainRaiser)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _currentMap = parent.Map;
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            _ticksPassed = 0;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _ticksPassed, "USH_TicksPassed", 0);
        }

        public override void CompTick()
        {
            base.CompTick();

            _ticksPassed++;
            if (_ticksPassed >= Props.ticksToPlace)
                SpawnThing(parent.Stuff);
        }

        public void SpawnThing(ThingDef ingredient)
        {
            WallRecipe wallRecipe = Props.recipes.Find(x => x.ingredient == ingredient);

            Thing product = ThingMaker.MakeThing(wallRecipe.product);
            GenPlace.TryPlaceThing(product, parent.Position, _currentMap, ThingPlaceMode.Direct);

            SpawnFleckEffect(parent.Position);
            PlaySoundEffect();
        }

        private void SpawnFleckEffect(IntVec3 position) => FleckMaker.Static(position, _currentMap, Props.fleckDef);

        private void PlaySoundEffect() => Props.soundDef.PlayOneShot(new TargetInfo(parent.Position, parent.Map, false));

        public override string CompInspectStringExtra() => "USH_GU_Reconstructing".Translate(SecondsLeft());

        private string SecondsLeft() => ((Props.ticksToPlace - _ticksPassed) / 60f).ToString("0.00");
    }

}
