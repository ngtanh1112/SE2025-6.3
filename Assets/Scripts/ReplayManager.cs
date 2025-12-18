using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GGMatch3;

public class ReplayManager : BehaviourSingletonInit<ReplayManager>
{
    [Serializable]
    public class RoomReplayData
    {
        public string roomName;
        public List<string> unlockedItems = new List<string>();
    }

    [Serializable]
    public class ReplayDataCollection
    {
        public List<RoomReplayData> rooms = new List<RoomReplayData>();
    }

    private ReplayDataCollection dataCollection = new ReplayDataCollection();
    private string filePath;

    public override void Init()
    {
        filePath = Path.Combine(Application.persistentDataPath, "room_replay_data.json");
        LoadData();
    }

    private void LoadData()
    {
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                dataCollection = JsonUtility.FromJson<ReplayDataCollection>(json);
                if (dataCollection == null) dataCollection = new ReplayDataCollection();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load replay data: " + e.Message);
                dataCollection = new ReplayDataCollection();
            }
        }
    }

    private void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(dataCollection, true);
            File.WriteAllText(filePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save replay data: " + e.Message);
        }
    }

    public RoomReplayData GetRoomData(string roomName)
    {
        foreach (var room in dataCollection.rooms)
        {
            if (room.roomName == roomName) return room;
        }
        
        var newRoom = new RoomReplayData { roomName = roomName };
        dataCollection.rooms.Add(newRoom);
        return newRoom;
    }

    public void RecordPurchase(string roomName, string visualObjectName)
    {
        var roomData = GetRoomData(roomName);
        if (!roomData.unlockedItems.Contains(visualObjectName))
        {
            roomData.unlockedItems.Add(visualObjectName);
            SaveData();
        }
    }

    public void StartReplay(DecorateRoomScreen screen, string roomName, Action onComplete = null)
    {
        StartCoroutine(ReplayRoutine(screen, roomName, onComplete));
    }

    private IEnumerator ReplayRoutine(DecorateRoomScreen screen, string roomName, Action onComplete)
    {
        var roomData = GetRoomData(roomName);
        if (roomData == null || roomData.unlockedItems.Count == 0)
        {
            Debug.LogWarning("No replay data for room: " + roomName);
            if (onComplete != null) onComplete();
            yield break;
        }

        DecoratingScene scene = screen.scene;
        if (scene == null)
        {
             if (onComplete != null) onComplete();
             yield break;
        }

        // Hide UI
        GGUtil.SetActive(screen.widgetsToHide, false);
        GGUtil.SetActive(screen.controlWidgets, false);
        
        // Reset Scene (Hide all purchased items)
        List<VisualObjectBehaviour> allBehaviours = scene.visualObjectBehaviours;
        // Create a HashSet for faster lookup, casing might matter so let's handle it
        HashSet<string> replayItemNames = new HashSet<string>();
        foreach(var item in roomData.unlockedItems)
        {
            replayItemNames.Add(item.ToLower());
        }

        foreach (var behaviour in allBehaviours)
        {
            if (behaviour.isPlayerControlledObject)
            {
                // Ensure markers (dashed lines) are OFF for replay
                behaviour.SetMarkersActive(false);

                // Only hide if it is in our replay list. 
                // If it's not in the list, it's either a default item or something we didn't buy, so we shouldn't touch it.
                // Assuming defaults are correctly set up in the scene before this.
                if (replayItemNames.Contains(behaviour.visualObject.name.ToLower()))
                {
                    if (behaviour.hasDefaultVariation)
                    {
                        behaviour.ShowDefaultVariation();
                    }
                    else
                    {
                        behaviour.Hide();
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        // Replay
        foreach (string itemName in roomData.unlockedItems)
        {
            VisualObjectBehaviour behaviour = scene.GetBehaviour(itemName.ToLower());
            if (behaviour != null)
            {
                // Show particle effect
                if (screen.visualObjectParticles != null)
                {
                    screen.visualObjectParticles.CreateParticles(VisualObjectParticles.PositionType.BuySuccess, scene.rootTransform.gameObject, behaviour);
                    GGSoundSystem.Play(GGSoundSystem.SFXType.ButtonConfirm);
                }

                behaviour.SetVisualState(); // Use existing logic to show the correct variation
                yield return new WaitForSeconds(0.4f); // Delay between items
            }
        }

        yield return new WaitForSeconds(1.0f);

        // Logic split: If onComplete has been provided, we assume the caller handles the next steps (like showing Win Screen).
        // If not, we restore the screen state ourselves.
        if (onComplete != null)
        {
            screen.Init(); // Still need to restore scene state just in case, but let callback handle UI
            onComplete();
        }
        else
        {
             screen.Init();
        }
    }
}
