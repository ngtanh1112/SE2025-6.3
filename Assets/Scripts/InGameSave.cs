using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Collections.Generic;

// Copy lại 2 class này để đóng gói dữ liệu
[System.Serializable]
public class GameFileData {
    public string fileName;
    public string contentBase64;
}
[System.Serializable]
public class GameCloudPackage {
    public List<GameFileData> files = new List<GameFileData>();
}

public class InGameSave : MonoBehaviour
{
    public Button btnSave; // Nút bấm
    public Text txtStatus; // Dòng chữ báo trạng thái (nếu có)

    // Sửa lại IP máy bạn cho đúng nhé
    private string baseUrl = "http://localhost:8080"; 

    private string myPlayerId;
    private string persistentPath;

    void Start()
    {
        persistentPath = Application.persistentDataPath;
        
        // Lấy lại ID mà lúc nãy AuthManager đã lưu
        myPlayerId = PlayerPrefs.GetString("CURRENT_PLAYER_ID", "");

        if (btnSave != null)
        {
            btnSave.onClick.AddListener(OnClick_SaveNow);
        }
    }

    public void OnClick_SaveNow()
    {
        if (string.IsNullOrEmpty(myPlayerId))
        {
            Debug.LogError("Mất kết nối ID người chơi!");
            if(txtStatus) txtStatus.text = "Lỗi ID!";
            return;
        }
        StartCoroutine(UploadSaveRoutine());
    }

    IEnumerator UploadSaveRoutine()
    {
        if(txtStatus) txtStatus.text = "Đang lưu...";
        Debug.Log("Bắt đầu lưu game...");

        // 1. Gom file
        GameCloudPackage package = new GameCloudPackage();
        string[] filePaths = Directory.GetFiles(persistentPath);
        foreach (string path in filePaths)
        {
            if (path.Contains("Unity") || path.Contains(".log")) continue;
            byte[] bytes = File.ReadAllBytes(path);
            GameFileData fd = new GameFileData();
            fd.fileName = Path.GetFileName(path);
            fd.contentBase64 = System.Convert.ToBase64String(bytes);
            package.files.Add(fd);
        }

        string jsonPackage = JsonUtility.ToJson(package);

        // 2. Gửi
        WWWForm form = new WWWForm();
        form.AddField("playerId", myPlayerId);
        form.AddField("data", jsonPackage);

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/cloud/upload", form))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Lưu thành công: " + www.downloadHandler.text);
                if(txtStatus) txtStatus.text = "Đã lưu Cloud!";
            }
            else
            {
                if(txtStatus) txtStatus.text = "Lỗi lưu!";
            }
        }
    }
}