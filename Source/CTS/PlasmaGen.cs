using System;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CTS;

[StaticConstructorOnStartup]
public class PlasmaGen : Building
{
    private static readonly SoundDef SoundHiss = SoundDef.Named("PowerOn");

    private static readonly string FramePath = "CTS/PlasmaGenFrames/PlasmaGenF";

    private static readonly string FramePath2 = "CTS/PlasmaGenFrames/PlasmaGenVF";

    private static readonly int FrameCount = 12;

    private static readonly Graphic[] TexResFrames = new Graphic[FrameCount];

    private static readonly Graphic[] TexResFrames2;
    private static readonly GameConditionDef solarFlare = GameConditionDef.Named("SolarFlare");

    private int Charges;

    private int ChargesString;

    public CompTempControl compTempControl;

    private CompGlower glowerComp;

    private CompPowerTrader powerComp;

    private int Stage;

    private Graphic TexMain;

    private int timer;

    static PlasmaGen()
    {
        checked
        {
            for (var i = 0; i < FrameCount; i++)
            {
                TexResFrames[i] = GraphicDatabase.Get<Graphic_Single>(FramePath + (i + 1),
                    ThingDef.Named("CTSPlasmaGen").graphicData.Graphic.Shader);
                TexResFrames[i].drawSize = ThingDef.Named("CTSPlasmaGen").graphicData.drawSize;
            }

            TexResFrames2 = new Graphic[FrameCount];
            for (var i = 0; i < FrameCount; i++)
            {
                TexResFrames2[i] = GraphicDatabase.Get<Graphic_Single>(FramePath2 + (i + 1),
                    ThingDef.Named("CTSPlasmaGen").graphicData.Graphic.Shader);
                TexResFrames2[i].drawSize = ThingDef.Named("CTSPlasmaGen").graphicData.drawSize;
            }
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        powerComp = GetComp<CompPowerTrader>();
        glowerComp = GetComp<CompGlower>();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref timer, "timer");
        Scribe_Values.Look(ref Charges, "Charges");
        Scribe_Values.Look(ref ChargesString, "ChargesString");
        Scribe_Values.Look(ref Stage, "Stage");
    }

    protected override void Tick()
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
                switch (Charges)
                {
                    case >= 90000:
                        Stage = 1;
                        break;
                    case < 90000:
                        Charges++;
                        powerComp.PowerOutput = -1000f;
                        break;
                }

                switch (Stage)
                {
                    case <= 0:
                    {
                        if (Position.GetTemperature(Map) > -100f)
                        {
                            GenTemperature.PushHeat(this, -0.5f);
                        }

                        if (Math.IEEERemainder(Charges, 4500.0) == 1.0)
                        {
                            ChargesString++;
                        }

                        break;
                    }
                    case > 0:
                    {
                        powerComp.PowerOutput =
                            Map.gameConditionManager.ConditionIsActive(solarFlare) ? 2000f : 1000f;

                        if (Position.GetTemperature(Map) < 100f)
                        {
                            GenTemperature.PushHeat(this, 0.5f);
                        }

                        break;
                    }
                }

                return;
            }

            if (powerComp.PowerOn || Stage > 0)
            {
                return;
            }

            Charges = 0;
            ChargesString = 0;
        }
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        stringBuilder.Append("CTS_PowerOutput".Translate(powerComp.PowerOutput));
        checked
        {
            if (Stage > 0)
            {
                return stringBuilder.ToString();
            }

            stringBuilder.AppendLine();
            stringBuilder.Append("CTS_ChargingProgress".Translate((Charges / 90000f).ToStringPercent()));
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

    private void HandleAnimation()
    {
        if (timer >= checked(TexResFrames.Length * 3))
        {
            return;
        }

        var num = timer / 3;
        switch (Stage)
        {
            case <= 0:
                TexMain = TexResFrames2[num];
                break;
            case > 0:
                TexMain = TexResFrames[num];
                break;
        }

        TexMain.color = base.Graphic.color;
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        if (!powerComp.PowerOn || TexMain == null)
        {
            return;
        }

        var matrix4x = default(Matrix4x4);
        var vector = new Vector3(2f, 1f, 2f);
        matrix4x.SetTRS(drawLoc + Altitudes.AltIncVect, Rotation.AsQuat, vector);
        Graphics.DrawMesh(MeshPool.plane10, matrix4x, TexMain.MatAt(Rotation), 0);
    }
}