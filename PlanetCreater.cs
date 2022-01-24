using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Net;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Security;
using System.Security.Permissions;

namespace DSPAllPlanetInfo
{
    internal class PlanetCreater
    {
        public static IEnumerator Create(PlanetData　planetData)

		//public static void Create(PlanetData planetData)
		{
			try
			{
				PlanetAlgorithm planetAlgorithm = PlanetModelingManager.Algorithm(planetData);
				if (planetAlgorithm != null)
				{
					HighStopwatch highStopwatch = new HighStopwatch();
					double num2 = 0.0;
					double num3 = 0.0;
					double num4 = 0.0;
					if (planetData.data == null)
					{
						highStopwatch.Begin();
						planetData.data = new PlanetRawData(planetData.precision);
						planetData.modData = planetData.data.InitModData(planetData.modData);
						planetData.data.CalcVerts();
						planetData.aux = new PlanetAuxData(planetData);
						planetAlgorithm.GenerateTerrain(planetData.mod_x, planetData.mod_y);
						planetAlgorithm.CalcWaterPercent();
						num2 = highStopwatch.duration;
					}
					if (planetData.factory == null)
					{
						highStopwatch.Begin();
						if (planetData.type != EPlanetType.Gas)
						{
							planetAlgorithm.GenerateVegetables();
						}
						num3 = highStopwatch.duration;
						highStopwatch.Begin();
						if (planetData.type != EPlanetType.Gas)
						{
							planetAlgorithm.GenerateVeins(false);
						}
						num4 = highStopwatch.duration;
					}
					if (planetData.landPercentDirty)
					{
						PlanetAlgorithm.CalcLandPercent(planetData);
						planetData.landPercentDirty = false;
					}
					planetData.loaded = true;

					if (PlanetModelingManager.planetComputeThreadLogs != null)
					{
						List<string> obj4 = PlanetModelingManager.planetComputeThreadLogs;
						lock (obj4)
						{
							PlanetModelingManager.planetComputeThreadLogs.Add(string.Format("{0}\r\nGenerate Terrain {1:F5} s\r\nGenerate Vegetables {2:F5} s\r\nGenerate Veins {3:F5} s\r\n", new object[]
							{
									planetData.displayName,
									num2,
									num3,
									num4
							}));
						}
					}
				}
			}
			catch (Exception ex)
			{
				string obj5 = PlanetModelingManager.planetComputeThreadError;
				lock (obj5)
				{
					if (string.IsNullOrEmpty(PlanetModelingManager.planetComputeThreadError))
					{
						PlanetModelingManager.planetComputeThreadError = ex.ToString();
					}
				}
			}
			yield return null;
		}



    }
}
