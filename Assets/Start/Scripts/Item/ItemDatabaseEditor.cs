#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Reload Items"))
        {
            ((ItemDatabase)target).SendMessage("LoadAllItems");
        }
    }
}
#endif

