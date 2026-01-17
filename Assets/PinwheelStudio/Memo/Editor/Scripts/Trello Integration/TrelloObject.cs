using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Memo.Trello
{
    [System.Serializable]
    public class TrelloObject
    {
        [SerializeField]
        protected string m_id;
        public string id
        {
            get
            {
                return m_id;
            }
            protected set
            {
                m_id = value;
            }
        }

        [SerializeField]
        protected string m_name;
        public string name
        {
            get
            {
                return m_name;
            }
            internal set
            {
                m_name = value;
            }
        }

        [System.NonSerialized]
        protected int m_syncCount;
        public int syncCount
        {
            get
            {
                return m_syncCount;
            }
            protected set
            {
                m_syncCount = value;
            }
        }

        public TrelloObject(string id)
        {
            m_id = id;
        }
    }
}
