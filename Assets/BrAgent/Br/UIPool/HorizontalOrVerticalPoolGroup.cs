using UnityEngine;
using System.Collections.Generic;

namespace UIPool
{
    public abstract class HorizontalOrVerticalPoolGroup : BasePoolGroup
    {
        //
        // Constructors
        //
        protected HorizontalOrVerticalPoolGroup()
        {
        }

        //
        // Method
        //

        /// <summary>
        /// <para>Define the way how you use multiple cell size.</para>
        /// <para></para>
        /// <para>Param: delegate (int 'index of element in adapter')</para>
        /// </summary>
        public void HowToUseCellSize(CellSizeDelegate func)
        {
            cellSizeCallback = func;
        }

        public virtual void ScrollTo(int index, float duration = 0f, bool cancelIfFitInMask = false)
        {
        }

        /// <summary>
        /// <para>Return size of element at 'index' of adapter.</para>
        /// <para></para>
        /// <para>Param: Index of element in adapter</para>
        /// </summary>
        protected Vector2 GetElementSize(int index)
        {
            if (cellSizeCallback != null)
                return cellSizeCallback(index);
            return GetCellSize();
        }

        protected void CheckToAddAtleast3Cells()
        {
            if (adapter.Count == 0)
                return;
            int _1stIndex = 0;
            int _2ndIndex = 1;
            int _3rdIndex = 2;

            bool _1stFound = false;
            bool _2ndFound = false;
            bool _3rdFound = false;

            foreach (PoolObject po in listPool)
            {
                if (po.index == _1stIndex)
                    _1stFound = true;
                else if (po.index == _2ndIndex)
                    _2ndFound = true;
                else if (po.index == _3rdIndex)
                    _3rdFound = true;
            }

            if (_2ndIndex > (adapter.Count - 1))
                _2ndFound = true;
            if (_3rdIndex > (adapter.Count - 1))
                _3rdFound = true;

            if (!_1stFound)
                GetPooledObject(_1stIndex);
            if (!_2ndFound)
                GetPooledObject(_2ndIndex);
            if (!_3rdFound)
                GetPooledObject(_3rdIndex);
        }

        //
        // Overide
        //
        public override void SetAdapter(List<object> adapter, bool toFirst = true)
        {
            base.SetAdapter(adapter, toFirst);
            //==
            CalcSizeDelta();
            ResetPool();
            CheckToAddAtleast3Cells();
            UpdateData();
            //==
            if (toFirst)
                ScrollToFirst();
        }

        /// <summary>
        /// <para>Scroll pool group to position of last element of adapter.</para>
        /// <para></para>
        /// <para>Param: Duration for scrolling from beginning to end.</para>
        /// </summary>
        public override void ScrollToLast(float duration = 0)
        {
            if (adapter.Count <= 0)
                return;
            ScrollTo(adapter.Count - 1, duration, false);
        }
    }
    //end of class
}