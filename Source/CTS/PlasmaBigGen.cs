using System;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CTS
{
    // Token: 0x02000002 RID: 2
    [StaticConstructorOnStartup]
    public class PlasmaBigGen : Building
    {
        // Token: 0x04000006 RID: 6
        private static readonly SoundDef SoundHiss = SoundDef.Named("PowerOn");

        // Token: 0x04000008 RID: 8
        private static readonly string FramePath = "CTS/PlasmaBigGenFrames/PlasmaBigGenF";

        // Token: 0x04000009 RID: 9
        private static readonly string FramePath2 = "CTS/PlasmaBigGenFrames/PlasmaBigGenVF";

        // Token: 0x0400000A RID: 10
        private static readonly int FrameCount = 12;

        // Token: 0x0400000C RID: 12
        private static readonly Graphic[] TexResFrames = new Graphic[FrameCount];

        // Token: 0x0400000D RID: 13
        private static readonly Graphic[] TexResFrames2;

        // Token: 0x04000003 RID: 3
        private int Charges;

        // Token: 0x04000002 RID: 2
        private int ChargesString;

        // Token: 0x04000007 RID: 7
        public CompTempControl compTempControl;

        // Token: 0x04000004 RID: 4
        private CompGlower glowerComp;

        // Token: 0x04000005 RID: 5
        private CompPowerTrader powerComp;

        // Token: 0x04000001 RID: 1
        private int Stage;

        // Token: 0x0400000E RID: 14
        private Graphic TexMain;

        // Token: 0x0400000B RID: 11
        private int timer;

        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00001050
        static PlasmaBigGen()
        {
            checked
            {
                for (var i = 0; i < FrameCount; i++)
                {
                    TexResFrames[i] = GraphicDatabase.Get<Graphic_Single>(FramePath + (i + 1),
                        ThingDef.Named("CTSPlasmaBigGen").graphicData.Graphic.Shader);
                    TexResFrames[i].drawSize = ThingDef.Named("CTSPlasmaBigGen").graphicData.drawSize;
                }

                TexResFrames2 = new Graphic[FrameCount];
                for (var i = 0; i < FrameCount; i++)
                {
                    TexResFrames2[i] = GraphicDatabase.Get<Graphic_Single>(FramePath2 + (i + 1),
                        ThingDef.Named("CTSPlasmaBigGen").graphicData.Graphic.Shader);
                    TexResFrames2[i].drawSize = ThingDef.Named("CTSPlasmaBigGen").graphicData.drawSize;
                }
            }
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002181 File Offset: 0x00001181
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            glowerComp = GetComp<CompGlower>();
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000021A8 File Offset: 0x000011A8
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref timer, "timer");
            Scribe_Values.Look(ref Charges, "Charges");
            Scribe_Values.Look(ref ChargesString, "ChargesString");
            Scribe_Values.Look(ref Stage, "Stage");
        }

        // Token: 0x06000004 RID: 4 RVA: 0x0000220C File Offset: 0x0000120C
        public override void Tick()
        {
            base.Tick();
            checked
            {
                if (powerComp.PowerOn)
                {
                    timer++;
                    if (timer >= TexResFrames.Length * 3)
                    {
                        timer = 0;
                    }

                    HandleAnimation();
                    if (Charges >= 90000)
                    {
                        Stage = 1;
                    }

                    if (Charges < 90000)
                    {
                        Charges++;
                        powerComp.PowerOutput = -10000f;
                    }

                    if (Stage <= 0)
                    {
                        if (Position.GetTemperature(Map) > -100f)
                        {
                            GenTemperature.PushHeat(this, -0.5f);
                        }

                        if (Math.IEEERemainder(Charges, 4500.0) == 1.0)
                        {
                            ChargesString++;
                        }
                    }
                    else if (Stage > 0)
                    {
                        powerComp.PowerOutput =
                            Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare) ? 20000f : 10000f;

                        if (Position.GetTemperature(Map) < 100f)
                        {
                            GenTemperature.PushHeat(this, 0.5f);
                        }
                    }
                }
                else if (!powerComp.PowerOn && Stage <= 0)
                {
                    Charges = 0;
                    ChargesString = 0;
                }
            }
        }

        // Token: 0x06000005 RID: 5 RVA: 0x000023E8 File Offset: 0x000013E8
        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.Append("Power output: " + powerComp.PowerOutput + " W");
            checked
            {
                if (Stage > 0)
                {
                    return stringBuilder.ToString();
                }

                stringBuilder.AppendLine();
                stringBuilder.Append("Charging Progress: " + (int) (Charges / 90000f * 100f) + "%");
                stringBuilder.AppendLine();
                stringBuilder.Append("[");
                for (var i = 0; i < ChargesString; i++)
                {
                    stringBuilder.Append("■");
                }

                for (var j = 20; j > ChargesString; j--)
                {
                    stringBuilder.Append("□");
                }

                stringBuilder.Append("]");

                return stringBuilder.ToString();
            }
        }

        // Token: 0x06000006 RID: 6 RVA: 0x000024F0 File Offset: 0x000014F0
        private void HandleAnimation()
        {
            if (timer >= checked(TexResFrames.Length * 3))
            {
                return;
            }

            var num = timer / 3;
            if (Stage <= 0)
            {
                TexMain = TexResFrames2[num];
            }
            else if (Stage > 0)
            {
                TexMain = TexResFrames[num];
            }

            TexMain.color = base.Graphic.color;
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00002578 File Offset: 0x00001578
        public override void Draw()
        {
            base.Draw();
            if (!powerComp.PowerOn || TexMain == null)
            {
                return;
            }

            var matrix4x = default(Matrix4x4);
            var vector = new Vector3(6f, 1f, 6f);
            matrix4x.SetTRS(DrawPos + Altitudes.AltIncVect, Rotation.AsQuat, vector);
            Graphics.DrawMesh(MeshPool.plane10, matrix4x, TexMain.MatAt(Rotation), 0);
        }
    }
}