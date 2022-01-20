using CSGOExternal.Classes.SDK;
using GameOverlay.Drawing;
using GameOverlay.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSGOExternal.Classes
{
    internal class Cheat
    {
        private static GraphicsWindow _window;

        private static Dictionary<string, SolidBrush> _brushes;
        private static Dictionary<string, Font> _fonts;

        public static void Init()
        {
            _brushes = new Dictionary<string, SolidBrush>();
            _fonts = new Dictionary<string, Font>();

			var gfx = new Graphics()
			{
				UseMultiThreadedFactories = true,
				MeasureFPS = true,
				PerPrimitiveAntiAliasing = true,
				TextAntiAliasing = true
			};

			_window = new GraphicsWindow(25, 25, Settings.RadarWidth, Settings.RadarHeight, gfx)
			{
				FPS = 60,
				IsTopmost = true,
				IsVisible = true
			};

			_window.DestroyGraphics += _window_DestroyGraphics;
			_window.DrawGraphics += _window_DrawGraphics;
			_window.SetupGraphics += _window_SetupGraphics;

			_window.Create();
			_window.Join();
		}

        private static void _window_SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
			var gfx = e.Graphics;

			if (e.RecreateResources)
			{
				foreach (var pair in _brushes) pair.Value.Dispose();
			}

			_brushes["black"] = gfx.CreateSolidBrush(0, 0, 0);
			_brushes["white"] = gfx.CreateSolidBrush(255, 255, 255);
			_brushes["red"] = gfx.CreateSolidBrush(255, 0, 0);
			_brushes["green"] = gfx.CreateSolidBrush(0, 255, 0);
			_brushes["blue"] = gfx.CreateSolidBrush(0, 0, 255);
			_brushes["background"] = gfx.CreateSolidBrush(0x33, 0x36, 0x3F);
			_brushes["grid"] = gfx.CreateSolidBrush(255, 255, 255, 0.2f);
			_brushes["random"] = gfx.CreateSolidBrush(0, 0, 0);

			if (e.RecreateResources) return;

			_fonts["arial"] = gfx.CreateFont("Arial", 12);
			_fonts["consolas"] = gfx.CreateFont("Consolas", 14);
		}

        private static void _window_DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
			var gfx = e.Graphics;
			gfx.ClearScene(gfx.CreateSolidBrush(0, 0, 0, 0));

			if (Utils.IsKeyPushedDown(System.Windows.Forms.Keys.F1))
            {
				Settings.Radar = !Settings.Radar;
				Thread.Sleep(150);
			}

			if (Utils.IsKeyPushedDown(System.Windows.Forms.Keys.F2))
            {
				Settings.Radar_DrawTeammates = !Settings.Radar_DrawTeammates;
				Thread.Sleep(150);
			}

			if (Utils.IsKeyPushedDown(System.Windows.Forms.Keys.F3))
            {
				Settings.Radar_DrawName = !Settings.Radar_DrawName;
				Thread.Sleep(150);
			}	

			if (Utils.IsKeyPushedDown(System.Windows.Forms.Keys.F4))
            {
				Settings.Radar_DrawDistance = !Settings.Radar_DrawDistance;
				Thread.Sleep(150);
			}

			if (Utils.IsKeyPushedDown(System.Windows.Forms.Keys.F6))
            {
				Settings.Radar_Snaplines = !Settings.Radar_Snaplines;
				Thread.Sleep(150);
			}

			if (Utils.IsKeyPushedDown(System.Windows.Forms.Keys.F7))
            {
				Settings.Window_Streamproof = !Settings.Window_Streamproof;
				Settings.Streamproof_Init = false;
				Thread.Sleep(150);
			}

			if (!Settings.Streamproof_Init)
            {
				WinAPI.SetWindowDisplayAffinity(_window.Handle, Settings.Window_Streamproof ? WinAPI.DisplayAffinity.Monitor : WinAPI.DisplayAffinity.None);
				Settings.Streamproof_Init = true;
			}

			if(Settings.Radar)
            {
				int ClientState = Memory.Read<int>(Game.Engine + Offsets.signatures.dwClientState);
				int ClientState_State = Memory.Read<int>(ClientState + Offsets.signatures.dwClientState_State);
				if (ClientState_State == 6) // SignOnState, FULL = 6 --> if local player in game
				{
					// Draw Radar Base
					gfx.DrawBox2D(gfx.CreateSolidBrush(0, 0, 0), gfx.CreateSolidBrush(0, 0, 0, 128), 0, 0, Settings.RadarWidth, Settings.RadarHeight, 1f);
					gfx.DrawLine(gfx.CreateSolidBrush(0, 0, 0), new Line(Settings.RadarWidth / 2, 0, Settings.RadarWidth / 2, Settings.RadarHeight), 1f); //  __
					gfx.DrawLine(gfx.CreateSolidBrush(0, 0, 0), new Line(0, Settings.RadarHeight / 2, Settings.RadarWidth, Settings.RadarHeight / 2), 1f); //  |

					int ClientState_MaxPlayer = Memory.Read<int>(ClientState + Offsets.signatures.dwClientState_MaxPlayer);
					int LocalPlayer = Memory.Read<int>(Game.Client + Offsets.signatures.dwLocalPlayer);
					int LocalPlayer_Team = Memory.Read<int>(LocalPlayer + Offsets.netvars.m_iTeamNum);
					Vector3 LocalPlayer_Origin = Memory.Read<Vector3>(LocalPlayer + Offsets.netvars.m_vecOrigin);
					string LocalPlayer_LastPlaceName = Memory.ReadString(LocalPlayer + Offsets.netvars.m_szLastPlaceName, 64, Encoding.ASCII);

					for (int entity_id = 1; entity_id < ClientState_MaxPlayer; entity_id++)
					{
						int Entity = Memory.Read<int>(Game.Client + Offsets.signatures.dwEntityList + (entity_id * 0x10)); // Entity Size = 0x10
						if (Entity == 0 && Entity == LocalPlayer) continue;
						int Entity_Health = Memory.Read<int>(Entity + Offsets.netvars.m_iHealth);
						if (Entity_Health <= 0) continue;
						int Entity_Team = Memory.Read<int>(Entity + Offsets.netvars.m_iTeamNum);
						if (Entity_Team == 0) continue; // Team.Spectator = 0;
						bool Dormant = Memory.Read<bool>(Entity + Offsets.signatures.m_bDormant);
						if (Dormant) continue;
						Vector3 Entity_Origin = Memory.Read<Vector3>(Entity + Offsets.netvars.m_vecOrigin);
						var items = Memory.Read<int>(Memory.Read<int>(Memory.Read<int>(ClientState + Offsets.signatures.dwClientState_PlayerInfo) + 0x40) + 0x0C);
						Structs.player_info_s Entity_Info = Memory.Read<Structs.player_info_s>(Memory.Read<int>(items + 0x28 + ((entity_id) * 0x34)));
						string Entity_Name = new string(Entity_Info.m_szPlayerName).TrimEnd();

						if (Settings.Radar_DrawTeammates && LocalPlayer_Team == Entity_Team) continue;

						// Radar Info
						float RadarX = (LocalPlayer_Origin.X - Entity_Origin.X) * Settings.Radar_Scale;
						float RadarY = (LocalPlayer_Origin.Y - Entity_Origin.Y) * Settings.Radar_Scale;

						RadarX += Settings.RadarWidth / 2;
						RadarY += Settings.RadarHeight / 2;

						string Entity_LastPlaceName = Memory.ReadString(Entity + Offsets.netvars.m_szLastPlaceName, 64, Encoding.ASCII);
						Color Radar_Color = LocalPlayer_Team == Entity_Team ? Color.Green : new Color(255, 255, 0, 255);
						Radar_Color = LocalPlayer_LastPlaceName == Entity_LastPlaceName && LocalPlayer_Team != Entity_Team ? Color.Red : Radar_Color;

						if(Settings.Radar_Snaplines)
                        {
							gfx.DrawLine(gfx.CreateSolidBrush(0, 0, 0), new Line(Settings.RadarWidth / 2, Settings.RadarHeight / 2, RadarX, RadarY), 3f);
							gfx.DrawLine(gfx.CreateSolidBrush(Radar_Color), new Line(Settings.RadarWidth / 2, Settings.RadarHeight / 2, RadarX, RadarY), 1f);
						}

						gfx.DrawBox2D(gfx.CreateSolidBrush(0, 0, 0), gfx.CreateSolidBrush(Radar_Color), new Rectangle(RadarX - Settings.Radar_EntitySize, RadarY - Settings.Radar_EntitySize, RadarX + Settings.Radar_EntitySize, RadarY + Settings.Radar_EntitySize), 1f);
						if(Settings.Radar_DrawName)
                        {
							var entity_textSize = gfx.MeasureString(_fonts["consolas"], Entity_Name);
							gfx.DrawText(_fonts["consolas"], gfx.CreateSolidBrush(255, 255, 255), new Point(RadarX - entity_textSize.X / 2, RadarY - entity_textSize.Y * 1.45f), Entity_Name);
						}
						if(Settings.Radar_DrawDistance)
                        {
							float Entity_Distance = (float)Math.Round(Vector3.Distance(LocalPlayer_Origin, Entity_Origin) / 100);
							var entity_DistanceTextSize = gfx.MeasureString(_fonts["consolas"], Entity_Distance.ToString() + "M");
							gfx.DrawText(_fonts["consolas"], gfx.CreateSolidBrush(255, 255, 255), new Point(RadarX - entity_DistanceTextSize.X / 2, RadarY + entity_DistanceTextSize.Y / 2), Entity_Distance.ToString() + "M");
						}
						// END - Radar Info
					}
					var player_textSize = gfx.MeasureString(_fonts["consolas"], LocalPlayer_LastPlaceName);
					gfx.DrawText(_fonts["consolas"], gfx.CreateSolidBrush(255, 255, 255), new Point(4, Settings.RadarHeight - (player_textSize.Y + 2)), LocalPlayer_LastPlaceName);
				}
			}
		}

        private static void _window_DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
        {
			foreach (var pair in _brushes) pair.Value.Dispose();
			foreach (var pair in _fonts) pair.Value.Dispose();
		}
    }
}
