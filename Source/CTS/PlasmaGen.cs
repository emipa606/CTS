using System;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CTS
{
	// Token: 0x02000003 RID: 3
	[StaticConstructorOnStartup]
	public class PlasmaGen : Building
	{
		// Token: 0x06000009 RID: 9 RVA: 0x00002638 File Offset: 0x00001638
		static PlasmaGen()
		{
			checked
			{
				for (var i = 0; i < FrameCount; i++)
				{
                    TexResFrames[i] = GraphicDatabase.Get<Graphic_Single>(FramePath + (i + 1), ThingDef.Named("CTSPlasmaGen").graphicData.Graphic.Shader);
                    TexResFrames[i].drawSize = ThingDef.Named("CTSPlasmaGen").graphicData.drawSize;
				}
                TexResFrames2 = new Graphic_Single[FrameCount];
				for (var i = 0; i < FrameCount; i++)
				{
                    TexResFrames2[i] = GraphicDatabase.Get<Graphic_Single>(FramePath2 + (i + 1), ThingDef.Named("CTSPlasmaGen").graphicData.Graphic.Shader);
                    TexResFrames2[i].drawSize = ThingDef.Named("CTSPlasmaGen").graphicData.drawSize;
				}
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002769 File Offset: 0x00001769
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			glowerComp = GetComp<CompGlower>();
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002790 File Offset: 0x00001790
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref timer, "timer", 0, false);
			Scribe_Values.Look(ref Charges, "Charges", 0, false);
			Scribe_Values.Look(ref ChargesString, "ChargesString", 0, false);
			Scribe_Values.Look(ref Stage, "Stage", 0, false);
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000027F4 File Offset: 0x000017F4
		public override void Tick()
		{
			base.Tick();
			checked
			{
				if (powerComp.PowerOn)
				{
					timer++;
					if (timer >= TexResFrames.Count() * 3)
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
						powerComp.PowerOutput = -1000f;
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
						if (Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
						{
							powerComp.PowerOutput = 2000f;
						}
						else
						{
							powerComp.PowerOutput = 1000f;
						}
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

		// Token: 0x0600000D RID: 13 RVA: 0x000029D0 File Offset: 0x000019D0
		public override string GetInspectString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.Append("Power output: " + powerComp.PowerOutput + " W");
			checked
			{
				if (Stage <= 0)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("Charging Progress: " + (int)unchecked(Charges / 90000f * 100f) + "%");
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
				}
				return stringBuilder.ToString();
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002AD8 File Offset: 0x00001AD8
		private void HandleAnimation()
		{
			if (timer < checked(TexResFrames.Count() * 3))
			{
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
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002B60 File Offset: 0x00001B60
		public override void Draw()
		{
			base.Draw();
			if (powerComp.PowerOn && TexMain != null)
			{
				var matrix4x = default(Matrix4x4);
				var vector = new Vector3(2f, 1f, 2f);
				matrix4x.SetTRS(DrawPos + Altitudes.AltIncVect, Rotation.AsQuat, vector);
				Graphics.DrawMesh(MeshPool.plane10, matrix4x, TexMain.MatAt(Rotation, null), 0);
			}
		}

		// Token: 0x0400000F RID: 15
		private int Stage = 0;

		// Token: 0x04000010 RID: 16
		private int ChargesString = 0;

		// Token: 0x04000011 RID: 17
		private int Charges = 0;

		// Token: 0x04000012 RID: 18
		private CompGlower glowerComp;

		// Token: 0x04000013 RID: 19
		private CompPowerTrader powerComp;

		// Token: 0x04000014 RID: 20
		private static readonly SoundDef SoundHiss = SoundDef.Named("PowerOn");

		// Token: 0x04000015 RID: 21
		public CompTempControl compTempControl;

		// Token: 0x04000016 RID: 22
		private static readonly string FramePath = "CTS/PlasmaGenFrames/PlasmaGenF";

		// Token: 0x04000017 RID: 23
		private static readonly string FramePath2 = "CTS/PlasmaGenFrames/PlasmaGenVF";

		// Token: 0x04000018 RID: 24
		private static readonly int FrameCount = 12;

		// Token: 0x04000019 RID: 25
		private int timer = 0;

		// Token: 0x0400001A RID: 26
		private static readonly Graphic[] TexResFrames = new Graphic_Single[FrameCount];

		// Token: 0x0400001B RID: 27
		private static readonly Graphic[] TexResFrames2;

		// Token: 0x0400001C RID: 28
		private Graphic TexMain;
	}
}
