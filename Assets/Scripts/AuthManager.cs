using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Collections.Generic;

// --- CÁC CLASS DỮ LIỆU ---
[System.Serializable]
public class FileData
{
    public string fileName;
    public string contentBase64;
}

[System.Serializable]
public class CloudPackage
{
    public List<FileData> files = new List<FileData>();
}

[System.Serializable]
public class ServerResponse
{
    public bool success;
    public string message;
    public string playerId;
    public string data; // Dữ liệu save game
}

// --- CLASS CHÍNH ---
public class AuthManager : MonoBehaviour
{
    [Header("UI")]
    public InputField inputUsername;
    public InputField inputPassword;
    public Text txtStatus;

    // LINK SERVER CỦA BẠN (ĐÃ CẬP NHẬT)
    private string baseUrl = "https://se2025-6-3.onrender.com"; 

    private string myPlayerId = "";
    private string persistentPath;

    void Start()
    {
        persistentPath = Application.persistentDataPath;
    }

    // --- CÁC NÚT BẤM ---
    public void OnClick_Register()
    {
        StartCoroutine(AuthFlow("/auth/register"));
    }

    public void OnClick_Login()
    {
        StartCoroutine(AuthFlow("/auth/login"));
    }

    // --- LUỒNG XỬ LÝ CHÍNH ---
    IEnumerator AuthFlow(string endpoint)
    {
        txtStatus.text = "Đang kết nối server...";
        WWWForm form = new WWWForm();
        form.AddField("username", inputUsername.text);
        form.AddField("password", inputPassword.text);

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + endpoint, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                txtStatus.text = "Lỗi mạng: " + www.error;
            }
            else
            {
                // Parse JSON trả về
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                txtStatus.text = response.message;

                if (response.success)
                {
                    myPlayerId = response.playerId;

                    // 1. Lưu ID người chơi lại để sang màn Game dùng tiếp
                    PlayerPrefs.SetString("CURRENT_PLAYER_ID", myPlayerId);
                    PlayerPrefs.Save();

                    // 2. Tải Save từ trên mây về máy (Hàm bị thiếu nằm ở đây)
                    yield return StartCoroutine(DownloadSaveRoutine());

                    // 3. Vào game
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI"); 
                }
            }
        }
    }

    // --- HÀM TẢI SAVE (LÚC NÃY BẠN BỊ THIẾU CÁI NÀY) ---
    IEnumerator DownloadSaveRoutine()
    {
        txtStatus.text = "Đang đồng bộ dữ liệu...";
        
        WWWForm form = new WWWForm();
        form.AddField("playerId", myPlayerId);

        // Gọi API download
        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/cloud/download", form))
        {
            yield return www.SendWebRequest();
            
            // Xử lý kết quả
            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                
                if (response.success && !string.IsNullOrEmpty(response.data))
                {
                    Debug.Log("Tìm thấy dữ liệu cũ, đang bung file...");
                    
                    // Xóa file rác cũ trên máy
                    string[] oldFiles = Directory.GetFiles(persistentPath);
                    foreach (string file in oldFiles) { 
                        if (!file.Contains("Unity") && !file.Contains(".log")) 
                            try { File.Delete(file); } catch { } 
                    }

                    // Bung file mới ra
                    CloudPackage package = JsonUtility.FromJson<CloudPackage>(response.data);
                    foreach (FileData fd in package.files)
                    {
                        string fullPath = Path.Combine(persistentPath, fd.fileName);
                        byte[] bytes = System.Convert.FromBase64String(fd.contentBase64);
                        File.WriteAllBytes(fullPath, bytes);
                    }
                    Debug.Log("Đã tải xong save game!");
                }
                else
                {
                    Debug.Log("Tài khoản mới hoặc chưa có save -> Chơi từ đầu.");
                    // Xóa sạch dữ liệu cũ của người trước để tránh bị lẫn
                    PlayerPrefs.DeleteAll();
                    // Lưu lại ID lần nữa vì DeleteAll xóa mất nó rồi
                    PlayerPrefs.SetString("CURRENT_PLAYER_ID", myPlayerId); 
                    PlayerPrefs.Save();
                }
            }
        }
    }
}