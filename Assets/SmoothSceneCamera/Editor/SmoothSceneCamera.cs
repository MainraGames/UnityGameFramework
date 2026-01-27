#if UNITY_EDITOR

using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace SmoothSceneCamera.Editor
{
    [InitializeOnLoad]
    public class SmoothSceneCamera
    {
        #region Preferences

        private const string UseSmoothZoomKey = "NnUtils_SmoothSceneCamera_SmoothZoom";
        private static bool UseSmoothZoom
        {
            get => EditorPrefs.GetBool(UseSmoothZoomKey, true);
            set => EditorPrefs.SetBool(UseSmoothZoomKey, value);
        }

        private const string ZoomAmountKey = "NnUtils_SmoothSceneCamera_ZoomAmount";
        private static float ZoomAmount
        {
            get => EditorPrefs.GetFloat(ZoomAmountKey, 0.05f);
            set => EditorPrefs.SetFloat(ZoomAmountKey, value);
        }

        private const string ZoomDistancePowerKey = "NnUtils_SmoothSceneCamera_ZoomDistancePower";
        private static float ZoomDistancePower
        {
            get => EditorPrefs.GetFloat(ZoomDistancePowerKey, 1);
            set => EditorPrefs.SetFloat(ZoomDistancePowerKey, value);
        }

        private const string ZoomDurationKey = "NnUtils_SmoothSceneCamera_ZoomDuration";
        private static float ZoomDuration
        {
            get => EditorPrefs.GetFloat(ZoomDurationKey, 0.75f);
            set => EditorPrefs.SetFloat(ZoomDurationKey, value);
        }

        private const string ZoomEasingKey = "NnUtils_SmoothSceneCamera_ZoomEasing";
        private static Easings.Type ZoomEasing
        {
            get => (Easings.Type)EditorPrefs.GetInt(ZoomEasingKey, (int)Easings.Type.ExpoOut);
            set => EditorPrefs.SetInt(ZoomEasingKey, (int)value);
        }

        private const string NearZoomLimitKey = "NnUtils_SmoothSceneCamera_NearZoomLimit";
        private static float NearZoomLimit
        {
            get => EditorPrefs.GetFloat(NearZoomLimitKey, 0.0001f);
            set => EditorPrefs.SetFloat(NearZoomLimitKey, value);
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyPreferencesProvider()
        {
            var provider =
                new SettingsProvider("NnUtils/Smooth Scene Camera", SettingsScope.User)
                {
                    label = "Smooth Scene Camera",
                    guiHandler = _ =>
                    {
                        var originalIndent = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 1;

                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Zoom", EditorStyles.boldLabel);

                        EditorGUI.BeginChangeCheck();

                        var useSmoothZoom = EditorGUILayout.Toggle(
                            new GUIContent("Smooth Zoom",
                                           "Toggles the smooth zoom option"),
                            UseSmoothZoom);
                        var zoomAmount = EditorGUILayout.FloatField(
                            new GUIContent("Zoom Amount",
                                           "Amount of zoom per scroll step"),
                            ZoomAmount);
                        var zoomDistancePower = EditorGUILayout.FloatField(
                            new GUIContent("Zoom Distance Power",
                                           "Exponent that the distance multiplier is raised to"),
                            ZoomDistancePower);
                        var zoomDuration = EditorGUILayout.FloatField(
                            new GUIContent("Zoom Duration",
                                           "Duration of the zoom animation in seconds"),
                            ZoomDuration);
                        var zoomEasing = (Easings.Type)EditorGUILayout.EnumPopup(
                            new GUIContent("Zoom Easing",
                                           "Easing applied to the zoom motion"),
                            ZoomEasing);
                        var nearZoomLimit = EditorGUILayout.FloatField(
                            new GUIContent("Near Zoom Limit",
                                           "Minimum distance to the zoom pivot"),
                            NearZoomLimit);

                        if (!EditorGUI.EndChangeCheck()) return;
                        EditorGUI.indentLevel = originalIndent;

                        UseSmoothZoom = useSmoothZoom;
                        ZoomAmount = zoomAmount;
                        ZoomDistancePower = zoomDistancePower;
                        ZoomDuration = zoomDuration;
                        ZoomEasing = zoomEasing;
                        NearZoomLimit = nearZoomLimit;
                    }
                };

            return provider;
        }

        #endregion

        private static float _lastFrameTime;
        private static float _sizeDelta;
        private static bool _rmbPressed;
        private static bool _mmbPressed;

        static SmoothSceneCamera() => SceneView.duringSceneGui += OnSceneGUI;

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!UseSmoothZoom) return;

            var e = Event.current;
            if (e.type is EventType.MouseDown or EventType.MouseUp)
            {
                var isDown = e.type == EventType.MouseDown;
                if (e.button == 1) _rmbPressed = isDown;
                else if (e.button == 2) _mmbPressed = isDown;
            }

            if (e.type == EventType.ScrollWheel &&
                !_rmbPressed && !_mmbPressed &&
                e.modifiers == EventModifiers.None)
            {
                Zoom(e.delta.y);
                e.Use();
            }
        }

        private static void Zoom(float dir)
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (!sceneView) return;

            // Reset size delta if its sign is opposite of current dir
            if (!Mathf.Approximately(Mathf.Sign(dir), Mathf.Sign(_sizeDelta))) _sizeDelta = 0;

            var startSize = sceneView.size;
            var prevTargetSize = startSize + _sizeDelta;
            var targetSize = prevTargetSize +
                             dir * ZoomAmount *
                             Mathf.Pow(prevTargetSize, ZoomDistancePower);

            // Clamp the target size and calculate size delta
            var startSign = Mathf.Sign(startSize);
            targetSize = Mathf.Clamp(targetSize,
                                     startSign * NearZoomLimit,
                                     startSign * Mathf.Infinity);
            _sizeDelta = targetSize - startSize;

            if (_zoomRoutine != null) EditorCoroutineUtility.StopCoroutine(_zoomRoutine);
            _zoomRoutine = EditorCoroutineUtility.StartCoroutineOwnerless(
                ZoomRoutine(sceneView, startSize, targetSize));
        }

        private static EditorCoroutine _zoomRoutine;

        private static IEnumerator ZoomRoutine(
            SceneView sceneView, float startSize, float targetSize)
        {
            _lastFrameTime = (float)EditorApplication.timeSinceStartup;
            var startSizeDelta = _sizeDelta;

            float lerpPos = 0;
            while (lerpPos < 1)
            {
                var deltaTime = (float)(EditorApplication.timeSinceStartup - _lastFrameTime);
                lerpPos += deltaTime / ZoomDuration;
                lerpPos = Mathf.Clamp01(lerpPos);
                var t = Easings.Ease(lerpPos, ZoomEasing);

                UpdateCameraDistance(sceneView, startSize, targetSize, t);
                _sizeDelta = Mathf.LerpUnclamped(startSizeDelta, 0, t);

                _lastFrameTime = (float)EditorApplication.timeSinceStartup;
                yield return null;
            }

            _zoomRoutine = null;
        }

        private static void UpdateCameraDistance(SceneView sceneView,
                                                 float startPos, float targetPos, float t)
            => sceneView.LookAt(sceneView.pivot, sceneView.rotation,
                                Mathf.LerpUnclamped(startPos, targetPos, t),
                                sceneView.camera.orthographic, true);

    }
}

#endif
