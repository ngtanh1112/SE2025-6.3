using UnityEngine;
using UnityEditor;
using GGMatch3;
using System.IO;

public class EditorResetUtils
{
    [MenuItem("Tools/Reset Game Progress")]
    public static void ResetGameProgress()
    {
        Debug.Log("Resetting Game Progress...");

        // 1. Clear PlayerPrefs
        PlayerPrefs.DeleteAll();

        // 2. Clear Runtime Data (if in Play Mode)
        bool runtimeResetSuccess = false;
        try
        {
            if (Application.isPlaying)
            {
                if (Match3StagesDB.instance != null) Match3StagesDB.instance.ResetAll();
                if (SingletonInit<RoomsBackend>.instance != null) SingletonInit<RoomsBackend>.instance.Reset();
                if (BehaviourSingleton<EnergyManager>.instance != null) BehaviourSingleton<EnergyManager>.instance.FillEnergy();
                if (GGPlayerSettings.instance != null)
                {
                    GGPlayerSettings.instance.ResetEverything();
                    GGPlayerSettings.instance.Save();
                }
                
                // GGUIDPrivate is static or singleton
                GGUIDPrivate.Reset();
                
                AWSFirehoseAnalytics awsfirehoseAnalytics = Object.FindObjectOfType<AWSFirehoseAnalytics>();
                if (awsfirehoseAnalytics != null)
                {
                    awsfirehoseAnalytics.ResetModel();
                    awsfirehoseAnalytics.sessionID = GGUID.NewGuid();
                }

                Debug.Log("Runtime Singletons Reset.");
                runtimeResetSuccess = true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Runtime reset failed (expected if not playing): {e.Message}");
        }

        // 3. Delete Save Files (Robust method for Edit Mode)
        if (!runtimeResetSuccess || !Application.isPlaying)
        {
            string[] filesToDelete = new string[]
            {
                "player.bytes",
                "st.bytes",
                "r.bytes",
                "ownedItems.bytes",
                "room_replay_data.json"
            };

            foreach (string filename in filesToDelete)
            {
                string path = Path.Combine(Application.persistentDataPath, filename);
                if (File.Exists(path))
                {
                    try 
                    {
                        File.Delete(path);
                        Debug.Log($"Deleted save file: {filename}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to delete {filename}: {e.Message}");
                    }
                }
            }
        }

        Debug.Log("Game Reset Complete. If in Editor, hit Play to start fresh. If in Play Mode, recommended to Stop and Play again.");
    }
    [MenuItem("Tools/Give 100 Stars (Diamonds)")]
    public static void Give100Stars()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("You must be in Play Mode to use this cheat!");
            return;
        }

        if (GGPlayerSettings.instance != null && GGPlayerSettings.instance.walletManager != null)
        {
            GGPlayerSettings.instance.walletManager.AddCurrency(CurrencyType.diamonds, 100);
            GGPlayerSettings.instance.walletManager.AddCurrency(CurrencyType.coins, 10000); // Give coins too just in case
            Debug.Log("Added 100 Diamonds (Stars) and 10000 Coins!");
        }
        else
        {
             Debug.LogError("PlayerSettings or WalletManager not initialized.");
        }
    }
}
