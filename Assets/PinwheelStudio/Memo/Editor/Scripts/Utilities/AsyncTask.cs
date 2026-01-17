using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Memo
{
    public class AsyncTask : CustomYieldInstruction
    {
        protected bool m_isCompleted;
        public override bool keepWaiting => !m_isCompleted;

        public void Complete()
        {
            m_isCompleted = true;
        }

        public bool IsCompleted()
        {
            return m_isCompleted;
        }
    }
}
