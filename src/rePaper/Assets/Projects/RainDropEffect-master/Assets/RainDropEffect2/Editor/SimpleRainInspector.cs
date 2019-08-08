using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimpleRainBehaviour))]
public class SimpleRainInspector : Editor
{
    SimpleRainBehaviour beh;

    void OnEnable()
    {
        this.beh = (SimpleRainBehaviour)target;
    }

    public override void OnInspectorGUI()
    {
        // All the custom inspector will be implemented in the future update!

        /*EditorGUILayout.HelpBox(string.Format("Basic Settings"), MessageType.None);

        EditorGUI.BeginChangeCheck();
        var depth = EditorGUILayout.IntField("Depth", beh.Depth);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change Depth");
            beh.Depth = depth;
        }

        EditorGUI.BeginChangeCheck();
        var NormalMap = (Texture)EditorGUILayout.ObjectField("Normal Map", beh.Variables.NormalMap, typeof(Texture), false);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change Normal Map");
            beh.Variables.NormalMap = NormalMap;
        }

        EditorGUI.BeginChangeCheck();
        var OverlayTexture = (Texture)EditorGUILayout.ObjectField("Overlay Texture", beh.Variables.OverlayTexture, typeof(Texture), false);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change Overlay Texture");
            beh.Variables.OverlayTexture = OverlayTexture;
        }

        // -----------------------------------------------------------------------------------------
        GUILayout.Space(10f);
        EditorGUILayout.HelpBox(string.Format("Play Settings"), MessageType.None);

        EditorGUI.BeginChangeCheck();
        var Duration = EditorGUILayout.Slider("Duration", beh.Variables.Duration, 0f, 30f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change Duration");
            beh.Variables.Duration = Duration;
        }

        EditorGUI.BeginChangeCheck();
        var Delay = EditorGUILayout.Slider("Delay", beh.Variables.Delay, 0f, 30f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change Delay");
            beh.Variables.Delay = Delay;
        }

        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Lifetime");
        GUILayout.Box(string.Format("{0:f2}", beh.Variables.LifetimeMin));
        EditorGUILayout.MinMaxSlider(ref beh.Variables.LifetimeMin, ref beh.Variables.LifetimeMax, 0f, 30f);
        GUILayout.Box(string.Format("{0:f2}", beh.Variables.LifetimeMax));
        GUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change Lifetime");
        }

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Emission Rate");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("Min", GUILayout.Width (30));
        var EmissionRateMin = EditorGUILayout.IntField("", beh.Variables.EmissionRateMin, GUILayout.Width (50));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change EmissionRateMin");
            beh.Variables.EmissionRateMin = EmissionRateMin;
        }
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("Max", GUILayout.Width(30));
        var EmissionRateMax = EditorGUILayout.IntField("", beh.Variables.EmissionRateMax, GUILayout.Width(50));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change EmissionRateMax");
            beh.Variables.EmissionRateMax = EmissionRateMax;
        }
        GUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        var AutoStart = EditorGUILayout.Toggle("Play on Awake", beh.Variables.AutoStart);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change AutoStart");
            beh.Variables.AutoStart = AutoStart;
        }

        EditorGUI.BeginChangeCheck();
        var PlayOnce = EditorGUILayout.Toggle("Play Once", beh.Variables.PlayOnce);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change PlayOnce");
            beh.Variables.AutoStart = PlayOnce;
        }

        // -----------------------------------------------------------------------------------------
        GUILayout.Space(10f);
        EditorGUILayout.HelpBox(string.Format("Rain Settings"), MessageType.None);

        EditorGUI.BeginChangeCheck();
        var MaxRainSpawnCount = EditorGUILayout.IntSlider("Spawn Limit", beh.Variables.MaxRainSpawnCount, 0, 200);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change MaxRainSpawnCount");
            beh.Variables.MaxRainSpawnCount = MaxRainSpawnCount;
        }

        EditorGUI.BeginChangeCheck();
        var AutoRotate = EditorGUILayout.Toggle("Random Rotate", beh.Variables.AutoRotate);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change AutoRotate");
            beh.Variables.AutoStart = AutoRotate;
        }

        EditorGUI.BeginChangeCheck();
        var SpawnOffsetY = EditorGUILayout.Slider("Vertical Offset", beh.Variables.SpawnOffsetY, 0f, 30f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change SpawnOffsetY");
            beh.Variables.SpawnOffsetY = SpawnOffsetY;
        }

        EditorGUI.BeginChangeCheck();
        var OverlayColor = EditorGUILayout.ColorField("Overlay Color", beh.Variables.OverlayColor);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change OverlayColor");
            beh.Variables.OverlayColor = OverlayColor;
        }

        EditorGUI.BeginChangeCheck();
        var Darkness = EditorGUILayout.Slider("Darkness", beh.Variables.Darkness, 0f, 30f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(beh, "Change Darkness");
            beh.Variables.Darkness = Darkness;
        }*/

        

        base.OnInspectorGUI();
    }
}