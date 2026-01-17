using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Pinwheel.Memo.UI
{
    public partial class NoteExplorerWindow : MultipageEditorWindow
    {
        [MenuItem("Window/Memo/Note Explorer")]
        public static void ShowWindow()
        {
            NoteExplorerWindow window = GetWindow<NoteExplorerWindow>();
            window.titleContent = new GUIContent("Note Explorer");
            window.Show();
        }

        public void OnEnable()
        {
            wantsMouseMove = true;
        }

        public void OnDisable()
        {
        }

        public void OnGUI()
        {
            if (m_pageStack.Count == 0)
            {
                PushPage(new MainPage(this));
            }

            Page currentPage = null;
            if (m_pageStack.TryPeek(out currentPage))
            {
                currentPage.DrawHeader();
                currentPage.DrawBody();
            }
            else
            {
                EditorGUILayout.LabelField("Error: no page");
            }
        }
    }

    public partial class NoteExplorerWindow
    {
        public static class Styles
        {
            public static Color32 colorOutline = new Color32(32, 32, 32, 255);
            public static Color32 colorStrip = new Color32(32, 32, 32, 255);

            private static GUIStyle m_body;
            public static GUIStyle body
            {
                get
                {
                    if (m_body == null)
                    {
                        m_body = new GUIStyle();
                        m_body.padding = new RectOffset(12, 12, 8, 8);
                    }
                    return m_body;
                }
            }
        }

        public abstract class NoteExplorerPage : Page
        {
            protected NoteExplorerPage(IMultipageWindow window) : base(window)
            {
            }
        }

        public class MainPage : NoteExplorerPage
        {
            protected NoteExplorerTreeView m_treeView;
            public Vector2 m_scrollPos;

            public MainPage(IMultipageWindow window) : base(window)
            {
            }

            public override void OnPushed()
            {
                base.OnPushed();
                m_treeView = NoteExplorerTreeView.Create();
                m_treeView.Reload();

                NoteManager.noteAdded += OnNoteAddedOrRemoved;
                NoteManager.noteRemoved += OnNoteAddedOrRemoved;
                Undo.undoRedoPerformed += OnUndoRedo;
            }

            public override void OnPopped()
            {
                base.OnPopped();
                NoteManager.noteAdded -= OnNoteAddedOrRemoved;
                NoteManager.noteRemoved -= OnNoteAddedOrRemoved;
                Undo.undoRedoPerformed -= OnUndoRedo;
            }

            protected void OnNoteAddedOrRemoved(NoteManager sender, Note note)
            {
                if (m_treeView != null)
                {
                    m_treeView.Reload();
                }
            }

            public void OnUndoRedo()
            {
                if (m_treeView != null)
                {
                    m_treeView.Reload();
                }
            }

            public override void DrawBody()
            {
                EditorGUILayout.BeginVertical(Styles.body);
                DrawSearchAndFilter();
                DrawTreeView();
                EditorGUILayout.EndVertical();

                EditorGUI.DrawRect(EditorGUILayout.BeginVertical(Styles.body), Styles.colorStrip);
                GUIUtils.DrawLinkStrip("", "memo", "note-explorer-link-strip");
                EditorGUILayout.EndVertical();
            }

            public void DrawSearchAndFilter()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
                string searchText = EditorGUILayout.TextField(m_treeView.searchString, EditorStyles.toolbarSearchField, GUILayout.Width(300));
                if (EditorGUI.EndChangeCheck())
                {
                    m_treeView.searchString = searchText;
                }
                EditorGUILayout.EndHorizontal();
            }

            private void DrawTreeView()
            {
                m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
                Rect treeViewRect = EditorGUILayout.GetControlRect(GUILayout.Height(m_treeView.totalHeight), GUILayout.ExpandHeight(true));
                m_treeView.OnGUI(treeViewRect);
                EditorGUILayout.EndScrollView();
            }
        }
    }
}
