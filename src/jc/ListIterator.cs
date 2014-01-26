using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JC
{
    public class ListItarator<ItemType>
    {
        public ListItarator(List<ItemType> list)
        {
            this.list = list;
            size = list == null ? 0 : list.Count;
        }

        public bool HasNext()
        {
            int currentSize = list == null ? 0 : list.Count;
            if (currentSize != size)
            {
                throw new System.InvalidOperationException("Invalid list iterator");
            }

            if (index < size)
            {
                stateFlag = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public ItemType Get()
        {
            if (!stateFlag)
            {
                throw new System.InvalidOperationException("Invalid list iterator state");
            }

            stateFlag = false;

            return list[index++];
        }

        private List<ItemType> list = null;

        private int size = 0;

        private int index = 0;

        private bool stateFlag = false;
    }
}