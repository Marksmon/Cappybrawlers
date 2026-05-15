#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class DevTools
{
    [MenuItem("Capybrawlers/Reset Save Data")]
    public static void ResetSave()
    {
        var path = Path.Combine(Application.persistentDataPath, "profile.dat");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("[Capybrawlers] Save deleted — fresh profile will be created on next Play.");
        }
        else
        {
            Debug.Log("[Capybrawlers] No save file found at: " + path);
        }
    }
}
#endif
