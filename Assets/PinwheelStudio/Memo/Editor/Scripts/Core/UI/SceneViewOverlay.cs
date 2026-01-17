using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using UnityEditor.Toolbars;

namespace Pinwheel.Memo.UI
{
    [Overlay(typeof(SceneView), "Memo")]
    public class SceneViewOverlay : ToolbarOverlay
    {
        public delegate void PopulateItemHandler(List<string> elementIds);
        public static event PopulateItemHandler populateItemCallback;

        public SceneViewOverlay() : base(PopulateItems())
        {
        }

        private static string[] PopulateItems()
        {
            List<string> elementIds = new List<string>();
            elementIds.Add(NoteGizmosToggle.ID);
            populateItemCallback?.Invoke(elementIds);
            return elementIds.ToArray();
        }
    }

    [EditorToolbarElement(NoteGizmosToggle.ID, typeof(SceneView))]
    public class NoteGizmosToggle : EditorToolbarToggle
    {
        public const string ID = "memo-note-gizmos-toggle";

        public NoteGizmosToggle()
        {
            text = "";
            onIcon = Icons.NOTE_GIZMO;
            offIcon = Icons.NOTE_GIZMO;
            tooltip = "Draw note gizmos";
            value = NoteManager.instance.drawNoteGizmos;
        }

        protected override void ToggleValue()
        {
            base.ToggleValue();
            NoteManager.instance.drawNoteGizmos = this.value;
        }
    }
}