using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using Pinwheel.Memo.UI;
using Pinwheel.Memo.Trello;
using Object = UnityEngine.Object;
using System.IO;
using UnityEditor.SceneManagement;

namespace Pinwheel.Memo
{
    //[CreateAssetMenu(menuName = "Memo/Note Manager")]
    [ExecuteInEditMode]
    [InitializeOnLoad]
    public class NoteManager : ScriptableObject, ISerializationCallbackReceiver
    {
        public delegate void NoteDatabaseHandler(NoteManager sender, Note note);
        public static event NoteDatabaseHandler noteAdded;
        public static event NoteDatabaseHandler noteChanged;
        public static event NoteDatabaseHandler noteRemoved;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            //Load the manager object so it gets deserialize and draw note gizmos correctly
            m_instance = Resources.Load<NoteManager>("Memo/NoteManager");
        }

        protected static NoteManager m_instance;
        public static NoteManager instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = Resources.Load<NoteManager>("Memo/NoteManager");
                }
                if (m_instance == null)
                {
                    throw new System.Exception("Note Manager not found.");
                }
                return m_instance;
            }
        }

        //[SerializeField]
        protected List<Note> m_notes = new List<Note>();

        //[SerializeField]
        protected List<Tag> m_tags = new List<Tag>();

        public bool drawNoteGizmos { get; set; }
        protected string PREF_DRAW_GIZMOS = "memo-draw-gizmos";

        private PrefabStage m_currentPrefabStage;

        private void OnEnable()
        {
            TrelloIntegration.onNotifyAuthorizationResult += OnNotifyTrelloAuthResult;
            NoteAttacher.drawSceneNoteCallback += OnDrawSceneNote;
            NoteAttacher.updateCallback += OnUpdateNoteAttacher;
            EditorApplication.projectWindowItemOnGUI += OnDrawAssetGUI;
            EditorApplication.update += OnEditorUpdate;

            EditorPrefs.GetBool(PREF_DRAW_GIZMOS, true);
        }

        private void OnDisable()
        {
            TrelloIntegration.onNotifyAuthorizationResult -= OnNotifyTrelloAuthResult;
            NoteAttacher.drawSceneNoteCallback -= OnDrawSceneNote;
            NoteAttacher.updateCallback -= OnUpdateNoteAttacher;
            EditorApplication.projectWindowItemOnGUI -= OnDrawAssetGUI;
            EditorApplication.update -= OnEditorUpdate;

            EditorPrefs.SetBool(PREF_DRAW_GIZMOS, drawNoteGizmos);
        }

        private void OnEditorUpdate()
        {
            m_currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        }

        public IEnumerable<Note> GetNotes()
        {
            return m_notes;
        }

        public Note GetNoteById(string id)
        {
            return m_notes.Find(n => string.Equals(n.id, id));
        }

        public Note AddNoteToGameObject(GameObject g)
        {
            RecordUndo("Add note to game object");

            Note note = new Note();
            note.name = "My note";
            note.description = $"This note was created at {System.DateTime.Now.ToString()}";
            note.attachTarget = new AttachTarget();
            note.linkage = new NoteLinkage()
            {
                remote = NoteLinkage.Remote.None
            };
            m_notes.Add(note);
            SetDirty();
            EditorSceneManager.MarkSceneDirty(g.scene);

            NoteUtils.AttachNote(note, g);

            noteAdded?.Invoke(this, note);
            return note;
        }

        public Note AddNoteToAsset(Object asset)
        {
            if (!AssetDatabase.Contains(asset))
            {
                throw new System.ArgumentException($"Add note failed: {asset.name} is not an asset.");
            }

            RecordUndo("Add note to asset");

            Note note = new Note();
            note.name = "My note";
            note.description = $"This note was created at {System.DateTime.Now.ToString()}";
            note.attachTarget = new AttachTarget()
            {
                targetType = AttachTarget.TargetType.ProjectAsset,
                id = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset))
            };
            note.linkage = new NoteLinkage()
            {
                remote = NoteLinkage.Remote.None
            };
            m_notes.Add(note);
            SetDirty();

            noteAdded?.Invoke(this, note);
            return note;
        }

        public Note RemoveNote(string id)
        {
            Note n = m_notes.Find(n => string.Equals(id, n.id));
            if (n != null)
            {
                m_notes.Remove(n);
                noteRemoved?.Invoke(this, n);
                return n;
            }
            else
            {
                return null;
            }
        }

        public Note GetNoteAttachedTo(NoteAttacher attacher)
        {
            Note note = m_notes.Find(n =>
                n.attachTarget != null &&
                n.attachTarget.targetType == AttachTarget.TargetType.SceneObject &&
                !string.IsNullOrEmpty(n.attachTarget.id) &&
                string.Equals(n.attachTarget.id, attacher.id));
            return note;
        }

        public List<Note> GetNotesAttachedTo(string assetGuid)
        {
            return m_notes.FindAll(n =>
                n.attachTarget != null &&
                n.attachTarget.targetType == AttachTarget.TargetType.ProjectAsset &&
                !string.IsNullOrEmpty(n.attachTarget.id) &&
                string.Equals(n.attachTarget.id, assetGuid));
        }

        private void OnDrawSceneNote(NoteAttacher sender, SceneView sceneView)
        {
            if (!drawNoteGizmos)
                return;

            if (m_currentPrefabStage != null)
                return;

            Note note = GetNoteAttachedTo(sender);

            if (note == null)
                return;

            if (IsNoteBeingEdited(note.id))
            {
                return;
            }

            if (Vector3.Distance(sender.transform.position, sceneView.camera.transform.position) > 100) //Don't draw notes too far
                return;

            if (sceneView.camera.transform.InverseTransformPoint(sender.transform.position).z < 0) //Don't draw notes behind camera
                return;

            Vector2 gizmosSize = Vector2.one * 32f / HandleUtility.GetHandleSize(sender.transform.position);
            Vector2 gizmosCenter = HandleUtility.WorldToGUIPoint(sender.transform.position);
            Rect gizmosRect = new Rect();
            gizmosRect.size = gizmosSize;
            gizmosRect.center = gizmosCenter;
            GUIContent buttonContent = EditorGUIUtility.TrTextContent(string.Empty, note.name);

            Handles.BeginGUI();
            if (GUI.Button(gizmosRect, buttonContent, GUIStyle.none))
            {
                Rect popupActivatorRect = new Rect(gizmosRect.position, Vector2.zero);
                NoteUI.ShowAsPopup(popupActivatorRect, note);
            }

            Color guiColor = NoteStyles.GetNoteBackgroundColor(note.color);
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            guiColor.a = 1f;
            GUI.color = guiColor;
            GUI.DrawTexture(gizmosRect, Icons.NOTE_GIZMO);
            GUI.color = Color.white;
            if (NoteUtils.IsNoteConnectedToTrelloCard(note))
            {
                gizmosRect.size *= 0.4f;
                gizmosRect.center = gizmosCenter;
                GUI.DrawTexture(gizmosRect, Icons.TRELLO);
            }
            Handles.EndGUI();
        }

        protected Dictionary<string, string> m_beingEditedNoteIdAndHash = new Dictionary<string, string>();

        public void BeginNoteEditing(string noteId)
        {
            if (m_beingEditedNoteIdAndHash.ContainsKey(noteId))
            {
                throw new Exception($"Note {noteId} is being edited. Forgot to call {nameof(EndNoteEditing)}?");
            }

            Note note = GetNoteById(noteId);
            if (note != null)
            {
                string hash = NoteUtils.CalculateContentHash(note);
                m_beingEditedNoteIdAndHash.Add(noteId, hash);

                if (NoteUtils.IsNoteConnectedToTrelloCard(note))
                {
                    note.PullFromRemote();
                }
            }
        }

        public void EndNoteEditing(string noteId)
        {
            if (!m_beingEditedNoteIdAndHash.ContainsKey(noteId))
            {
                throw new Exception($"Note {noteId} is not being edited. Forgot to call {nameof(BeginNoteEditing)}?");
            }
            string oldHash = m_beingEditedNoteIdAndHash[noteId];
            m_beingEditedNoteIdAndHash.Remove(noteId);

            Note note = GetNoteById(noteId);
            if (note != null)
            {
                string newHash = NoteUtils.CalculateContentHash(note);
                if (!string.Equals(oldHash, newHash))
                {
                    note.PushToRemote();
                    noteChanged?.Invoke(this, note);
                }
            }
        }

        private void OnUpdateNoteAttacher(NoteAttacher sender)
        {
            Note note = GetNoteAttachedTo(sender);
            if (note != null)
            {
                sender.name = $"~Note: {Utilities.Ellipsis(note.name, 32)}";
            }
            else
            {
                if (sender.gameObject.hideFlags == HideFlags.DontSaveInBuild)
                {
                    //GameObject.DestroyImmediate(sender.gameObject);
                }
            }
        }

        public bool IsNoteBeingEdited(string noteId)
        {
            return m_beingEditedNoteIdAndHash.ContainsKey(noteId);
        }

        public new void SetDirty()
        {
            EditorUtility.SetDirty(this);
        }

        public void RecordUndo(string undoName)
        {
            Undo.RecordObject(this, undoName);
        }

        public Tag AddTag(Colors color, string name)
        {
            Tag tag = new Tag();
            tag.color = color;
            tag.name = name;

            m_tags.Add(tag);
            return tag;
        }

        public IEnumerable<Tag> GetTags()
        {
            return m_tags;
        }

        public Tag GetTagById(string tagId)
        {
            return m_tags.Find(t => string.Equals(t.id, tagId));
        }

        public Tag GetTagByName(string tagName)
        {

            return m_tags.Find(t => string.Equals(t.name, tagName));
        }

        public void RemoveTag(string tagId)
        {
            Tag t = m_tags.Find(t => string.Equals(t.id, tagId));
            if (t != null)
            {
                t.isDeleted = true;
            }
        }

        public bool HasTag(string tagId)
        {
            return m_tags.Exists(t => string.Equals(t.id, tagId));
        }

        public void ChangeTagId(Tag tag, string newTagId)
        {
            foreach (Note n in m_notes)
            {
                if (n.idTags.Contains(tag.id))
                {
                    n.idTags.Remove(tag.id);
                    n.idTags.Add(newTagId);
                }
            }

            tag.id = newTagId;
        }

        internal void CleanUpTags()
        {
            m_tags.RemoveAll(t => t == null || t.isDeleted || string.IsNullOrEmpty(t.id));
        }

        private void OnNotifyTrelloAuthResult(bool success)
        {
            if (success)
            {
                foreach (Note n in m_notes)
                {
                    if (NoteUtils.IsNoteConnectedToTrelloCard(n))
                    {
                        n.PullFromRemote();
                    }
                }
            }
        }

        private void OnDrawAssetGUI(string guid, Rect selectionRect)
        {
            if (!drawNoteGizmos)
                return;

            List<Note> notes = GetNotesAttachedTo(guid);
            if (notes.Count == 0)
                return;

            Rect baseButtonRect = new Rect();
            if (selectionRect.width > selectionRect.height)
            {
                baseButtonRect.size = Vector2.one * selectionRect.height;
                baseButtonRect.position = new Vector2(selectionRect.max.x - selectionRect.height, selectionRect.y);
                baseButtonRect = new RectOffset(1, 1, 1, 1).Remove(baseButtonRect);
            }
            else
            {
                baseButtonRect.size = Vector2.one * EditorGUIUtility.singleLineHeight;
                baseButtonRect.position = new Vector2(selectionRect.max.x - baseButtonRect.height, selectionRect.y);
            }

            for (int iNote = 0; iNote < notes.Count; ++iNote)
            {
                Note note = notes[iNote];
                Rect buttonRect = new Rect();
                buttonRect.position = new Vector2(baseButtonRect.x - baseButtonRect.width * iNote, baseButtonRect.y);
                buttonRect.size = baseButtonRect.size;

                if (buttonRect.x < selectionRect.x)
                {
                    continue;
                }

                GUI.color = NoteStyles.GetNoteBackgroundColor(note.color);
                if (GUIUtils.ButtonIcon(buttonRect, Icons.NOTE_GIZMO, note.name))
                {
                    NoteUI.ShowAsPopup(buttonRect, note);
                }
                GUI.color = Color.white;
            }
        }

        [SerializeField]
        [HideInInspector]
        protected string DATA_DIRECTORY;
        const string NOTES_FILE_NAME = "data.memonotes";
        const string TAGS_FILE_NAME = "data.memotags";

        public void OnBeforeSerialize()
        {
            ArrayWrapper<Note> noteArray = new ArrayWrapper<Note>(m_notes);
            string notesJson = JsonUtility.ToJson(noteArray);

            ArrayWrapper<Tag> tagArray = new ArrayWrapper<Tag>(m_tags);
            string tagsJson = JsonUtility.ToJson(tagArray);

            DATA_DIRECTORY = Utilities.GetMemoDataFolder();
            if (!Directory.Exists(DATA_DIRECTORY))
            {
                Directory.CreateDirectory(DATA_DIRECTORY);
            }
            File.WriteAllText(Path.Combine(DATA_DIRECTORY, NOTES_FILE_NAME), notesJson);
            File.WriteAllText(Path.Combine(DATA_DIRECTORY, TAGS_FILE_NAME), tagsJson);
        }

        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(DATA_DIRECTORY))
            {
                m_notes = new List<Note>();
                m_tags = new List<Tag>();
                return;
            }

            string notesFilePath = Path.Combine(DATA_DIRECTORY, NOTES_FILE_NAME);
            if (File.Exists(notesFilePath))
            {
                string notesJson = File.ReadAllText(notesFilePath);
                ArrayWrapper<Note> noteArray = JsonUtility.FromJson<ArrayWrapper<Note>>(notesJson);
                m_notes = new List<Note>(noteArray.items);
            }

            string tagsFilePath = Path.Combine(DATA_DIRECTORY, TAGS_FILE_NAME);
            if (File.Exists(tagsFilePath))
            {
                string tagsJson = File.ReadAllText(tagsFilePath);
                ArrayWrapper<Tag> tagArray = JsonUtility.FromJson<ArrayWrapper<Tag>>(tagsJson);
                m_tags = new List<Tag>(tagArray.items);
            }
        }
    }
}
