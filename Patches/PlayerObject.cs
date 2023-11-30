using GameNetcodeStuff;
using HarmonyLib;
using System.Diagnostics;
using System.Numerics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlayerDogModel.Patches
{
	[HarmonyPatch]
	internal class PlayerObjects
	{
		public static void InitModels()
		{
			PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

			// Find all the players and add the script to them.
			PlayerControllerB[] players = UnityEngine.Object.FindObjectsOfType<PlayerControllerB>();
			foreach (var player in players)
			{
				if (player == localPlayer)
				{
					
				}
				else
				{
					if (!player.GetComponent<PlayerModelReplacer>())
					{
						player.gameObject.AddComponent<PlayerModelReplacer>();
					}
				}
			}
		}

		[HarmonyPatch(typeof(PlayerControllerB), "SpawnPlayerAnimation")]
		[HarmonyPostfix]
		public static void InitModel(ref PlayerControllerB __instance)
		{
			// Note: not optimized, but oh well.
			InitModels();
		}

		// Hides the base player model.
		[HarmonyPatch(typeof(PlayerControllerB), "DisablePlayerModel")]
		[HarmonyPostfix]
		public static void DisablePlayerModel(ref PlayerControllerB __instance, GameObject playerObject)
		{
			PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
			if (playerObject == localPlayer)
			{
				return;
			}

			playerObject.gameObject.GetComponentInChildren<LODGroup>().enabled = false;
			foreach (SkinnedMeshRenderer mesh in playerObject.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				// Skip the meshes added by the mod.
				if (mesh.name == "Body" || mesh.name == "BodyAsymetrical")
				{
					continue;
				}

				mesh.enabled = false;
			}
		}
	}
}