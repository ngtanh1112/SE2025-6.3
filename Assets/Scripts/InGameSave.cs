using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Collections.Generic;

// --- CLASS DỮ LIỆU (Đã đổi tên để tránh trùng với AuthManager) ---
[System.Serializable]
public class GameFileData 
{
    public string fileName;
    public string contentBase64;
}

[System.Serializable]
public class GameCloudPackage 
{
    public List<GameFileData> files = new List<GameFileData>();
}

// --- CLASS CHÍNH ---
public class InGameSave : MonoBehaviour
{
    [Header("UI Reference")]
    public Button btnSave;      // Kéo nút Save vào đây
    public Text txtStatus;      // Kéo Text thông báo vào đây (nếu có)

    // LINK SERVER CHÍNH THỨC CỦA BẠN
    private string baseUrl = "https://se2025-6-3.onrender.com"; 

    private string persistentPath;

    void Start()
    {
        persistentPath = Application.persistentDataPath;

        // Gán sự kiện cho nút bấm (nếu có)
        if (btnSave != null)
        {
            btnSave.onClick.RemoveAllListeners(); // Xóa sự kiện cũ cho chắc
            btnSave.onClick.AddListener(OnClick_SaveNow);
        }
    }

    // Hàm được gọi khi bấm nút Save
    public void OnClick_SaveNow()
    {
        // QUAN TRỌNG: Lấy ID ngay lúc bấm nút để đảm bảo ID mới nhất
        string currentId = PlayerPrefs.GetString("CURRENT_PLAYER_ID", "");

        Debug.Log("[Save] Đang kiểm tra ID người chơi: " + currentId);

        if (string.IsNullOrEmpty(currentId))
        {
            Debug.LogError("LỖI: Chưa tìm thấy ID người chơi. Bạn đã đăng nhập chưa?");
            if(txtStatus) txtStatus.text = "Lỗi: Mất kết nối ID!";
            return;
        }

        // Bắt đầu quy trình lưu
        StartCoroutine(UploadSaveRoutine(currentId));
    }

    IEnumerator UploadSaveRoutine(string playerId)
    {
        if(txtStatus) txtStatus.text = "Đang đóng gói dữ liệu...";
        Debug.Log("[Save] Bắt đầu gom file save...");

        // 1. GOM FILE TỪ Ổ CỨNG
        GameCloudPackage package = new GameCloudPackage();
        
        if (Directory.Exists(persistentPath))
        {
            string[] filePaths = Directory.GetFiles(persistentPath);
            foreach (string path in filePaths)
            {
                // Bỏ qua file log rác và folder Unity
                if (path.Contains("Unity") || path.EndsWith(".log")) continue;

                byte[] bytes = File.ReadAllBytes(path);
                
                GameFileData fd = new GameFileData();
                fd.fileName = Path.GetFileName(path);
                fd.contentBase64 = System.Convert.ToBase64String(bytes);
                
                package.files.Add(fd);
            }
        }

        // Chuyển thành JSON để gửi
        string jsonPackage = JsonUtility.ToJson(package);
        Debug.Log($"[Save] Đã đóng gói {package.files.Count} file. Kích thước gói: {jsonPackage.Length} bytes");

        // 2. GỬI LÊN SERVER
        if(txtStatus) txtStatus.text = "Đang gửi lên Cloud...";

        WWWForm form = new WWWForm();
        form.AddField("playerId", playerId);
        form.AddField("data", jsonPackage);

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/cloud/upload", form))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[Save] Thành công! Server trả về: " + www.downloadHandler.text);
                if(txtStatus) txtStatus.text = "Đã lưu game thành công!";
            }
            else
            {
                string errorMsg = "Lỗi lưu: " + www.error;
                Debug.LogError(errorMsg);
                if(txtStatus) txtStatus.text = "Lưu thất bại!";
            }
        }
    }
}