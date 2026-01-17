using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Memo
{
    [System.Serializable]
    public class AttachTarget
    {
        public enum TargetType
        {
            None, SceneObject, ProjectAsset
        }

        [SerializeField]
        protected TargetType m_targetType;
        public TargetType targetType
        {
            get
            {
                return m_targetType;
            }
            set
            {
                m_targetType = value;
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
            set
            {
                m_id = value;
            }
        }
    }
}
