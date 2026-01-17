using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Linq;

namespace Pinwheel.Memo.UI
{
    public class NoteContentPage : NoteUIPage
    {
        protected Vector2 m_scrollPos;
        protected const string CHECKITEM_PLACEHOLDER = "Add to-do";

        private static Dictionary<string, string> s_footerLinks = new Dictionary<string, string>()
        {
            {"With <color=#FF0000>♥</color> from <color=#4646FF>pinwheelstud.io</color>", "https://docs.pinwheelstud.io" },
            {"Polaris - Low Poly Terrain Tool", "https://assetstore.unity.com/packages/tools/terrain/polaris-3-low-poly-terrain-tool-286886" },
            {"Vista - Procedural Terrain Generator", "https://assetstore.unity.com/packages/tools/terrain/procedural-terrain-generator-vista-pro-264414" },
            {"Poseidon - Low Poly Water", "https://assetstore.unity.com/packages/vfx/shaders/low-poly-water-poseidon-153826" },
            {"Jupiter - Procedural Sky Shader" ,"https://assetstore.unity.com/packages/2d/textures-materials/sky/procedural-sky-shader-day-night-cycle-jupiter-159992" },
            {"Texture Graph - Texture Generator", "https://assetstore.unity.com/packages/tools/visual-scripting/procedural-texture-generator-texture-graph-185542" },
            {"Contour - Edge Detection & Outline", "https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/contour-edge-detection-outline-post-effect-urp-render-graph-302915" },
            {"Beam - Volumetric Lighting & Fog","https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/beam-froxel-based-volumetric-lighting-fog-urp-render-graph-317850" }
        };
        private KeyValuePair<string, string> m_footerLink;

        public override void OnPushed()
        {
            base.OnPushed();
            int footerLinkCount = s_footerLinks.Count;
            int footerLinkIndex = UnityEngine.Random.Range(0, footerLinkCount);
            m_footerLink = s_footerLinks.ElementAt(footerLinkIndex);
        }

        public NoteContentPage(IMultipageWindow window, Note note) : base(window, note)
        {

        }

        public override void DrawBody()
        {
            Rect bodyRect = EditorGUILayout.BeginVertical(NoteStyles.noteBody);

            EditorGUI.BeginChangeCheck();
            string noteName = NoteUI.TextAreaWithPlaceholder("Add note name", note.name, NoteStyles.placeholderTextH2, NoteStyles.noteName);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(NoteManager.instance, "Edit note");
                EditorUtility.SetDirty(NoteManager.instance);
                note.name = noteName;
            }

