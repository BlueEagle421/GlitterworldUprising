// using System.Collections.Generic;
// using LudeonTK;
// using RimWorld;
// using UnityEngine;
// using Verse;
// using Verse.Sound;

// namespace GlitterworldUprising;

// public class ITab_Upgrades : ITab
// {
//     private float viewHeight = 1000f;

//     private Vector2 scrollPosition;

//     private Bill mouseoverBill;

//     private static readonly Vector2 WinSize = new(420f, 480f);

//     [TweakValue("Interface", 0f, 128f)]
//     private static readonly float PasteX = 48f;

//     [TweakValue("Interface", 0f, 128f)]
//     private static readonly float PasteY = 3f;

//     [TweakValue("Interface", 0f, 32f)]
//     private static readonly float PasteSize = 24f;

//     private const int MAX_BILLS = 15;

//     protected CompUpgradable SelComp => SelThing.TryGetComp<CompUpgradable>();

//     public ITab_Upgrades()
//     {
//         size = WinSize;
//         labelKey = "USH_Upgrades";
//         tutorTag = "Bills";
//     }

//     protected override void FillTab()
//     {
//         PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BillsTab, KnowledgeAmount.FrameDisplayed);

//         var pasteRect = new Rect(WinSize.x - PasteX, PasteY, PasteSize, PasteSize);
//         DrawPasteButton(pasteRect);

//         var listRect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
//         mouseoverBill = SelComp.UpgradesWorkTable.BillStack.DoListing(listRect, CreateRecipeOptions, ref scrollPosition, ref viewHeight);
//     }

//     private void DrawPasteButton(Rect rect)
//     {
//         var clip = BillUtility.Clipboard;
//         if (clip is null)
//             return;

//         bool canPaste = SelComp.parent.def.AllRecipes.Contains(clip.recipe)
//                         && clip.recipe.AvailableNow
//                         && clip.recipe.AvailableOnNow(SelComp.parent);
//         bool limitReached = SelComp.UpgradesWorkTable.BillStack.Count >= MAX_BILLS;

//         if (!canPaste || limitReached)
//         {
//             GUI.color = Color.gray;
//             Widgets.DrawTextureFitted(rect, TexButton.Paste, 1f);
//             GUI.color = Color.white;

//             if (Mouse.IsOver(rect))
//             {
//                 var tipKey = !canPaste
//                     ? "ClipboardBillNotAvailableHere".Translate()
//                     : "PasteBillTip_LimitReached".Translate();
//                 TooltipHandler.TipRegion(rect, $"{tipKey}: {clip.LabelCap}");
//             }
//         }
//         else
//         {
//             if (Widgets.ButtonImageFitted(rect, TexButton.Paste, Color.white))
//                 PasteClipboardBill();

//             if (Mouse.IsOver(rect))
//                 TooltipHandler.TipRegion(rect, "PasteBillTip".Translate() + $": {clip.LabelCap}");
//         }
//     }

//     private void PasteClipboardBill()
//     {
//         var newBill = BillUtility.Clipboard.Clone();
//         newBill.InitializeAfterClone();
//         SelComp.UpgradesWorkTable.BillStack.AddBill(newBill);
//         SoundDefOf.Tick_Low.PlayOneShotOnCamera();
//     }

//     private List<FloatMenuOption> CreateRecipeOptions()
//     {
//         var options = new List<FloatMenuOption>();

//         foreach (var recipe in SelComp.AllRecipes)
//         {
//             if (!recipe.AvailableNow || !recipe.AvailableOnNow(SelComp.UpgradesWorkTable))
//                 continue;

//             AddOption(options, recipe);

//             foreach (var ideo in Faction.OfPlayer.ideos.AllIdeos)
//             {
//                 foreach (var style in ideo.cachedPossibleBuildings)
//                 {
//                     if (style.ThingDef == recipe.ProducedThingDef)
//                         AddOption(options, recipe);
//                 }
//             }
//         }

//         if (!options.Any())
//             options.Add(new FloatMenuOption("NoneBrackets".Translate(), null));

//         return options;
//     }

//     private void AddOption(List<FloatMenuOption> options, RecipeDef recipe)
//     {
//         string label = recipe.LabelCap;

//         options.Add(new FloatMenuOption(
//             label,
//             () => TryAddBill(recipe),
//             iconTex: recipe.UIIcon,
//             shownItemForIcon: recipe.UIIconThing,
//             extraPartOnGUI: rect => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, recipe)
//         )
//         {
//             mouseoverGuiAction = rect => BillUtility.DoBillInfoWindow(SelComp.AllRecipes.IndexOf(recipe), label, rect, recipe),
//             extraPartWidth = 29f,
//             orderInPriority = -recipe.displayPriority
//         });
//     }

//     private void TryAddBill(RecipeDef recipe)
//     {
//         if (!CheckSkillRequirement(recipe))
//             return;

//         var bill = recipe.MakeNewBill();
//         SelComp.UpgradesWorkTable.billStack.AddBill(bill);

//         if (recipe.conceptLearned != null)
//             PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);

//         if (TutorSystem.TutorialMode)
//             TutorSystem.Notify_Event("AddBill-" + recipe.LabelCap.Resolve());
//     }


//     private bool CheckSkillRequirement(RecipeDef recipe)
//     {
//         if (!SelComp.parent.Map.mapPawns.FreeColonists.Any(col => recipe.PawnSatisfiesSkillRequirements(col)))
//         {
//             Bill.CreateNoPawnsWithSkillDialog(recipe);
//             return false;
//         }
//         return true;
//     }

//     public override void TabUpdate()
//     {
//         if (mouseoverBill != null)
//         {
//             mouseoverBill.TryDrawIngredientSearchRadiusOnMap(SelComp.parent.Position);
//             mouseoverBill = null;
//         }
//     }

// }

// public class CompProperties_Upgradable : CompProperties
// {
//     public List<RecipeDef> recipes;
//     public CompProperties_Upgradable() => compClass = typeof(CompUpgradable);
// }

// public class CompUpgradable : ThingComp
// {
//     private Building_UpgradesWorkTable _upgradesWorkTable;
//     public Building_UpgradesWorkTable UpgradesWorkTable => _upgradesWorkTable;
//     public List<RecipeDef> AllRecipes => Props.recipes;
//     public CompProperties_Upgradable Props => (CompProperties_Upgradable)props;

//     public override void PostExposeData()
//     {
//         base.PostExposeData();

//         Scribe_References.Look(ref _upgradesWorkTable, "_upgradesWorkTable");
//     }

//     public override void PostSpawnSetup(bool respawningAfterLoad)
//     {
//         base.PostSpawnSetup(respawningAfterLoad);

//         if (_upgradesWorkTable == null)
//         {
//             _upgradesWorkTable = ThingMaker.MakeThing(USHDefOf.USH_UpgradesWorkTable) as Building_UpgradesWorkTable;
//             _upgradesWorkTable.HitPoints = _upgradesWorkTable.def.BaseMaxHitPoints;
//             _upgradesWorkTable.GraphicOverride = parent.Graphic;
//             _upgradesWorkTable.SetFaction(parent.Faction);
//             GenSpawn.Spawn(_upgradesWorkTable, parent.Position, parent.Map, Rot4.North, WipeMode.FullRefund);
//         }
//         _upgradesWorkTable.Position = parent.Position;
//     }
// }

// public class Building_UpgradesWorkTable : Building_WorkTable
// {
//     public Graphic GraphicOverride { get; set; }
//     public override Graphic Graphic
//     {
//         get
//         {
//             GraphicOverride.color = Color.clear;
//             return GraphicOverride;
//         }
//     }
// }
