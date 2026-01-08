using TMPro;
using Unity;
using UnityEngine;
//класс накдиовать уже на созданную тень
public class ShadowTest:MonoBehaviour
{
    TextMeshProUGUI _parentText;
    TextMeshProUGUI _thisText;
    private void Awake()
    {
        _parentText = transform.parent.GetComponent<TextMeshProUGUI>();
        _thisText = GetComponent<TextMeshProUGUI>();
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ChangeText);
        _thisText.text = _parentText.text;
    }
    private void OnDestroy()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ChangeText);
    }
    void ChangeText(object o)
    {
        if(o == _parentText)
        {
        _thisText.text=_parentText.text;
        }

    }
}