using UnityEngine;
using UnityEditor;

public class DeletePlayerPrefs : EditorWindow {
    [MenuItem("Window/Delete PlayerPrefs (All)")]
    static void DeleteAllPlayerPrefs() {
        PlayerPrefs.DeleteAll();
    }
}
