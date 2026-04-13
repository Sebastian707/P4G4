using UnityEngine;

public class PlayerPrefLoader : MonoBehaviour
{
    void Start()
    {
        float saved = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        GetComponent<StarterAssets.FirstPersonController>().MouseSensitivity = saved;
        Debug.Log("Mouse Sensitivity loaded" + saved);
    }
}