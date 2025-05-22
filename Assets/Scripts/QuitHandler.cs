using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class QuitHandler : MonoBehaviour
{
    static bool quitRequested = false;

    [RuntimeInitializeOnLoadMethod]
    static void RegisterQuitCallback()
    {
        Application.wantsToQuit += OnWantsToQuit;
    }
    

    IEnumerator Start()
    {
        if (quitRequested) yield break;
        quitRequested = true;

        int playerId = SessionData.playerId;

        WWWForm form = new WWWForm();
        form.AddField("player_id", playerId);

        using (UnityWebRequest www = UnityWebRequest.Post("https://unity-server-sdfo.onrender.com/delete_player.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to delete user: " + www.error);
            }
            else
            {
                Debug.Log("User deleted successfully: " + www.downloadHandler.text);
            }
        }

        Application.Quit();
    }


    static bool OnWantsToQuit()
    {
        if (quitRequested)
            return true;

        GameObject obj = new GameObject("QuitHandler");
        obj.AddComponent<QuitHandler>();
        return false;
    }

}
