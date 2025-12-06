using UnityEngine;

namespace UIPool
{
    [AddComponentMenu("UIHelper/Horizontal Pool Group", 1)]
    public class HorizontalPoolGroup : HorizontalOrVerticalPoolGroup
    {
        //
        // Fields
        //
        [SerializeField]
        private UIHelperEnum.HorizontalAlignment m_ChildAlignment;
        [SerializeField]
        private bool contentCenter = false;
        //
        // Overide
        //
        protected override void CalcSizeDelta()
        {
            base.CalcSizeDelta();
            //==
            float sizeX = 0f;
            float sizeY = m_ScrollRect.viewport.rect.height;

            //calculate content size
            for (int i = 0; i < adapter.Count; i++)
            {
                listCellSize.Add(GetElementSize(i));
                sizeX = sizeX + GetElementSize(i).x + m_Spacing.x;
                float tmpY = GetElementSize(i).y + m_Spacing.y;
                if (tmpY > sizeY)
                    sizeY = tmpY;
            }

            //set content size delta
            m_ScrollRect.content.sizeDelta = new Vector2(sizeX - m_Spacing.x, sizeY);
            CheckCancelDragIfFits();

            /*
             * calculate init local position of each cell in group.		 
             * anchors min, max, pivot is at (0, 1)
             */

            float posX = 0f;
            float posY = 0f;
            if (contentCenter)
            {
                if (m_ScrollRect.viewport.rect.width > m_ScrollRect.content.sizeDelta.x)
                {
                    posX = (m_ScrollRect.viewport.rect.width - m_ScrollRect.content.sizeDelta.x) / 2;
                }
            }
            for (int i = 0; i < adapter.Count; i++)
            {
                if (m_ChildAlignment == UIHelperEnum.HorizontalAlignment.UpperLeft)
                    posY = 0f;
                else if (m_ChildAlignment == UIHelperEnum.HorizontalAlignment.MiddleLeft)
                    posY = listCellSize[i].y * 0.5f - sizeY * 0.5f;
                else if (m_ChildAlignment == UIHelperEnum.HorizontalAlignment.LowerLeft)
                    posY = listCellSize[i].y - sizeY;
                //
                listCellPos.Add(new Vector2(posX, posY + m_Spacing.y));
                posX = posX + (listCellSize[i].x + m_Spacing.x);
            }
        }

        protected override void UpdateData()
        {
            //Calculate distance between current pivot's position and init pivot's position of layput group
            float offsetX = m_ScrollRect.content.anchoredPosition.x;//init posX is at x = 0 (local)

            //check pool, inactive object if it's out of bound
            foreach (PoolObject po in listPool)
            {
                if (!po.isAvailable)
                {
                    float xLeft = listCellPos[po.index].x + offsetX;
                    float xRight = xLeft + listCellSize[po.index].x;
                    if (xRight < 0 || xLeft > m_ScrollRect.viewport.rect.width)
                        po.RecycleObject();
                }
            }

            //data
            for (int i = 0; i < adapter.Count; i++)
            {
                float xLeft = listCellPos[i].x + offsetX;
                float xRight = xLeft + listCellSize[i].x;
                if (xRight < 0 || xLeft > m_ScrollRect.viewport.rect.width || IsCellVisible(i))
                {
                    continue;
                }

                //add cell
                GetPooledObject(i);
            }
        }

        /// <summary>
        /// <para>Scroll pool group to position of element at 'index' of adapter.</para>
        /// <para></para>
        /// <para>Param1: Index of element in adapter.</para>
        /// <para>Param2: Duration for scrolling from beginning to end.</para>
        /// <para>Param3: If value is true and element at 'index' fits in mask width, scroll will cancel.</para>
        /// </summary>
        public override void ScrollTo(int index, float duration = 0, bool cancelIfFitInMaskWidth = false)
        {
            if (cancelIfFitInMaskWidth && IsFitInMaskWidth(index))
                return;

            float maxAbsX = (m_ScrollRect.content.rect.width > m_ScrollRect.viewport.rect.width) ?
                (m_ScrollRect.content.rect.width - m_ScrollRect.viewport.rect.width) : 0f;

            pointFrom = m_ScrollRect.content.anchoredPosition;
            pointTo = pointFrom;

            if (index >= 0 && index < listCellPos.Count)
            {
                float newX = -listCellPos[index].x;
                if (Mathf.Abs(newX) > maxAbsX)
                    newX = -maxAbsX;
                pointTo = new Vector2(newX, pointFrom.y);
            }

            if (pointFrom.x != pointTo.x)
            {
                if (duration > 0)
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
            if (m_CancelDragIfFits && horizontal)
            {
                if (m_ScrollRect.content.rect.width <= m_ScrollRect.viewport.rect.width)
                    m_ScrollRect.enabled = false;
                else
                    m_ScrollRect.enabled = true;
            }
        }
    }
    //end of class
}