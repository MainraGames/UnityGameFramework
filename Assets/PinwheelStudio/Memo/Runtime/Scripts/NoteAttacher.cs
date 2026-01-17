using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Memo
{
    //[AddComponentMenu("")]
    [ExecuteInEditMode]
    public class NoteAttacher : MonoBehaviour
    {
        [SerializeField]
        protected string m_id;
        public string id
        {
            get
            {
                return m_id;
            }
        }

#if UNITY_EDITOR
        public delegate void DrawNoteInSceneHandler(NoteAttacher sender, SceneView sceneView);
        public static event DrawNoteInSceneHandler drawSceneNoteCallback;

        public delegate void DrawNoteGizmosHandler(NoteAttacher sender, GizmoType gizmoType);
        public static event DrawNoteGizmosHandler drawNoteGizmosCallback;

        public delegate void MessageHandler(NoteAttacher sender);
        public static event MessageHandler updateCallback;

        private static Dictionary<string, NoteAttacher> s_instanceById = new Dictionary<string, NoteAttacher>();

        private void Reset()
        {
            m_id = $"memo-{System.Guid.NewGuid().ToString()}";
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += DuringSceneGUI;
            if (!string.IsNullOrEmpty(m_id))
            {
                s_instanceById[id] = this;
            }
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            if (!string.IsNullOrEmpty(id) && s_instanceById.ContainsKey(id))
            {
                s_instanceById.Remove(id);
            }
        }

        public static NoteAttacher GetById(string id)
        {
            NoteAttacher noteAttacher = null;
            if (s_instanceById.TryGetValue(id, out noteAttacher))
            {
                return noteAttacher;
            }
            else
            {
                return null;
            }
        }

        private void DuringSceneGUI(SceneView sv)
        {
            drawSceneNoteCallback?.Invoke(this, sv);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
        private static void DrawSceneNoteGizmos(NoteAttacher sender, GizmoType gizmoType)
        {
            drawNoteGizmosCallback?.Invoke(sender, gizmoType);
        }

        private void Update()
        {
            //Only raise this event outside play mode to prevent adding overhead to the runtime
            if (!Application.isPlaying)
            {
                updateCallback?.Invoke(this);
            }
        }
#endif  
    }
}
