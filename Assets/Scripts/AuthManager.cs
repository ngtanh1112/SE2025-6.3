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

    // LINK SERVER CỦA BẠN
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
        Debug.Log($"[Auth] Bắt đầu gửi request tới: {endpoint}");

        WWWForm form = new WWWForm();
        form.AddField("username", inputUsername.text);
        form.AddField("password", inputPassword.text);

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + endpoint, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = "Lỗi mạng: " + www.error;
                txtStatus.text = errorMsg;
                Debug.LogError($"[Auth Error] {errorMsg}");
            }
            else
            {
                // Parse JSON trả về
                Debug.Log($"[Auth Success] Raw Response: {www.downloadHandler.text}");
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                txtStatus.text = response.message;

                if (response.success)
                {
                    myPlayerId = response.playerId;
                    Debug.Log($"[Auth] Đăng nhập thành công. Player ID: {myPlayerId}");

                    // Tải Save từ trên mây về máy
                    yield return StartCoroutine(DownloadSaveRoutine());
                }
            }
        }
    }

    // --- HÀM TẢI SAVE & XỬ LÝ LOGIC GAME ---
    IEnumerator DownloadSaveRoutine()
    {
        txtStatus.text = "Đang đồng bộ dữ liệu...";
        Debug.Log("[Sync] Bắt đầu đồng bộ dữ liệu save...");
        
        WWWForm form = new WWWForm();
        form.AddField("playerId", myPlayerId);

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/cloud/download", form))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[Sync] Nhận dữ liệu từ server: " + www.downloadHandler.text);
                var response = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                
                // Kiểm tra dữ liệu rỗng (Tài khoản mới)
                bool isDataEmpty = string.IsNullOrEmpty(response.data) || response.data == "null" || response.data == "{}";

                // TRƯỜNG HỢP 1: TÀI KHOẢN CŨ (CÓ FILE SAVE)
                if (response.success && !isDataEmpty)
                {
                    Debug.LogWarning("==> [LOGIC] TÀI KHOẢN CŨ: Tiến hành tải file save.");
                    
                    // Xóa file rác trước
                    WipeLocalData();

                    try {
                        CloudPackage package = JsonUtility.FromJson<CloudPackage>(response.data);
                        foreach (FileData fd in package.files)
                        {
                            string fullPath = Path.Combine(persistentPath, fd.fileName);
                            byte[] bytes = System.Convert.FromBase64String(fd.contentBase64);
                            File.WriteAllBytes(fullPath, bytes);
                            Debug.Log($"-> Đã ghi file: {fd.fileName}");
                        }
                        Debug.Log("[Sync] Bung file save thành công!");
                    }
                    catch (System.Exception e) {
                        Debug.LogError("[Sync Error] Lỗi khi bung file: " + e.Message);
                    }
                }
                // TRƯỜNG HỢP 2: TÀI KHOẢN MỚI (RESET GAME)
                else
                {
                    Debug.LogWarning("==> [LOGIC] TÀI KHOẢN MỚI: Reset toàn bộ game về Level 1.");
                    WipeLocalData();
                    Debug.Log("[Sync] Đã xóa sạch dữ liệu cũ trên đĩa.");
                }

                // Lưu lại ID người chơi
                GlobalData.CurrentPlayerId = myPlayerId;
                Debug.Log("==> Đã lưu ID vào RAM: " + GlobalData.CurrentPlayerId);

                // --- BƯỚC QUAN TRỌNG: DỌN RAM & CHUYỂN CẢNH ---
                
                // 1. Xóa các Singleton cũ trên RAM
                ResetGameSingletons(); 

                // 2. Đợi 1 giây THỰC TẾ (Bất chấp game lag hay pause) - FIX LỖI TREO
                Debug.Log("[System] Đang đợi Unity dọn dẹp bộ nhớ (1s)...");
                yield return new WaitForSecondsRealtime(1.0f);

                // 3. Vào game
                Debug.Log("[System] -> CHUYỂN CẢNH VÀO GAME NGAY BÂY GIỜ!");
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI"); 
            }
            else
            {
                 Debug.LogError("[Sync Error] Lỗi kết nối khi tải save: " + www.error);
            }
        }
    }

    // --- HÀM XÓA DỮ LIỆU TRÊN ĐĨA CỨNG ---
    void WipeLocalData()
    {
        Debug.Log("[Wipe] Đang xóa PlayerPrefs...");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save(); // Lưu ngay lập tức

        if (Directory.Exists(persistentPath))
        {
            string[] files = Directory.GetFiles(persistentPath);
            foreach (string file in files)
            {
                if (!file.EndsWith(".log")) 
                {
                    try { 
                        File.Delete(file); 
                        // Debug.Log("[Wipe] Đã xóa file: " + Path.GetFileName(file));
                    } catch { }
                }
            }
        }
        Debug.Log("[Wipe] Hoàn tất xóa dữ liệu đĩa.");
    }

    // --- HÀM HỦY DIỆT SINGLETON TRÊN RAM (FIX LỖI KHÔNG RESET LEVEL) ---
    void ResetGameSingletons()
    {
        // 1. Ép thời gian chạy (Đề phòng game đang bị Pause)
        Time.timeScale = 1.0f; 
        Debug.Log("==> [Reset RAM] Bắt đầu hủy các Singleton cũ...");

        // Danh sách rút gọn (An toàn, không gây Crash)
        string[] managersToKill = new string[] {
            "EnergyManager",      // Tiền, Tim
            "GGPlayerSettings",   // Level, Setting
            "NavigationManager",  // Màn hình cũ
            "FileIOChanges"       // File System
        };

        foreach (string objName in managersToKill)
        {
            // Tìm theo tên GameObject (An toàn hơn FindObjectOfType)
            GameObject obj = GameObject.Find(objName); 
            
            if (obj != null)
            {
                Debug.Log($"-> [Reset RAM] Đang hủy object: {obj.name}");
                Destroy(obj);
            }
            else
            {
                Debug.Log($"-> [Reset RAM] Không tìm thấy {objName} (Có thể đã tự hủy).");
            }
        }
        
        Debug.Log("==> [Reset RAM] ĐÃ HOÀN TẤT!");
    }
}