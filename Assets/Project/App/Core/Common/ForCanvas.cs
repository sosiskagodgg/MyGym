using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ForCanvas : MonoBehaviour
{
    static RectTransform rt;
    private Keyboard keyboard;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();

        // Получаем ссылку на клавиатуру
        keyboard = Keyboard.current;
    }

    private void Update()
    {
        if (keyboard != null && keyboard.f5Key.wasPressedThisFrame)
        {
            UpdateCanvas();
        }
    }

    public static void UpdateCanvas()
    {
        try
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
        catch (System.Exception)
        {

            
        }
    }
}