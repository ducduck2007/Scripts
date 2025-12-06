using System;
using UnityEngine;

namespace UIPool
{
    [AddComponentMenu("UIHelper/Vertical Pool Group", 2)]
    public class VerticalPoolGroup : HorizontalOrVerticalPoolGroup
    {
        //
        // Fields
        //
        [SerializeField]
        private UIHelperEnum.VerticalAlignment m_ChildAlignment;

        //
        // Overide
        //
        protected override void CalcSizeDelta()
        {
            base.CalcSizeDelta();
            //==
            float sizeX = m_ScrollRect.viewport.rect.width;
            float sizeY = 0f;

            //calculate content size
            for (int i = 0; i < adapter.Count; i++)
            {
                listCellSize.Add(GetElementSize(i));
                sizeY = sizeY + GetElementSize(i).y + m_Spacing.y;
                float tmpX = GetElementSize(i).x + m_Spacing.x;
                if (tmpX > sizeX)
                    sizeX = tmpX;
            }

            //set content size delta
            m_ScrollRect.content.sizeDelta = new Vector2(sizeX, sizeY - m_Spacing.y);
            CheckCancelDragIfFits();

            /*
             * calculate init local position of each cell in group.		 
             * anchors min, max, pivot is at (0, 1)
             */

            float posX = 0f;
            float posY = 0f;

            for (int i = 0; i < adapter.Count; i++)
            {
                if (m_ChildAlignment == UIHelperEnum.VerticalAlignment.UpperLeft)
                    posX = 0f;
                else if (m_ChildAlignment == UIHelperEnum.VerticalAlignment.UpperCenter)
                    posX = sizeX * 0.5f - listCellSize[i].x * 0.5f;
                else if (m_ChildAlignment == UIHelperEnum.VerticalAlignment.UpperRight)
                    posX = sizeX - listCellSize[i].x;

                listCellPos.Add(new Vector2(posX + m_Spacing.x, posY));
                posY = posY - (listCellSize[i].y + m_Spacing.y);
            }
        }

        //goi lien tuc khi list scroll
        protected override void UpdateData()
        {
            //Calculate distance between current pivot's position and init pivot's position of layput group
            float offsetY = m_ScrollRect.content.anchoredPosition.y; //init posY is at y = 0 (local)

            //check pool, inactive object if it's out of bound
            foreach (PoolObject po in listPool)
            {
                if (!po.isAvailable)
                {
                    float yTop = listCellPos[po.index].y + offsetY;
                    float yBot = yTop - listCellSize[po.index].y;
                    if (yBot > 0 || yTop < -m_ScrollRect.viewport.rect.height)
                        po.RecycleObject();
                }
            }

            //data
            for (int i = 0; i < adapter.Count; i++)
            {
                float yTop = listCellPos[i].y + offsetY;
                float yBot = yTop - listCellSize[i].y;
                if (yBot > 0 || yTop < -m_ScrollRect.viewport.rect.height || IsCellVisible(i))
                    continue;

                //add cell
                GetPooledObject(i);
            }
        }

        /// <summary>
        /// <para>Scroll pool group to position of element at 'index' of adapter.</para>
        /// <para></para>
        /// <para>Param1: Index of element in adapter.</para>
        /// <para>Param2: Duration for scrolling from beginning to end.</para>
        /// <para>Param3: If value is true and element at 'index' fits in mask height, scroll will cancel.</para>
        /// </summary>
        public override void ScrollTo(int index, float duration = 0, bool cancelIfFitInMaskHeight = false)
        {
            if (cancelIfFitInMaskHeight && IsFitInMaskHeight(index))
                return;

            float maxAbsY = (m_ScrollRect.content.rect.height > m_ScrollRect.viewport.rect.height) ?
                (m_ScrollRect.content.rect.height - m_ScrollRect.viewport.rect.height) : 0f;

            pointFrom = m_ScrollRect.content.anchoredPosition;
            pointTo = pointFrom;

            if (index >= 0 && index < listCellPos.Count)
            {
                float newY = -listCellPos[index].y;
                if (Mathf.Abs(newY) > maxAbsY)
                    newY = maxAbsY;
                pointTo = new Vector2(pointFrom.x, newY);
            }

            if (pointFrom.y != pointTo.y)
            {
                if (duration > 0f)
                {
                    this.duration = duration;
                }
                else
                {
                    m_ScrollRect.content.anchoredPosition = pointTo;
                    UpdateData();
                    if (onScrollFinishedCallback != null)
                        onScrollFinishedCallback();
                }
            }
        }

        protected override void CheckCancelDragIfFits()
        {
            if (m_CancelDragIfFits && vertical)
            {
                if (m_ScrollRect.content.rect.height <= m_ScrollRect.viewport.rect.height)
                    m_ScrollRect.enabled = false;
                else
                    m_ScrollRect.enabled = true;
            }
        }
    }
    //end of class

}