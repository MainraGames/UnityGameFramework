using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Memo
{
    [System.Serializable]
    public class NoteLinkage
    {
        public enum Remote
        {
            None, TrelloCard
        }

        [SerializeField]
        protected Remote m_remote;
        public Remote remote
        {
            get
            {
                return m_remote;
            }
            set
            {
                m_remote = value;
            }
        }

        [SerializeField]
        protected string m_json;
        public string json
        {
            get
            {
                return m_json;
            }
            set
            {
                m_json = value;
            }
        }

        public void Clear()
        {
            m_remote = Remote.None;
            m_json = null;
        }

        public void Set(TrelloLinkage linkage)
        {
            m_remote = Remote.TrelloCard;
            m_json = JsonUtility.ToJson(linkage);
        }
    }

    [System.Serializable]
    public class TrelloLinkage
    {
        public string idList;
        public string idCard;
    }
}
