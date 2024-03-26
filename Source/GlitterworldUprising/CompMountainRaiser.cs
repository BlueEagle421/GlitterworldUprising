using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.Sound;

namespace GliterworldUprising
{
    public class CompProperties_MountainRaiser : CompProperties
    {
        public float ticksToPlace;
        public FleckDef fleckDef;
        public SoundDef soundDef;
        public List<GlitterThingToTurnInto> recipes = new List<GlitterThingToTurnInto>();

        public CompProperties_MountainRaiser() => compClass = typeof(CompMountainRaiser);
    }

    public class GlitterThingToTurnInto
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

        public override void CompTick()
        {
            base.CompTick();
            _ticksPassed++;
            if (_ticksPassed >= Props.ticksToPlace)
                raiseTheWall(parent.Stuff);
        }

        public void raiseTheWall(ThingDef stuff)
        {
            foreach (GlitterThingToTurnInto entry in Props.recipes)
            {
                if (entry.ingredient == stuff)
                {

                    //Generate the wall
                    Thing building = ThingMaker.MakeThing(entry.product);
                    GenPlace.TryPlaceThing(building, parent.Position, _currentMap, ThingPlaceMode.Direct);

                    //Play the sound
                    if (Props.soundDef != null)
                        Props.soundDef.PlayOneShot((SoundInfo)new TargetInfo(parent.Position, _currentMap));

                    //Make particles
                    FleckMaker.Static(parent.Position, _currentMap, Props.fleckDef);

                }
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this != null)
            {
                stringBuilder.Append((string)"USH_GU_Reconstructing".Translate() + ": " + (Props.ticksToPlace - _ticksPassed).ToString() + " " + "USH_GU_TimeLeft".Translate());
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString().TrimEnd();
        }
    }

}
