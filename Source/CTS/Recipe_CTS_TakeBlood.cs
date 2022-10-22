using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CTS;

public class Recipe_CTS_TakeBlood : Recipe_InstallImplant
{
    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients,
        Bill bill)
    {
        if (billDoer != null)
        {
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
        }

        pawn.health.AddHediff(recipe.addsHediff, part);
    }
}