using UnityEngine;
using System;

namespace UIPool
{
    public class UIHelperEnum
    {
        [Flags]
        public enum HorizontalAlignment
        {
            UpperLeft,
            MiddleLeft,
            LowerLeft
        }

        [Flags]
        public enum VerticalAlignment
        {
            UpperLeft,
            UpperCenter,
            UpperRight
        }

        [Flags]
        public enum StartAxis
        {
            //fill all column(x) before fill new row(y)
            Horizontal,
            //fill all row(y) before fill new column(x)
            Vertical
        }

        [Flags]
        public enum GridAlignment
        {
            UpperLeft,
            UpperCenter,
            UpperRight,
            MiddleLeft,
            LowerLeft
        }
    }

    public struct RectStruct
    {
        public float xRight;
        public float xLeft;
        public float xMiddle;

        public float yTop;
        public float yBottom;
        public float yMiddle;

        public static implicit operator RectStruct(RectTransform input)
        {
            RectStruct tmp = new RectStruct();

            tmp.xRight = input.anchoredPosition.x + input.rect.width * (1f - input.pivot.x);
            tmp.xLeft = input.anchoredPosition.x - input.rect.width * input.pivot.x;
            tmp.xMiddle = (tmp.xRight + tmp.xLeft) * 0.5f;

            tmp.yTop = input.anchoredPosition.y + input.rect.height * (1f - input.pivot.y);
            tmp.yBottom = input.anchoredPosition.y - input.rect.height * input.pivot.y;
            tmp.yMiddle = (tmp.yTop + tmp.yBottom) * 0.5f;

            return tmp;
        }
    }
}