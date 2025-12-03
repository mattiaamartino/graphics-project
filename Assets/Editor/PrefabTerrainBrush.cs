using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

public class PrefabTerrainBrush : TerrainPaintTool<PrefabTerrainBrush>
{
    private const string kToolName = "Prefab Brush";

    [SerializeField] private GameObject prefabToPaint;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private bool alignToNormal = true;

    private Vector3 lastPaintPosition = Vector3.positiveInfinity;

    public override string GetName() => kToolName;
    public override string GetDescription() => "Paint arbitrary prefabs directly onto the terrain surface.";

    public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI context)
    {
        EditorGUILayout.LabelField("Prefab Brush Settings", EditorStyles.boldLabel);
        prefabToPaint = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabToPaint, typeof(GameObject), false);
        spacing = EditorGUILayout.FloatField("Spacing", spacing);
        alignToNormal = EditorGUILayout.Toggle("Align To Normal", alignToNormal);

        EditorGUILayout.HelpBox("Left-click to paint prefabs onto the terrain.", MessageType.Info);
    }

    public override void OnSceneGUI(Terrain terrain, IOnSceneGUI context)
    {
        if (prefabToPaint == null)
            return;

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(hit.point, hit.normal, 0.5f);

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                TryPaintPrefab(hit);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && e.button == 0 && !e.alt)
            {
                if (Vector3.Distance(lastPaintPosition, hit.point) > spacing)
                {
                    TryPaintPrefab(hit);
                    e.Use();
                }
            }
        }
    }

    private void TryPaintPrefab(RaycastHit hit)
    {
        if (prefabToPaint == null)
            return;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPaint);
        Undo.RegisterCreatedObjectUndo(instance, "Paint Prefab");

        instance.transform.position = hit.point;

        if (alignToNormal)
            instance.transform.up = hit.normal;

        lastPaintPosition = hit.point;
    }
}
