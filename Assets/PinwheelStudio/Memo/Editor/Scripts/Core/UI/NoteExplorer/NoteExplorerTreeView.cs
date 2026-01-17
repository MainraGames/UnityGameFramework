using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using Pinwheel.Memo;
using UnityEditor;
using System.Linq;
using Pinwheel.Memo.Trello;
using System;
using Object = UnityEngine.Object;

namespace Pinwheel.Memo.UI
{
    public class NoteExplorerTreeView : TreeView<int>
    {
        public static class Styles
        {
            public static readonly Color32 colorRowOdd = new Color32(55, 55, 55, 255);
            public static readonly Color32 colorRowEven = new Color32(65, 65, 65, 255);
            public static readonly Color32 colorRowSelected = new Color32(62, 95, 150, 255);

            private static GUIStyle m_noteName;
            public static GUIStyle noteName
            {
                get
                {
                    if (m_noteName == null)
                    {
                        m_noteName = new GUIStyle(Pinwheel.Memo.Styles.p1);
                        m_noteName.normal.textColor = NoteStyles.colorTextNormal;
                        m_noteName.hover.textColor = NoteStyles.colorTextNormal;
                        m_noteName.focused.textColor = NoteStyles.colorTextNormal;
                        m_noteName.active.textColor = NoteStyles.colorTextNormal;
                        m_noteName.wordWrap = false;
                    }
                    return m_noteName;
                }
            }

            public static Rect ShrinkRect(Rect r, int pixel)
            {
                return new RectOffset(pixel, pixel, pixel, pixel).Remove(r);
            }
        }

        public class Item : TreeViewItem<int>
        {
            public Note note { get; set; }

            public Item(int id, int depth, string displayName = null) : base(id, depth, displayName)
            {

            }
        }

        protected NoteExplorerTreeView(TreeViewState<int> state) : base(state)
        {
            Init();
        }

        protected NoteExplorerTreeView(TreeViewState<int> state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            Init();
        }

        public static NoteExplorerTreeView Create()
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column(){headerContent = new GUIContent("Name"), width = 200},
                new MultiColumnHeaderState.Column(){headerContent = new GUIContent("Tags"), width = 100, canSort = false},
                new MultiColumnHeaderState.Column(){headerContent = new GUIContent("To-do"), width = 50},
                new MultiColumnHeaderState.Column(){headerContent = new GUIContent("Description"), width = 100},
                new MultiColumnHeaderState.Column(){headerContent = new GUIContent("Attach To"), width = 100},
                new MultiColumnHeaderState.Column(){headerContent = new GUIContent("Link To"), width = 100},
                new MultiColumnHeaderState.Column(){headerContent = new GUIContent("Actions"), width = 100, allowToggleVisibility = false, canSort = false}
            };
            MultiColumnHeaderState headerState = new MultiColumnHeaderState(columns);
            MultiColumnHeader header = new MultiColumnHeader(headerState);
            header.ResizeToFit();

            TreeViewState<int> state = new TreeViewState<int>();
            NoteExplorerTreeView treeView = new NoteExplorerTreeView(state, header);
            treeView.useScrollView = false;
            treeView.showAlternatingRowBackgrounds = true;
            treeView.showBorder = true;

            header.sortingChanged += (header) => { treeView.Reload(); };

