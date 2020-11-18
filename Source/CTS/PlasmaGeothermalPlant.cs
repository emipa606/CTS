using System;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CTS
{
	// Token: 0x02000004 RID: 4
	[StaticConstructorOnStartup]
	public class PlasmaGeothermalPlant : Building
	{
		// Token: 0x06000011 RID: 17 RVA: 0x00002C20 File Offset: 0x00001C20
		static PlasmaGeothermalPlant()
		{
			checked
			{
				for (var i = 0; i < FrameCount; i++)
				{
                    TexResFrames[i] = GraphicDatabase.Get<Graphic_Single>(FramePath + (i + 1), ThingDef.Named("CTSPlasmaGeothermalPlant").graphicData.Graphic.Shader);
                    TexResFrames[i].drawSize = ThingDef.Named("CTSPlasmaGeothermalPlant").graphicData.drawSize;
				}
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002CCC File Offset: 0x00001CCC
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			powerComp.PowerOn = true;
			flickableComp = GetComp<CompFlickable>();
			refuelableComp = GetComp<CompRefuelable>();
			breakdownableComp = GetComp<CompBreakdownable>();
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002D20 File Offset: 0x00001D20
		public override void Tick()
		{
			base.Tick();
			checked
			{
				if ((breakdownableComp != null && breakdownableComp.BrokenDown) || (refuelableComp != null && !refuelableComp.HasFuel) || (flickableComp != null && !flickableComp.SwitchIsOn) || (powerComp != null && !powerComp.PowerOn))
				{
					powerComp.PowerOutput = 0f;
					timer = 0;
					HandleAnimation();
				}
				else
				{
					if (Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
					{
						powerComp.PowerOutput = 10000f;
					}
					else
					{
						powerComp.PowerOutput = 5000f;
					}
					timer++;
					if (timer >= TexResFrames.Count() * 9)
					{
						timer = 0;
					}
					if (Rand.Value < 0.1f)
					{
						MoteMaker.ThrowAirPuffUp(GenThing.TrueCenter(this), Map);
					}
					HandleAnimation();
				}
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002E60 File Offset: 0x00001E60
		public override string GetInspectString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.Append("Power output: " + powerComp.PowerOutput + " W");
			return stringBuilder.ToString();
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002EB4 File Offset: 0x00001EB4
		private void HandleAnimation()
		{
			if (timer < checked(TexResFrames.Count() * 9))
			{
				var num = timer / 9;
				TexMain = TexResFrames[num];
				TexMain.color = base.Graphic.color;
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002F10 File Offset: 0x00001F10
		public override void Draw()
		{
			base.Draw();
			if (powerComp.PowerOn && TexMain != null)
			{
				var matrix4x = default(Matrix4x4);
				var vector = new Vector3(4f, 1f, 4f);
				matrix4x.SetTRS(DrawPos + Altitudes.AltIncVect, Rotation.AsQuat, vector);
				Graphics.DrawMesh(MeshPool.plane10, matrix4x, TexMain.MatAt(Rotation, null), 0);
			}
		}

		// Token: 0x0400001D RID: 29
		private static readonly SoundDef SoundHiss = SoundDef.Named("PowerOn");

		// Token: 0x0400001E RID: 30
		public CompTempControl compTempControl;

		// Token: 0x0400001F RID: 31
		private static readonly string FramePath = "CTS/PlasmaGeothermalPlantFrames/GeothermalPlantF";

		// Token: 0x04000020 RID: 32
		private static readonly int FrameCount = 12;

		// Token: 0x04000021 RID: 33
		private int timer = 0;

		// Token: 0x04000022 RID: 34
		private static readonly Graphic[] TexResFrames = new Graphic_Single[FrameCount];

		// Token: 0x04000023 RID: 35
		private Graphic TexMain;

		// Token: 0x04000024 RID: 36
		private CompFlickable flickableComp;

		// Token: 0x04000025 RID: 37
		private CompPowerTrader powerComp;

		// Token: 0x04000026 RID: 38
		private CompRefuelable refuelableComp;

		// Token: 0x04000027 RID: 39
		private CompBreakdownable breakdownableComp;

		// Token: 0x04000028 RID: 40
		public float basePowerConsumption;
	}
}
