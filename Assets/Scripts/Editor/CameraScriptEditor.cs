using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraScript))]
public class CameraScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CameraScript cameraScript = (CameraScript)target;
        if (GUILayout.Button(nameof(cameraScript.ResetCameraSize)))
        {
            cameraScript.ResetCameraSize();
        }
    }
}
