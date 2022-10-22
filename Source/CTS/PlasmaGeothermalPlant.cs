using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CTS;

[StaticConstructorOnStartup]
public class PlasmaGeothermalPlant : Building
{
    private static readonly SoundDef SoundHiss = SoundDef.Named("PowerOn");

    private static readonly string FramePath = "CTS/PlasmaGeothermalPlantFrames/GeothermalPlantF";

    private static readonly int FrameCount = 12;

    private static readonly Graphic[] TexResFrames = new Graphic[FrameCount];

    public float basePowerConsumption;

    private CompBreakdownable breakdownableComp;

    public CompTempControl compTempControl;

    private CompFlickable flickableComp;

    private CompPowerTrader powerComp;

    private CompRefuelable refuelableComp;

    private Graphic TexMain;

    private int timer;

    static PlasmaGeothermalPlant()
    {
        checked
        {
            for (var i = 0; i < FrameCount; i++)
            {
                TexResFrames[i] = GraphicDatabase.Get<Graphic_Single>(FramePath + (i + 1),
                    ThingDef.Named("CTSPlasmaGeothermalPlant").graphicData.Graphic.Shader);
                TexResFrames[i].drawSize = ThingDef.Named("CTSPlasmaGeothermalPlant").graphicData.drawSize;
            }
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        powerComp = GetComp<CompPowerTrader>();
        powerComp.PowerOn = true;
        flickableComp = GetComp<CompFlickable>();
        refuelableComp = GetComp<CompRefuelable>();
        breakdownableComp = GetComp<CompBreakdownable>();
    }

    public override void Tick()
    {
        base.Tick();
        checked
        {
            if (breakdownableComp is { BrokenDown: true } ||
                refuelableComp is { HasFuel: false } ||
                flickableComp is { SwitchIsOn: false } || powerComp is { PowerOn: false })
            {
                powerComp.PowerOutput = 0f;
                timer = 0;
                HandleAnimation();
            }
            else
            {
                if (powerComp != null)
                {
                    powerComp.PowerOutput =
                        Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare) ? 10000f : 5000f;
                }

                timer++;
                if (timer >= TexResFrames.Length * 9)
                {
                    timer = 0;
                }

                if (Rand.Value < 0.1f)
                {
                    FleckMaker.ThrowAirPuffUp(this.TrueCenter(), Map);
                }

                HandleAnimation();
            }
        }
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        stringBuilder.Append("CTS_PowerOutput".Translate(powerComp.PowerOutput));
        return stringBuilder.ToString();
    }

    private void HandleAnimation()
    {
        if (timer >= checked(TexResFrames.Length * 9))
        {
            return;
        }

        var num = timer / 9;
        TexMain = TexResFrames[num];
        TexMain.color = base.Graphic.color;
    }

    public override void Draw()
    {
        base.Draw();
        if (!powerComp.PowerOn || TexMain == null)
        {
            return;
        }

        var matrix4x = default(Matrix4x4);
        var vector = new Vector3(4f, 1f, 4f);
        matrix4x.SetTRS(DrawPos + Altitudes.AltIncVect, Rotation.AsQuat, vector);
        Graphics.DrawMesh(MeshPool.plane10, matrix4x, TexMain.MatAt(Rotation), 0);
    }
}