using RimWorld;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GliterworldUprising
{
    public class CompProperties_MobileComforter : CompProperties
    {
        public float joyGain;
        public int doseOffset;
        public SoundDef doseSound;
        public FleckDef fleck;
        public ThingDef mote;
        public int moteCount = 3;
        public FloatRange moteOffsetRange = new FloatRange(0.2f, 0.4f);
        public CompProperties_MobileComforter() => this.compClass = typeof(CompMobileComforter);
    }

    public class CompMobileComforter : ThingComp
    {
        Map map;
        private int nextDoseTick = -1;
        private bool isReady;

        public CompProperties_MobileComforter Props => (CompProperties_MobileComforter)this.props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            map = this.parent.Map;

        }

        public override void CompTick()
        {
            base.CompTick();

            checkForUser();

            int ticksGame = Find.TickManager.TicksGame;
            if (this.nextDoseTick == -1)
                this.nextDoseTick = ticksGame + this.Props.doseOffset;
            else if (ticksGame >= this.nextDoseTick)
            {
                this.nextDoseTick = ticksGame + this.Props.doseOffset;
                isReady = true;
            }
        }

        public void checkForUser()
        {

            foreach (Thing thing in map.thingGrid.ThingsAt(this.parent.InteractionCell))
            {
                if (thing is Pawn pawn)
                {
                    tryToCheerUp(pawn);
                }
                break;
            }
        }

        public void tryToCheerUp(Pawn cheeree)
        {
            if (cheeree.CurJob.def.defName == "USH_UseMobileComforter" && isReady)
            {
                //Make a sound
                if (this.Props.doseSound != null)
                    this.Props.doseSound.PlayOneShot((SoundInfo)new TargetInfo(this.parent.Position, this.map));
                //Give joy
                cheeree.needs.joy.GainJoy(this.Props.joyGain, cheeree.CurJob.def.joyKind);
                isReady = false;
                //Spawn particles
                if (this.Props.mote != null || this.Props.fleck != null)
                {
                    Vector3 drawPos = cheeree.DrawPos;
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



        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this != null)
            {
                if (isReady)
                    stringBuilder.Append((string)"USH_GU_Ready".Translate());
                else
                    stringBuilder.Append((string)"USH_GU_WarmingUp".Translate() + ": " + (nextDoseTick - Find.TickManager.TicksGame).ToString());
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString().TrimEnd();
        }

    }
}