            return treeView;
        }

        protected override TreeViewItem<int> BuildRoot()
        {
            int itemId = 0;
            Item root = new Item(itemId++, -1);
            IEnumerable<Note> notes = NoteManager.instance.GetNotes();
            foreach (Note n in notes)
            {
                Item item = new Item(itemId++, 0);
                item.note = n;
                root.AddChild(item);
            }

            return root;
        }

        protected override IList<TreeViewItem<int>> BuildRows(TreeViewItem<int> root)
        {
            List<TreeViewItem<int>> items = new List<TreeViewItem<int>>();
            if (root.children!=null)
                items.AddRange(root.children);

            if (!string.IsNullOrEmpty(searchString))
                items = items.Where(item => DoesItemMatchSearch(item, searchString)).ToList();

            int sortedColumnIndex = multiColumnHeader.sortedColumnIndex;
            if (sortedColumnIndex >= 0 &&
                m_sortDelegates[sortedColumnIndex] != null)
            {
                MultiColumnHeaderState.Column column = multiColumnHeader.state.columns[sortedColumnIndex];
                bool sortAscending = column.sortedAscending;
                items.Sort(m_sortDelegates[sortedColumnIndex]);

                if (!sortAscending)
                {
                    items.Reverse();
                }
            }

            return items;
        }

        protected delegate void CellGUIHandler(Rect cellRect, Note note);
        protected CellGUIHandler[] m_cellGuiDelegates;
        protected System.Comparison<TreeViewItem<int>>[] m_sortDelegates;

        protected void Init()
        {
            m_cellGuiDelegates = new CellGUIHandler[]
            {
                CellGUI_Name,
                CellGUI_Tags,
                CellGUI_Todo,
                CellGUI_Description,
                CellGUI_AttachTarget,
                CellGUI_Linkage,
                CellGUI_Actions
            };

            m_sortDelegates = new System.Comparison<TreeViewItem<int>>[]
            {
                SortName,
                null,
                SortTodo,
                SortDescription,
                SortAttachTarget,
                SortLinkage,
                null
            };
        }

        protected int SortName(TreeViewItem<int> x, TreeViewItem<int> y)
        {
            return string.Compare((x as Item).note.name, (y as Item).note.name);
        }

        protected int SortTodo(TreeViewItem<int> x, TreeViewItem<int> y)
        {
            int completed, total;
            Note noteX = (x as Item).note;
            noteX.GetChecklistsProgress(out completed, out total);
            int remainingX = total - completed;

            Note noteY = (y as Item).note;
            noteY.GetChecklistsProgress(out completed, out total);
            int remainingY = total - completed;

            return remainingX.CompareTo(remainingY);
        }

        protected int SortDescription(TreeViewItem<int> x, TreeViewItem<int> y)
        {
            return string.Compare((x as Item).note.description, (y as Item).note.description);
        }

        protected int SortAttachTarget(TreeViewItem<int> x, TreeViewItem<int> y)
        {
            return string.Compare((x as Item).note.attachTarget.targetType.ToString(), (y as Item).note.attachTarget.targetType.ToString());
        }

        protected int SortLinkage(TreeViewItem<int> x, TreeViewItem<int> y)
        {
            int xVal = NoteUtils.IsNoteConnectedToTrelloCard((x as Item).note) ? 1 : 0;
            int yVal = NoteUtils.IsNoteConnectedToTrelloCard((y as Item).note) ? 1 : 0;
            return xVal.CompareTo(yVal);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            Item item = (Item)args.item;
            Note note = item.note;

            int columnsCount = args.GetNumVisibleColumns();
            for (int iC = 0; iC < columnsCount; ++iC)
            {
                int realColumnIndex = args.GetColumn(iC);
                Rect cellRect = args.GetCellRect(iC);
                if (realColumnIndex >= 0 && realColumnIndex < m_cellGuiDelegates.Length)
                {
                    CellGUIHandler guiDelegate = m_cellGuiDelegates[realColumnIndex];
                    guiDelegate.Invoke(cellRect, note);
                }
                else
                {
                    EditorGUI.LabelField(cellRect, "No GUI delegate");
                }
            }
        }

        protected void CellGUI_ID(Rect r, Note n)
        {
            GUIContent content = EditorGUIUtility.TrTextContent(n.id, n.id);
            EditorGUI.LabelField(r, content);
        }

        protected void CellGUI_Name(Rect r, Note n)
        {
            EditorGUI.DrawRect(Styles.ShrinkRect(r, 1), NoteStyles.GetNoteBackgroundColor(n.color));
            GUIContent content = EditorGUIUtility.TrTextContent(n.name, n.name);
            EditorGUI.LabelField(r, content, Styles.noteName);
        }

        protected void CellGUI_Tags(Rect r, Note n)
        {
            List<Tag> tags = new List<Tag>();
            foreach (string idTag in n.idTags)
            {
                Tag t = NoteManager.instance.GetTagById(idTag);
                if (t != null)
                {
                    tags.Add(t);
                }
            }

            List<string> tagNames = tags.Select(t => "  ").ToList();
            List<Rect> tagRects = EditorGUIUtility.GetFlowLayoutedRects(r, TreeView<int>.DefaultStyles.label, 1, 1, tagNames);

            for (int i = 0; i < tags.Count; ++i)
            {
                Rect tagRect = tagRects[i];
                Color32 tagColor = NoteStyles.GetTagBackgroundColor(tags[i].color);
                GUIContent tagLabel = EditorGUIUtility.TrTextContent("  ", tags[i].name);
                EditorGUI.DrawRect(new RectOffset(0, 2, 3, 3).Remove(tagRect), tagColor);
                EditorGUI.LabelField(tagRect, tagLabel, DefaultStyles.label);
            }
        }

        protected void CellGUI_Todo(Rect r, Note n)
        {
            int completed = 0;
            int total = 0;
            n.GetChecklistsProgress(out completed, out total);
            string label = total > 0 ? $"{completed}/{total}" : "";
            EditorGUI.LabelField(r, label);
        }

        protected void CellGUI_Description(Rect r, Note n)
        {
            GUIContent content = EditorGUIUtility.TrTextContent(n.description, n.description);
            EditorGUI.LabelField(r, content);
        }

        protected void CellGUI_Color(Rect r, Note n)
        {
            EditorGUI.DrawRect(r, NoteStyles.GetNoteBackgroundColor(n.color));
        }

        protected void CellGUI_AttachTarget(Rect r, Note n)
        {
            AttachTarget att = n.attachTarget;
            if (att.targetType == AttachTarget.TargetType.SceneObject)
            {
                NoteAttacher attacher = NoteAttacher.GetById(att.id);
                if (attacher != null)
                {
                    GUIContent buttonLabel = EditorGUIUtility.TrTempContent($"{attacher.transform.parent?.name} (Game Object)");
                    Vector2 buttonSize = EditorStyles.label.CalcSize(buttonLabel);
                    buttonSize = Vector2.Min(buttonSize, r.size);
                    Rect buttonRect = new Rect(r.position, buttonSize);
                    if (GUIUtils.Button3(buttonRect, buttonLabel.text))
                    {
                        EditorGUIUtility.PingObject(attacher.gameObject);
                    }
                }
            }
            else if (att.targetType == AttachTarget.TargetType.ProjectAsset)
            {
                string assetGuid = att.id;
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    GUIContent buttonLabel = EditorGUIUtility.TrTempContent($"{asset.name} ({asset.GetType().Name})");
                    Vector2 buttonSize = EditorStyles.label.CalcSize(buttonLabel);
                    buttonSize = Vector2.Min(buttonSize, r.size);
                    Rect buttonRect = new Rect(r.position, buttonSize);
                    if (GUIUtils.Button3(buttonRect, buttonLabel.text))
                    {
                        EditorGUIUtility.PingObject(asset);
                    }
                }
            }
        }

        protected void CellGUI_Linkage(Rect r, Note n)
        {
            if (NoteUtils.IsNoteConnectedToTrelloCard(n))
            {
                Rect iconRect = r;
                iconRect.width = r.height;
                string cardUrl = TrelloIntegration.GetCardUrl(n);
                if (GUIUtils.ButtonIcon(iconRect, Icons.TRELLO, cardUrl))
                {
                    Application.OpenURL(cardUrl);
                }
            }
        }

        protected void CellGUI_Actions(Rect r, Note n)
        {
            Rect buttonRect = new Rect()
            {
                position = r.position,
                size = Vector2.one * r.height
            };
            if (GUIUtils.ButtonIcon(buttonRect, Icons.TRASH_BIN, "Pernamently Delete"))
            {
                if (EditorUtility.DisplayDialog(
                    "Confirm Delete",
                    $"Delete '{Utilities.Ellipsis(n.name, 64)}' from database? This only delete the note from this local machine.",
                    "Delete", "Cancel"))
                {
                    NoteManager.instance.SetDirty();
                    NoteManager.instance.RecordUndo("Delete note from database");
                    NoteManager.instance.RemoveNote(n.id);
                }
            }

            buttonRect.x += r.height;
            if (GUIUtils.ButtonIcon(buttonRect, Icons.EDIT_NOTE, "Edit"))
            {
                NoteUI.ShowAsPopup(buttonRect, n);
            }

            buttonRect.x += r.height;
            bool isNoteAttached = NoteUtils.IsNoteAttached(n);
            if (Utilities.HasSingleObjectSelection())
            {
                if (!isNoteAttached &&
                    GUIUtils.ButtonIcon(buttonRect, Icons.PIN, "Attach to selected object"))
                {
                    NoteUtils.AttachNote(n, Selection.activeObject);
                    EditorApplication.RepaintHierarchyWindow();
                    EditorApplication.RepaintProjectWindow();
                }
            }

            if (isNoteAttached &&
                GUIUtils.ButtonIcon(buttonRect, Icons.UNPIN, "Detach"))
            {
                NoteUtils.DetachNote(n);
                EditorApplication.RepaintHierarchyWindow();
                EditorApplication.RepaintProjectWindow();
            }
        }

        protected override bool DoesItemMatchSearch(TreeViewItem<int> treeViewItem, string search)
        {
            Item item = (Item)treeViewItem;
            Note note = item.note;

            if (note.name.Contains(search, System.StringComparison.InvariantCultureIgnoreCase) ||
                note.description.Contains(search, System.StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            foreach (string idTag in note.idTags)
            {
                Tag t = NoteManager.instance.GetTagById(idTag);
                if (t != null && t.name.Contains(search, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            if (string.Equals(search, "trello", System.StringComparison.InvariantCultureIgnoreCase) &&
                NoteUtils.IsNoteConnectedToTrelloCard(note))
            {
                return true;
            }

            return false;
        }
    }
}
