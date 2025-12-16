using UnityEngine;
using ProtoModels;

public class BackendPing : MonoBehaviour
{
    void Start()
    {
        Debug.Log("BackendPing Start - urlBase=" + GGServerConstants.instance.urlBase);

        GGServerRequestsBackend.instance.ExecuteGetPlayerId((req) =>
        {
            Debug.Log("GetPlayerId status=" + req.status + " err=" + req.errorMessage);

            var pidObj = req.GetResponse<Pid>(); // nếu class Pid có trong project
            if (pidObj != null)
                Debug.Log("PID=" + pidObj.pid);
        });
    }
}
