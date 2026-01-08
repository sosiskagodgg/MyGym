using UnityEngine;
using UnityEngine.UI;

public class ForCanvas : MonoBehaviour
{
    static RectTransform rt;
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public static void UpdateCanvas()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }
}
