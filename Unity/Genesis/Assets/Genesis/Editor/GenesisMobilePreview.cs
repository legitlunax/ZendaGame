using System;
using System.Reflection;
using Genesis;
using UnityEditor;
using UnityEngine;

public static class GenesisMobilePreview
{
    [MenuItem("Genesis/Preview/Mobile portrait (390 x 700)")]
    public static void Portrait()
    {
        // Unity has no public Game view sizing API; keep the editor-only reflection isolated.
        Type type = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        MethodInfo setResolution = type?.GetMethod("SetCustomResolution",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
            null, new[] { typeof(Vector2), typeof(string) }, null);
        if (setResolution == null)
            throw new NotSupportedException("Select a 390 x 700 fixed resolution in the Game view size menu.");
        EditorWindow window = EditorWindow.GetWindow(type);
        setResolution.Invoke(setResolution.IsStatic ? null : window, new object[]
        {
            new Vector2(GenesisSanctum.DesktopWindowWidth, GenesisSanctum.DesktopWindowHeight),
            "Genesis mobile portrait"
        });
        window.Focus();
        window.Repaint();
        EditorApplication.QueuePlayerLoopUpdate();
        Debug.Log("Genesis Game view set to 390 x 700 portrait. No scenes saved.");
    }
}