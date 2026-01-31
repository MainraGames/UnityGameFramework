using UnityEditor;
using UnityEngine;
using _Game.Scripts.Utility;

namespace _Game.Scripts.Utility.Editor
{
    /// <summary>
    /// Custom editor for the Tweener component.
    /// </summary>
    [CustomEditor(typeof(Tweener))]
    public class TweenerEditor : UnityEditor.Editor
    {
        #region Serialized Properties
        
        private SerializedProperty _useUnscaledTimeProp;
        private SerializedProperty _startFromInitialActiveStateProp;
        private SerializedProperty _simultaneousTweensProp;
        private SerializedProperty _sequentialTweensProp;
        
        #endregion

        #region Private Fields
        
        private bool[] _foldoutsSimultaneous;
        private bool[] _foldoutsSequential;
        
        #endregion

        #region Unity Editor Callbacks
        
        private void OnEnable()
        {
            _useUnscaledTimeProp = serializedObject.FindProperty("_useUnscaledTime");
            _startFromInitialActiveStateProp = serializedObject.FindProperty("_startFromInitialActiveState");
            _simultaneousTweensProp = serializedObject.FindProperty("_simultaneousTweens");
            _sequentialTweensProp = serializedObject.FindProperty("_sequentialTweens");
            UpdateFoldoutsArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawColoredBackground(Color.black, Color.white, () =>
            {
                EditorGUILayout.PropertyField(_useUnscaledTimeProp, new GUIContent("Use Unscaled Time", "Use unscaled time for tweens"));
                EditorGUILayout.PropertyField(_startFromInitialActiveStateProp, new GUIContent("Start From Initial Active State", "Start tween from the initial active state of the object"));
            });

            EditorGUILayout.Space();
            DrawColoredBackground(Color.black, Color.white, () => DrawTweensList("Simultaneous Tweens", _simultaneousTweensProp, ref _foldoutsSimultaneous));

            EditorGUILayout.Space();
            DrawColoredBackground(Color.black, Color.white, () => DrawTweensList("Sequential Tweens", _sequentialTweensProp, ref _foldoutsSequential));

            serializedObject.ApplyModifiedProperties();
        }
        
        #endregion

        #region Private Methods
        
        private void UpdateFoldoutsArray()
        {
            _foldoutsSimultaneous = new bool[_simultaneousTweensProp.arraySize];
            _foldoutsSequential = new bool[_sequentialTweensProp.arraySize];
        }

        private void DrawColoredBackground(Color backgroundColor, Color textColor, System.Action drawContent)
        {
            var rect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(rect, backgroundColor);
            GUI.color = textColor;
            drawContent();
            GUI.color = Color.white;
            EditorGUILayout.EndVertical();
        }

        private void DrawTweensList(string label, SerializedProperty tweensProp, ref bool[] foldouts)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            if (foldouts.Length != tweensProp.arraySize)
            {
                UpdateFoldoutsArray();
            }

            for (int i = 0; i < tweensProp.arraySize; i++)
            {
                SerializedProperty tween = tweensProp.GetArrayElementAtIndex(i);
                SerializedProperty tweenName = tween.FindPropertyRelative("name");
                SerializedProperty tweenType = tween.FindPropertyRelative("tweenType");

                foldouts[i] = EditorGUILayout.Foldout(foldouts[i], $"{label} {i + 1}: {tweenName.stringValue}", true);

                if (foldouts[i])
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.PropertyField(tween.FindPropertyRelative("target"));
                    EditorGUILayout.PropertyField(tweenName, new GUIContent("Name"));
                    EditorGUILayout.PropertyField(tweenType);

                    switch ((Tweener.TweenSettings.TweenType)tweenType.enumValueIndex)
                    {
                        case Tweener.TweenSettings.TweenType.Move:
                        case Tweener.TweenSettings.TweenType.Scale:
                        case Tweener.TweenSettings.TweenType.Rotate:
                            EditorGUILayout.PropertyField(tween.FindPropertyRelative("targetValue"));
                            break;
                        case Tweener.TweenSettings.TweenType.Fade:
                            EditorGUILayout.PropertyField(tween.FindPropertyRelative("targetAlpha"));
                            break;
                        case Tweener.TweenSettings.TweenType.Color:
                            EditorGUILayout.PropertyField(tween.FindPropertyRelative("targetColor"));
                            break;
                    }

                    EditorGUILayout.PropertyField(tween.FindPropertyRelative("duration"));
                    EditorGUILayout.PropertyField(tween.FindPropertyRelative("delay"));

                    SerializedProperty useCustomCurve = tween.FindPropertyRelative("useCustomCurve");
                    EditorGUILayout.PropertyField(useCustomCurve);

                    if (useCustomCurve.boolValue)
                    {
                        EditorGUILayout.PropertyField(tween.FindPropertyRelative("customCurve"));
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(tween.FindPropertyRelative("easeType"));
                    }

                    SerializedProperty loop = tween.FindPropertyRelative("loop");
                    EditorGUILayout.PropertyField(loop);

                    if (loop.boolValue)
                    {
                        EditorGUILayout.PropertyField(tween.FindPropertyRelative("loopCount"));
                        EditorGUILayout.PropertyField(tween.FindPropertyRelative("pingpong"));
                    }

                    EditorGUILayout.PropertyField(tween.FindPropertyRelative("OnTweenComplete"));
                    
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("Move Up") && i > 0)
                {
                    tweensProp.MoveArrayElement(i, i - 1);
                }
                GUI.backgroundColor = Color.magenta;
                if (GUILayout.Button("Move Down") && i < tweensProp.arraySize - 1)
                {
                    tweensProp.MoveArrayElement(i, i + 1);
                }
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Remove"))
                {
                    tweensProp.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    UpdateFoldoutsArray();
                    return;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button($"Add {label} Animation"))
            {
                tweensProp.InsertArrayElementAtIndex(tweensProp.arraySize);
                serializedObject.ApplyModifiedProperties();
                UpdateFoldoutsArray();
            }
            GUI.backgroundColor = Color.white;
        }
        
        #endregion
    }
}