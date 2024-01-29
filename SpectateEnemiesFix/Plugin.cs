using System;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;

namespace SpectateEnemiesFix {
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("SpectateEnemy")]
	public class Plugin : BaseUnityPlugin {

		public static ManualLogSource log;


		private Harmony harmony = new Harmony("red.thishalf.spectateenemiesfix");

		private void Awake() {
			log = Logger;
			harmony.PatchAll();

			Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
		}

	}

	[HarmonyPatch]
	public static class ZoomFix {

		private static Type spectateEnemiesType;
		private static FieldInfo spectateEnemiesZoomLevelField;
		private static FieldInfo spectateEnemiesSpectatingEnemiesField;

		[HarmonyTargetMethod]
		private static MethodBase TargetMethod() {
			spectateEnemiesType = AccessTools.TypeByName("SpectateEnemy.SpectateEnemies");
			spectateEnemiesZoomLevelField = AccessTools.Field(spectateEnemiesType, "zoomLevel");
			spectateEnemiesSpectatingEnemiesField = AccessTools.Field(spectateEnemiesType, "SpectatingEnemies");
			return AccessTools.Method(spectateEnemiesType, "LateUpdate");
		}

		private static void Postfix(object __instance) {
			float zoomLevel = (float) spectateEnemiesZoomLevelField.GetValue(__instance);
			bool spectatingEnemies = (bool) spectateEnemiesSpectatingEnemiesField.GetValue(__instance);
			if (spectatingEnemies) {
				GameNetworkManager.Instance.localPlayerController.spectateCameraPivot.position -= Vector3.up * zoomLevel;
				Camera camera = GameNetworkManager.Instance.localPlayerController.spectateCameraPivot.GetComponentInChildren<Camera>();
				camera.transform.localPosition = Vector3.back * (zoomLevel + 0.5f);
			}
		}
	}
}
