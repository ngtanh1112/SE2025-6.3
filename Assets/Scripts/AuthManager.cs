using UnityEngine;
using UnityEngine.UI; // Để dùng UI Legacy
using UnityEngine.Networking; // Để gọi Server
using System.Collections;

// Class để đọc dữ liệu JSON từ Server trả về
[System.Serializable]
public class ServerResponse
{
    public bool success;
    public string message;
    public string playerId;
}

public class AuthManager : MonoBehaviour
{
    [Header("Kéo thả UI vào đây")]
    public InputField inputUsername;
    public InputField inputPassword;
    public Text txtStatus;

    [Header("Backend Config")]
    // Lưu ý: Port backend của bạn là 8080
    private string baseUrl = "http://localhost:8080"; 
    
    // Biến lưu ID người chơi sau khi đăng nhập
    private string myPlayerId = "";

    // --- CÁC HÀM SẼ GẮN VÀO NÚT BẤM ---

    public void OnClick_Register()
    {
        StartCoroutine(SendAuthRequest("/auth/register"));
    }

    public void OnClick_Login()
    {
        StartCoroutine(SendAuthRequest("/auth/login"));
    }

    public void OnClick_SaveScore()
    {
        if (myPlayerId == "")
        {
            txtStatus.text = "Bạn chưa đăng nhập!";
            return;
        }
        // Test lưu 500 điểm
        StartCoroutine(SendScoreRequest(500));
    }

    // --- HÀM XỬ LÝ GỬI DỮ LIỆU ---

    IEnumerator SendAuthRequest(string endpoint)
    {
        txtStatus.text = "Đang kết nối...";

        // Đóng gói dữ liệu Username/Pass để gửi đi
        WWWForm form = new WWWForm();
        form.AddField("username", inputUsername.text);
        form.AddField("password", inputPassword.text);

        // Gửi thư đến Server
        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + endpoint, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                txtStatus.text = "Lỗi mạng: " + www.error;
            }
            else
            {
                // Nhận thư trả lời từ Server
                string jsonResult = www.downloadHandler.text;
                Debug.Log("Server trả lời: " + jsonResult);

                // Đọc nội dung JSON
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(jsonResult);

                txtStatus.text = response.message;

                if (response.success)
                {
                    myPlayerId = response.playerId;
                    txtStatus.text = "Xin chào: " + inputUsername.text;
                    
                    // --- SỬA THÀNH TÊN NÀY ---
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI"); 
                }
            }
        }
    }

    IEnumerator SendScoreRequest(int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("playerId", myPlayerId);
        form.AddField("score", score);

        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/simple/saveScore", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                txtStatus.text = "Đã lưu điểm thành công!";
            }
            else
            {
                txtStatus.text = "Lưu điểm thất bại!";
            }
        }
    }
}