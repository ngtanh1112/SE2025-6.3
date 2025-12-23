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

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/cloud/download", form))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                
                // TRƯỜNG HỢP 1: CÓ SAVE TRÊN MÂY (TÀI KHOẢN CŨ)
                if (response.success && !string.IsNullOrEmpty(response.data))
                {
                    Debug.Log("Tìm thấy dữ liệu Cloud -> Đang tải về...");
                    
                    // 1. Xóa sạch file cũ trước cho chắc
                    WipeLocalData();

                    // 2. Bung file mới ra
                    CloudPackage package = JsonUtility.FromJson<CloudPackage>(response.data);
                    foreach (FileData fd in package.files)
                    {
                        string fullPath = Path.Combine(persistentPath, fd.fileName);
                        byte[] bytes = System.Convert.FromBase64String(fd.contentBase64);
                        File.WriteAllBytes(fullPath, bytes);
                    }
                    Debug.Log("Đã đồng bộ save game thành công!");
                }
                // TRƯỜNG HỢP 2: KHÔNG CÓ SAVE TRÊN MÂY (TÀI KHOẢN MỚI) -> RESET GAME
                else
                {
                    Debug.Log("Tài khoản mới tinh -> Reset game về Level 1");
                    
                    // QUAN TRỌNG: Xóa sạch dữ liệu cũ của người chơi trước đi!
                    WipeLocalData();
                }

                // Lưu lại ID người chơi hiện tại (Vì hàm WipeLocalData đã xóa mất nó rồi)
                PlayerPrefs.SetString("CURRENT_PLAYER_ID", myPlayerId);
                PlayerPrefs.Save();
            }
        }
    }

    // --- HÀM PHỤ ĐỂ XÓA SẠCH DỮ LIỆU ---
    void WipeLocalData()
    {
        // 1. Xóa PlayerPrefs (Level, Coin, Settings...)
        PlayerPrefs.DeleteAll();

        // 2. Xóa các file JSON lưu trong máy
        if (Directory.Exists(persistentPath))
        {
            string[] files = Directory.GetFiles(persistentPath);
            foreach (string file in files)
            {
                // Giữ lại file log của Unity, còn lại xóa hết
                if (!file.EndsWith(".log")) 
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }
    }
}