using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SwapLeftRight : MonoBehaviour,IDragHandler,IEndDragHandler,IBeginDragHandler
{
    [Header("Настройки свайпа")]
    [SerializeField] public float min;
    [SerializeField] public float max;
    [SerializeField] public bool stopDrag;
    
    protected bool isRightSwipe;
    protected VerticalLayoutGroup verticalLayoutGroup;
    protected ContentSizeFitter contentSizeFitter;
    protected RectTransform rectTransform;
    protected virtual void Awake()
    {
        verticalLayoutGroup = transform.GetComponentInParent<VerticalLayoutGroup>();
        contentSizeFitter = transform.GetComponentInParent<ContentSizeFitter>();
        rectTransform = GetComponent<RectTransform>();
    }


    protected float firstClick = 0f;
    protected Vector2 startPosition { get; private set; }
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        contentSizeFitter.enabled = false;
        verticalLayoutGroup.enabled = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.transform as RectTransform, eventData.position, Camera.main, out Vector2 localPosInParent);
        firstClick = localPosInParent.x;
        startPosition = rectTransform.anchoredPosition;
        StartDragLogic();
    }
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if(!stopDrag)OnDrag(eventData);
    }
    protected virtual void OnDrag(PointerEventData eventData)
    {

        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.transform as RectTransform, eventData.position, Camera.main, out Vector2 localPosInParent);
        if (localPosInParent.x > startPosition.x)
        {
            isRightSwipe = true;
        }
        else if (localPosInParent.x < startPosition.x)
        {
            isRightSwipe = false; 
        }
        float newX = Mathf.Clamp(startPosition.x + (localPosInParent.x - firstClick), min, max);
        localPosInParent = new Vector2(newX, rectTransform.anchoredPosition.y);
        rectTransform.anchoredPosition = localPosInParent;
        DragLogic();
    }
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        isRightSwipe = false;
        contentSizeFitter.enabled = true;
        verticalLayoutGroup.enabled = true;
        EndDragLogic();
    }

    protected abstract void StartDragLogic();
    protected abstract void DragLogic();
    protected abstract void EndDragLogic();
}
