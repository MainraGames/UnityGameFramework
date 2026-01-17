using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pinwheel.Memo
{
    public class ArrayWrapper<T>
    {
        public T[] items;

        public ArrayWrapper()
        {

        }

        public ArrayWrapper(IEnumerable<T> collection)
        {
            items = collection.ToArray();
        }
    }
}
