using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CTS
{
    // Token: 0x02000006 RID: 6
    public class Recipe_CTS_TakeBlood : Recipe_InstallImplant
    {
        // Token: 0x06000025 RID: 37 RVA: 0x00003580 File Offset: 0x00002580
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
}