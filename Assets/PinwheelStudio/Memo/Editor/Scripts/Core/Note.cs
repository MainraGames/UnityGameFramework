using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Memo.Trello;
using System.Linq;

namespace Pinwheel.Memo
{
    [System.Serializable]
    public class Note
    {
        [SerializeField]
        protected string m_name;
        public string name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        [SerializeField]
        protected string m_description;
        public string description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
            }
        }

        [SerializeField]
        protected string m_id;
        public string id
        {
            get
            {
                return m_id;
            }
        }

        [SerializeField]
        protected Colors m_color;
        public Colors color
        {
            get
            {
                return m_color;
            }
            set
            {
                m_color = value;
            }
        }

        [SerializeField]
        protected AttachTarget m_attachTarget;
        public AttachTarget attachTarget
        {
            get
            {
                return m_attachTarget;
            }
            set
            {
                m_attachTarget = value;
            }
        }

        [SerializeField]
        protected NoteLinkage m_linkage;
        public NoteLinkage linkage
        {
            get
            {
                return m_linkage;
            }
            set
            {
                m_linkage = value;
            }
        }

        [SerializeField]
        protected List<Checklist> m_checklists = new List<Checklist>();
        public List<Checklist> checklists
        {
            get
            {
                return m_checklists;
            }
        }

        [SerializeField]
        protected List<string> m_idTags = new List<string>();
        public List<string> idTags
        {
            get
            {
                return m_idTags;
            }
        }

        [SerializeField]
        protected List<Link> m_links = new List<Link>();
        public List<Link> links
        {
            get
            {
                return m_links;
            }
        }

        internal Note()
        {
            m_id = Utilities.NewId();
            m_color = Colors.Yellow;
        }

        public void PushToRemote()
        {
            if (m_linkage == null || m_linkage.remote == NoteLinkage.Remote.None)
            {
                return;
            }
            else if (m_linkage.remote == NoteLinkage.Remote.TrelloCard)
            {
                if (!string.IsNullOrEmpty(m_linkage.json))
                {
                    TrelloIntegration.PushNote(this, OnAfterTrelloSync);
                }
                else
                {
                    Debug.LogWarning("Attempt to push note to Trello card, but no card linked.");
                }
            }
        }

        private void OnAfterTrelloSync(string error)
        {
            Utilities.LogIfError(error);
            CleanUp();
        }

        public void PullFromRemote()
        {
            if (m_linkage == null || m_linkage.remote == NoteLinkage.Remote.None)
            {
                return;
            }
            else if (m_linkage.remote == NoteLinkage.Remote.TrelloCard)
            {
                if (!string.IsNullOrEmpty(m_linkage.json))
                {
                    TrelloIntegration.PullNote(this, OnAfterTrelloSync);
                }
                else
                {
                    Debug.LogWarning("Attempt to pull content from Trello card, but no card linked.");
                }
            }
        }

        protected void CleanUp()
        {
            //Clean all deleted checklist that's not linked to remote
            checklists.RemoveAll(cl => string.IsNullOrEmpty(cl.idRemote) && cl.isDeleted);
            //Clean all deleted check items that's note linked to remote
            checklists.ForEach((cl) =>
            {
                cl.items.RemoveAll((ci) => string.IsNullOrEmpty(ci.idRemote) && ci.isDeleted);
            });

            //Clean all deleted links that's not linked to remote
            links.RemoveAll(l => string.IsNullOrEmpty(l.idRemote) && l.isDeleted);
        }

        public void GetChecklistsProgress(out int completed, out int total)
        {
            completed = 0;
            total = 0;

            foreach (Checklist checklist in checklists)
            {
                if (checklist != null && !checklist.isDeleted)
                {
                    completed += checklist.items
                        .Where(item => item != null && item.isChecked && !item.isDeleted)
                        .Count();
                    total += checklist.items
                        .Where(item => item != null && !item.isDeleted)
                        .Count();
                }
            }
        }

        public class BadgesInfo
        {
            public bool hasDescription;
            public int completedCheckItemCount;
            public int totalCheckItemCount;
            public int linkCount;
            public int tagCount;
            public Colors[] tagColors;
            public bool linkedToTrello;
        }

        public BadgesInfo GetBadgesInfo()
        {
            BadgesInfo badges = new BadgesInfo();
            badges.hasDescription = !string.IsNullOrEmpty(m_description);
            GetChecklistsProgress(out badges.completedCheckItemCount, out badges.totalCheckItemCount);

            badges.linkCount = links
                .Where(link => link != null && !string.IsNullOrEmpty(link.url) && !link.isDeleted)
                .Count();

            List<Colors> tagColors = new List<Colors>();
            foreach (string idTag in idTags)
            {
                if (NoteManager.instance.HasTag(idTag))
                {
                    Tag tag = NoteManager.instance.GetTagById(idTag);
                    tagColors.Add(tag.color);
                }
            }

            badges.tagColors = tagColors.ToArray();
            badges.tagCount = tagColors.Count;
            badges.linkedToTrello = NoteUtils.IsNoteConnectedToTrelloCard(this);

            return badges;
        }
    }
}
