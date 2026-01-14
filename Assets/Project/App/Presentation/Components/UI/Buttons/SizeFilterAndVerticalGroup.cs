using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine.UI;

public class SizeFilterAndVerticalGroup : MonoBehaviour
{
    #region Параметры и работа с юнити

    #region Публичные поля

    public float Spacing
    {
        get
        {
            return _spacing;
        }

        set
        {
            _spacing = value;
            SetTransform();
        }
    }
    [SerializeField] public Padding padding;
    #endregion


    [SerializeField] float _spacing;

    RectTransform contentRt;

    List<RectTransform> childrenRt;

#if UNITY_EDITOR
    bool firstValid;
    private void OnValidate()
    {
        if (!firstValid) { firstValid = true; return;}
        if (UnityEditor.EditorApplication.isPlaying &&
            !UnityEditor.EditorApplication.isPaused &&
            enabled)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    SetTransform();
                }
            };
        }
    }
#endif

    #endregion

    #region Монобихейвор методы
    private void Awake()
    {
        contentRt = GetComponent<RectTransform>();
    }
    #endregion

    #region Работа с детьми
    public void SetTransform()
    {
        childrenRt = GetDirectChildrenLINQ();
        SetSizeY(childrenRt);
        HeightDistribution(childrenRt);
        SetContentStartPos(contentRt);
    }
    public List<RectTransform> GetDirectChildrenLINQ()
    {
        return transform.Cast<Transform>()
                        .Select(t => t.GetComponent<RectTransform>())
                        .Where(rt => rt != null)
                        .ToList();
    } 
    #endregion

    #region Контент сайз методы
    private void SetSizeY(List<RectTransform> childrenRt)
    {
        float height = padding.Top + padding.Bottom;
        for (int i = 0; i < childrenRt.Count; i++) { height += childrenRt[i].rect.height + Spacing; }
        contentRt??=GetComponent<RectTransform>();
        contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, height);
    }
    #endregion

    #region Вертикал лей аут методы
    private void HeightDistribution(List<RectTransform> childrenRt)
    {
        float y = childrenRt?.Count>0 ? -childrenRt[0].rect.height / 2 - padding.Top: 0;
        for (int i = 0; i < childrenRt.Count; i++) 
        {
            childrenRt[i].anchoredPosition = new Vector2(childrenRt[i].anchoredPosition.x,y);
            y -= (childrenRt[i].rect.height + _spacing);
        }
    }
    #endregion

    #region Методы для настройки позиции контента
    private void SetContentStartPos(RectTransform content) => content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y - content.rect.height/2);




    #endregion

    #region Доп классы
    [System.Serializable]
    public class Padding
    {
        public float Top = 0f;
        public float Bottom = 0f;
        public float Left = 0f;
        public float Right = 0f;
    }
    #endregion
}
