using UnityEditor;
using UnityEngine;

public static class SnapTools
{
    [MenuItem("Tools/Snap To Ground &s")] // alt s
    public static void SnapToGround()
    {
        if (Selection.transforms.Length == 0)
            return;

        foreach (Transform t in Selection.transforms)
        {
            PhysicsScene physicsScene = t.gameObject.scene.GetPhysicsScene();

            if (!physicsScene.Raycast(t.position, Vector3.down, out RaycastHit hit))
                continue;

            float offset = GetBottomOffset(t);

            Undo.RecordObject(t, "Snap To Ground");
            t.position = new Vector3(t.position.x, hit.point.y + offset, t.position.z);
        }
    }

    private static float GetBottomOffset(Transform t)
    {
        Renderer[] renderers = t.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return 0f;

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        return t.position.y - bounds.min.y;
    }
}