            DrawTagsSection();
            EditorGUILayout.EndVertical();

            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, NoteStyles.noteContentScrollView);
            EditorGUI.BeginChangeCheck();
            string noteDescription = NoteUI.TextAreaWithPlaceholder("Add note description", note.description, NoteStyles.placeholderTextP1, NoteStyles.noteDescription);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(NoteManager.instance, "Edit note");
                EditorUtility.SetDirty(NoteManager.instance);
                note.description = noteDescription;
            }

            DrawLinksSection();
            DrawChecklistSection();

            EditorGUILayout.Space();
            if (NoteUI.ButtonMini("Add Content"))
            {
                ShowContextMenuForAddContent(note, hostWindow);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal(NoteStyles.noteBody);
            NewsChecker.NewsEntry featuredNews = NewsChecker.GetFeaturedNews();
            if (featuredNews != null)
            {
                string url = NetUtils.ModURL(featuredNews.link, "", "memo", "note-footer-news");
                string title = $"<color=#FF0000>֍</color> {featuredNews.title}";
                GUIContent buttonContent = EditorGUIUtility.TrTextContent(title, $"{featuredNews.description.Replace("#mm", "")}\n{url}");
                if (NoteUI.Button3(buttonContent, true, NoteStyles.buttonMini, 300))
                {
                    Application.OpenURL(url);
                }
            }
            else
            {
                if (NoteUI.Button3(m_footerLink.Key, true, NoteStyles.buttonMini))
                {
                    string url = NetUtils.ModURL(m_footerLink.Value, "", "memo", "note-footer");
                    Application.OpenURL(url);
                }
            }

            GUILayout.FlexibleSpace();
            if (NoteUI.Button3("Note explorer", true, NoteStyles.buttonMini))
            {
                NoteExplorerWindow.ShowWindow();
            }

            EditorGUILayout.EndHorizontal();
            if (Event.current.type == EventType.MouseDown)
            {
                GUI.FocusControl(string.Empty);
            }
        }

        float m_tagSectionHeight = EditorGUIUtility.singleLineHeight;
        private void DrawTagsSection()
        {
            List<Tag> addedTags = new List<Tag>();
            foreach (string idTag in note.idTags)
            {
                Tag tag = NoteManager.instance.GetTagById(idTag);
                if (tag != null && !addedTags.Contains(tag))
                {
                    addedTags.Add(tag);
                }
            }

            if (addedTags.Count > 0)
            {
                Rect tagsAreaRect = EditorGUILayout.BeginVertical();
                List<string> tagNames = addedTags.Select(t => t.name).ToList();
                tagNames.Add("Edit Tags");
                List<Rect> tagRects = EditorGUIUtility.GetFlowLayoutedRects(tagsAreaRect, NoteStyles.tagBody, 2, 2, tagNames);
                for (int i = 0; i < tagNames.Count - 1; ++i)
                {
                    Rect rect = tagRects[i];
                    Tag tag = addedTags[i];
                    NoteUI.DrawTag(rect, tag);
                }
                if (NoteUI.ButtonMini(tagRects[^1], tagNames[^1], NoteStyles.tagBody))
                {
                    hostWindow.PushPage(new NoteTagsPage(hostWindow, note));
                }

                if (Event.current.type == EventType.Repaint)
                {
                    Rect lastRect = tagRects[^1];
                    Rect firstRect = tagRects[0];
                    m_tagSectionHeight = lastRect.yMax - firstRect.yMin;
                }
                EditorGUILayout.GetControlRect(GUILayout.Height(m_tagSectionHeight));
                EditorGUILayout.EndVertical();
            }
            else
            {
                if (NoteUI.ButtonMini("Add Tags"))
                {
                    hostWindow.PushPage(new NoteTagsPage(hostWindow, note));
                }
            }
        }

        private static void ShowContextMenuForAddContent(Note note, IMultipageWindow hostWindow)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Checklist"),
                false,
                () =>
                {
                    Undo.RecordObject(NoteManager.instance, "Edit note");
                    EditorUtility.SetDirty(NoteManager.instance);
                    note.checklists.Add(new Checklist());
                });
            menu.AddItem(
                new GUIContent("Link"),
                false,
                () =>
                {
                    Undo.RecordObject(NoteManager.instance, "Edit note");
                    EditorUtility.SetDirty(NoteManager.instance);
                    note.links.Add(new Link());
                    hostWindow.PushPage(new EditLinksPage(hostWindow, note));
                });
            menu.ShowAsContext();
        }

        private void DrawLinksSection()
        {
            IEnumerable<Link> links = note.links.Where(l => !string.IsNullOrEmpty(l.url) && !l.isDeleted);
            if (links.Count() == 0)
                return;

            EditorGUILayout.Space();
            Rect linksRect = EditorGUILayout.BeginVertical(NoteStyles.noteBody);
            GUIUtils.DrawOutline(linksRect, NoteStyles.colorOutlines);

            EditorGUILayout.LabelField($"Link{(links.Count() > 1 ? "s" : "")}", NoteStyles.h3);
            foreach (Link link in links)
            {
                DrawLink(link);
            }
            if (NoteUI.ButtonMini("Edit Links"))
            {
                hostWindow.PushPage(new EditLinksPage(hostWindow, note));
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawLink(Link link)
        {
            string displayText = !string.IsNullOrEmpty(link.displayText) ? link.displayText : link.url;
            if (NoteUI.ButtonLink(displayText, link.url))
            {
                Application.OpenURL(link.url);
            }
        }

        private void DrawChecklistSection()
        {
            foreach (Checklist checklist in note.checklists)
            {
                if (checklist.isDeleted)
                    continue;

                EditorGUILayout.Space();
                Rect checklistRect = EditorGUILayout.BeginVertical(NoteStyles.noteBody);
                GUIUtils.DrawOutline(checklistRect, NoteStyles.colorOutlines);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                string checklistName = NoteUI.TextboxWithPlaceholder("Name your check list", checklist.name, NoteStyles.placeholderTextP1, NoteStyles.checklistName);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(NoteManager.instance, "Edit note");
                    EditorUtility.SetDirty(NoteManager.instance);
                    checklist.name = checklistName;
                }

                if (NoteUI.Button3("…"))
                {
                    ShowContextMenuForChecklist(checklist);
                }
                EditorGUILayout.EndHorizontal();

                int checkItemCount = checklist.items.FindAll(item => !item.isDeleted).Count;
                if (checkItemCount > 0)
                {
                    int checkedCount = checklist.items.FindAll(item => item.isChecked && !item.isDeleted).Count;
                    GUIContent checklistProgressGUIContent = EditorGUIUtility.TrTempContent($"{checkedCount}/{checkItemCount} completed");
                    Vector2 checklistProgressSize = NoteStyles.checklistProgress.CalcSize(checklistProgressGUIContent);
                    Rect checklistProgressRect = EditorGUILayout.GetControlRect(GUILayout.Width(checklistProgressSize.x), GUILayout.Height(checklistProgressSize.y));
                    if (checkedCount == checkItemCount)
                    {
                        EditorGUI.DrawRect(checklistProgressRect, NoteStyles.colorAccomplished);
                    }
                    EditorGUI.LabelField(checklistProgressRect, checklistProgressGUIContent, NoteStyles.checklistProgress);
                }

                for (int i = 0; i < checklist.items.Count; i++)
                {
                    Checklist.Item item = checklist.items[i];
                    if (item.isDeleted)
                        continue;

                    Rect checkItemRect = EditorGUILayout.BeginHorizontal();
                    Rect checkboxRect = EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    DrawCheckboxForItem(checkboxRect, item);

                    EditorGUI.BeginChangeCheck();
                    string itemName = NoteUI.TextAreaWithPlaceholder("Add to-do", item.name, NoteStyles.placeholderTextP1, item.isChecked ? NoteStyles.checkItemNameFaded : NoteStyles.checkItemName);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(NoteManager.instance, "Edit note");
                        EditorUtility.SetDirty(NoteManager.instance);
                        item.name = itemName;
                    }

                    if (NoteUI.Button3("…"))
                    {
                        ShowContextMenuForCheckItem(checklist.items, i);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (NoteUI.ButtonMini("Add Item"))
                {
                    Undo.RecordObject(NoteManager.instance, "Edit note");
                    EditorUtility.SetDirty(NoteManager.instance);
                    Checklist.Item newItem = new Checklist.Item();
                    checklist.items.Add(newItem);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }

        private static void DrawCheckboxForItem(Rect rect, Checklist.Item item)
        {
            EditorGUI.LabelField(rect, item.isChecked ? "■" : "□", item.isChecked ? NoteStyles.checkItemCheckboxFaded : NoteStyles.checkItemCheckbox);

            const string PREF_CHECK_ITEM_ACTIVE_RECT = "check-item-active-rect";
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                SessionState.SetString(PREF_CHECK_ITEM_ACTIVE_RECT, rect.ToString());
            }
            if (Event.current.type == EventType.MouseUp)
            {
                string activeRect = SessionState.GetString(PREF_CHECK_ITEM_ACTIVE_RECT, null);
                if (string.Equals(rect.ToString(), activeRect))
                {
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        Undo.RecordObject(NoteManager.instance, "Edit note");
                        EditorUtility.SetDirty(NoteManager.instance);
                        item.isChecked = !item.isChecked;
                        Event.current.Use();
                    }
                    else
                    {
                        SessionState.SetString(PREF_CHECK_ITEM_ACTIVE_RECT, null);
                        Event.current.Use();
                    }
                }
            }
        }

        private static void ShowContextMenuForChecklist(Checklist checklist)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Delete Checklist"),
                false,
                () =>
                {
                    Undo.RecordObject(NoteManager.instance, "Deleting checklist");
                    EditorUtility.SetDirty(NoteManager.instance);
                    checklist.isDeleted = true;
                });

            menu.ShowAsContext();
        }

        private static void ShowContextMenuForCheckItem(List<Checklist.Item> items, int index)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Delete"),
                false,
                () =>
                {
                    Undo.RecordObject(NoteManager.instance, "Deleting check item");
                    EditorUtility.SetDirty(NoteManager.instance);
                    items[index].isDeleted = true;
                });
            menu.ShowAsContext();
        }
    }
}
