using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;
using System.Text;

namespace GliterworldUprising
{
    [StaticConstructorOnStartup]
    public class CompMountainRaiser : ThingComp
    {
        Map map;
        private int tickFromStart = -1;

        public CompProperties_MountainRaiser Props => (CompProperties_MountainRaiser)this.props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            map = this.parent.Map;
        }

        public override void CompTick()
        {
            base.CompTick();
            tickFromStart++;
            if(tickFromStart >= this.Props.timeOffset)
            {
                raiseTheWall(this.parent.Stuff);
            }
        }

        public void raiseTheWall(ThingDef stuff)
        {
            foreach (GlitterThingToTurnInto entry in Props.recipes)
            {
                if (entry.item == stuff)
                {

                    //Generate the wall
                    Thing building = ThingMaker.MakeThing(entry.building);
                    GenPlace.TryPlaceThing(building, this.parent.Position, map,ThingPlaceMode.Direct);

                    //Play the sound
                    if(this.Props.raiseSound != null)
                        this.Props.raiseSound.PlayOneShot((SoundInfo)new TargetInfo(this.parent.Position, this.map));

                    //Make particles
                    if (this.Props.mote != null || this.Props.fleck != null)
                    {
                        Vector3 drawPos = this.parent.DrawPos;
                        for (int index = 0; index < this.Props.moteCount; ++index)
                        {
                            Vector2 vector2 = Rand.InsideUnitCircle * this.Props.moteOffsetRange.RandomInRange * (float)Rand.Sign;
                            Vector3 loc = new Vector3(drawPos.x + vector2.x, drawPos.y, drawPos.z + vector2.y);
                            if (this.Props.mote != null)
                                MoteMaker.MakeStaticMote(loc, this.map, this.Props.mote);
                            else
                                FleckMaker.Static(loc, this.map, this.Props.fleck);
                        }
                    }

                }
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this != null)
            {
                stringBuilder.Append((string)"USH_GU_Reconstructing".Translate() + ": " + (this.Props.timeOffset - tickFromStart).ToString() + " " + "USH_GU_TimeLeft".Translate());
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString().TrimEnd();
        }
    }

}
