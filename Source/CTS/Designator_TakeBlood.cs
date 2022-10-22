using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CTS;

internal class Designator_TakeBlood : Designator
{
    public bool didWeDesignateAnything;

    public Designator_TakeBlood()
    {
        defaultLabel = "CTS_BloodLabel".Translate();
        icon = ContentFinder<Texture2D>.Get("CTS/Items/BloodBag");
        defaultDesc = "CTS_BloodInfo".Translate();
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

    public override int DraggableDimensions => 2;

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (!c.InBounds(Map) || c.Fogged(Map))
        {
            return false;
        }

        var value = false;
        foreach (var t in c.GetThingList(Map))
        {
            if (CanDesignateThing(t).Accepted)
            {
                value = true;
            }
        }

        return value;
    }

    public override AcceptanceReport CanDesignateThing(Thing t)
    {
        if (t is not Pawn pawn1)
        {
            return false;
        }

        bool any;
        if (!pawn1.IsColonist)
        {
            any = !pawn1.IsPrisonerOfColony || pawn1.BillStack.Bills.Any(x => x.recipe.defName == "CTSTakeBlood");
        }
        else
        {
            any = false;
        }

        return !any;
    }

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

    public void NotifyResult()
    {
        if (didWeDesignateAnything)
        {
            SoundDefOf.Designate_Deconstruct.PlayOneShotOnCamera();
        }
        else
        {
            Messages.Message("CTS_MessageAlready".Translate(),
                MessageTypeDefOf.CautionInput);
            soundFailed = SoundDefOf.Designate_Failed;
        }

        didWeDesignateAnything = false;
    }

    public override void DesignateThing(Thing t)
    {
        var pawn = (Pawn)t;
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
                if (recipeDef.defName != "CTSTakeBlood")
                {
                    continue;
                }

                var bill_Medical = new Bill_Medical(recipeDef, null);
                pawn.BillStack.AddBill(bill_Medical);
                bill_Medical.Part = bodyPartRecord;
                didWeDesignateAnything = true;
            }
        }

        didWeDesignateAnything = true;
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
    }

    protected override void FinalizeDesignationSucceeded()
    {
        base.FinalizeDesignationSucceeded();
        Messages.Message("CTS_MessagePawns".Translate(), MessageTypeDefOf.CautionInput);
        soundSucceeded = SoundDefOf.Designate_DragAreaAdd;
        didWeDesignateAnything = false;
    }

    protected override void FinalizeDesignationFailed()
    {
        base.FinalizeDesignationFailed();
        Messages.Message("CTS_MessageDesignate".Translate(), MessageTypeDefOf.CautionInput);
        soundFailed = SoundDefOf.Designate_Failed;
        didWeDesignateAnything = false;
    }
}