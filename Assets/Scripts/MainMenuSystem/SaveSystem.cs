using System;
using System.IO;
using UnityEngine;


[Serializable]
public class SaveData
{
    public bool   isEmpty    = true;
    public string lastPlayed = "";       
}

public static class SaveSystem
{
    public const int SLOT_COUNT = 3;

    private static string SlotPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    public static void Save(int slot, SaveData data)
    {
        if (slot < 0 || slot >= SLOT_COUNT)
        {
            Debug.LogWarning($"[SaveSystem] Invalid slot {slot}");
            return;
        }

        data.isEmpty    = false;
        data.lastPlayed = DateTime.Now.ToString("dd MMM yyyy – HH:mm");

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SlotPath(slot), json);
        Debug.Log($"[SaveSystem] Saved slot {slot} → {SlotPath(slot)}");
    }
    public static SaveData Load(int slot)
    {
        string path = SlotPath(slot);

        if (!File.Exists(path))
            return new SaveData();                

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to read slot {slot}: {e.Message}");
            return new SaveData();
        }
    }


    public static SaveData[] LoadAll()
    {
        SaveData[] saves = new SaveData[SLOT_COUNT];
        for (int i = 0; i < SLOT_COUNT; i++)
            saves[i] = Load(i);
        return saves;
    }

    public static void Delete(int slot)
    {
        string path = SlotPath(slot);
        if (File.Exists(path))
            File.Delete(path);
    }

    public static int  ActiveSlot
    {
        get => PlayerPrefs.GetInt("ActiveSaveSlot", 0);
        set { PlayerPrefs.SetInt("ActiveSaveSlot", value); PlayerPrefs.Save(); }
    }
}
