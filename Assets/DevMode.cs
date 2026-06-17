using UnityEngine;

public class DevMode : MonoBehaviour
{
    public void OnDevModeActivated()
    {
        Debug.Log("[DevMode] Le mode triche a bien été notifié sur ce script.");
    }
}