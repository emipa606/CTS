using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CTS
{
    // Token: 0x02000005 RID: 5
    internal class Designator_TakeBlood : Designator
    {
        // Token: 0x04000029 RID: 41
        public bool didWeDesignateAnything;

        // Token: 0x06000019 RID: 25 RVA: 0x00002FEC File Offset: 0x00001FEC
        public Designator_TakeBlood()
        {
            defaultLabel = "Take Blood";
            icon = ContentFinder<Texture2D>.Get("CTS/Items/BloodBag");
            defaultDesc = "Quickly designate Pawns to add the take blood medical bill.";
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;
            soundSucceeded = SoundDefOf.Designate_Haul;
            var named = DefDatabase<DesignationCategoryDef>.GetNamed("Orders");
            var type = named.specialDesignatorClasses.Find(x => x == GetType());
            if (type != null)
            {
                return;
            }

            named.specialDesignatorClasses.Add(GetType());
            named.ResolveReferences();
            var named2 = DefDatabase<DesignationCategoryDef>.GetNamed("OrdersTakeBloodAll");
            var allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
            allDefsListForReading.Remove(named2);
            DefDatabase<DesignationCategoryDef>.ResolveAllReferences();
        }

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000018 RID: 24 RVA: 0x00002FBC File Offset: 0x00001FBC
        public override int DraggableDimensions => 2;

        // Token: 0x0600001A RID: 26 RVA: 0x000030D4 File Offset: 0x000020D4
        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            AcceptanceReport result;
            if (!c.InBounds(Map) || c.Fogged(Map))
            {
                result = false;
            }
            else
            {
                var value = false;
                foreach (var t in c.GetThingList(Map))
                {
                    if (CanDesignateThing(t).Accepted)
                    {
                        value = true;
                    }
                }

                result = value;
            }

            return result;
        }

        // Token: 0x0600001B RID: 27 RVA: 0x000031B8 File Offset: 0x000021B8
        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            AcceptanceReport result;
            if (!(t is Pawn pawn1))
            {
                result = false;
            }
            else
            {
                var pawn = pawn1;
                bool any;
                if (!pawn.IsColonist)
                {
                    any = !pawn.IsPrisonerOfColony || pawn.BillStack.Bills.Any(x => x.recipe.defName == "CTSTakeBlood");
                }
                else
                {
                    any = false;
                }

                result = !any;
            }

            return result;
        }

        // Token: 0x0600001C RID: 28 RVA: 0x0000324C File Offset: 0x0000224C
        public override void DesignateSingleCell(IntVec3 c)
        {
            foreach (var t in c.GetThingList(Map))
            {
                if (CanDesignateThing(t).Accepted)
                {
                    DesignateThing(t);
                }
            }

            NotifyResult();
        }

        // Token: 0x0600001D RID: 29 RVA: 0x000032D0 File Offset: 0x000022D0
        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            foreach (var c in cells)
            {
                foreach (var t in c.GetThingList(Map))
                {
                    if (CanDesignateThing(t).Accepted)
                    {
                        DesignateThing(t);
                    }
                }
            }

            NotifyResult();
        }

        // Token: 0x0600001E RID: 30 RVA: 0x00003398 File Offset: 0x00002398
        public void NotifyResult()
        {
            if (didWeDesignateAnything)
            {
                SoundDefOf.Designate_Deconstruct.PlayOneShotOnCamera();
            }
            else
            {
                Messages.Message("Must designate colonists that aren't already designated.",
                    MessageTypeDefOf.CautionInput);
                soundFailed = SoundDefOf.Designate_Failed;
            }

            didWeDesignateAnything = false;
        }

        // Token: 0x0600001F RID: 31 RVA: 0x000033E8 File Offset: 0x000023E8
        public override void DesignateThing(Thing t)
        {
            var pawn = (Pawn) t;
            foreach (var recipeDef in pawn.def.AllRecipes)
            {
                if (!recipeDef.AvailableNow)
                {
                    continue;
                }

                var partsToApplyOn = recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef);
                if (!partsToApplyOn.Any())
                {
                    continue;
                }

                foreach (var bodyPartRecord in partsToApplyOn)
                {
                    var recipe = recipeDef;
                    var part = bodyPartRecord;
                    if (recipeDef.defName != "CTSTakeBlood")
                    {
                        continue;
                    }

                    var bill_Medical = new Bill_Medical(recipe);
                    pawn.BillStack.AddBill(bill_Medical);
                    bill_Medical.Part = part;
                    didWeDesignateAnything = true;
                }
            }

            didWeDesignateAnything = true;
        }

        // Token: 0x06000020 RID: 32 RVA: 0x0000351C File Offset: 0x0000251C
        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00003525 File Offset: 0x00002525
        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();
            Messages.Message("Pawns designated for blood taking.", MessageTypeDefOf.CautionInput);
            soundSucceeded = SoundDefOf.Designate_DragAreaAdd;
            didWeDesignateAnything = false;
        }

        // Token: 0x06000022 RID: 34 RVA: 0x00003552 File Offset: 0x00002552
        protected override void FinalizeDesignationFailed()
        {
            base.FinalizeDesignationFailed();
            Messages.Message("Must designate pawns.", MessageTypeDefOf.CautionInput);
            soundFailed = SoundDefOf.Designate_Failed;
            didWeDesignateAnything = false;
        }
    }
}