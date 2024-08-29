using GameNetcodeStuff;
using MoreCompany;
using MoreCompany.Cosmetics;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerDogModel.Patches.MoreCompanyPatch
{

    static class EnableDogModelPatch
    {
        public static void HideMoreCompanyCosmeticsForPlayer(PlayerControllerB playerController)
        {
            CosmeticApplication cosmeticApplication = playerController.meshContainer.GetComponentInChildren<CosmeticApplication>();

            Debug.Log($"{PluginInfo.PLUGIN_GUID}: This cosmetic application's instance ID was {cosmeticApplication.GetInstanceID()}");

            if (cosmeticApplication == null)
            {
                Debug.Log($"{PluginInfo.PLUGIN_GUID}: cosmeticApplication was null!");
                return;
            }

            cosmeticApplication.ClearCosmetics();

            List<string> selectedCosmetics = MainClass.playerIdsAndCosmetics[(int)playerController.playerClientId];
            foreach (var selected in selectedCosmetics)
            {
                Debug.Log($"{PluginInfo.PLUGIN_GUID}: Disabling {playerController.playerUsername}'s cosmetics...");
                cosmeticApplication.ApplyCosmetic(selected, false);
            }

            cosmeticApplication.RefreshAllCosmeticPositions();
            foreach (var cosmetic in cosmeticApplication.spawnedCosmetics)
            {
                cosmetic.transform.localScale *= CosmeticRegistry.COSMETIC_PLAYER_SCALE_MULT;
            }
        }
    }

    static class EnableHumanModelPatch
    {
        public static void ShowMoreCompanyCosmeticsForPlayer(PlayerControllerB playerController)
        {
            CosmeticApplication cosmeticApplication = playerController.meshContainer.GetComponentInChildren<CosmeticApplication>();

            Debug.Log($"{PluginInfo.PLUGIN_GUID}: This cosmetic application's instance ID was {cosmeticApplication.GetInstanceID()}");

            if (cosmeticApplication == null)
            {
                Debug.Log($"{PluginInfo.PLUGIN_GUID}: cosmeticApplication was null!");
                return;
            }

            cosmeticApplication.ClearCosmetics();
            List<string> selectedCosmetics = MainClass.playerIdsAndCosmetics[(int)playerController.playerClientId];
            foreach (var selected in selectedCosmetics)
            {
                Debug.Log($"{PluginInfo.PLUGIN_GUID}: Enabling {playerController.playerUsername}'s cosmetics...");
                cosmeticApplication.ApplyCosmetic(selected, true);
            }

            cosmeticApplication.RefreshAllCosmeticPositions();
            foreach (var cosmetic in cosmeticApplication.spawnedCosmetics)
            {
                cosmetic.transform.localScale *= CosmeticRegistry.COSMETIC_PLAYER_SCALE_MULT;
            }
        }
    }
}
