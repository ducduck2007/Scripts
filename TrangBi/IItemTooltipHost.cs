using UnityEngine;

public interface IItemTooltipHost
{
    void ShowTooltip(int itemId, RectTransform anchor);
    void HideTooltip();
}
