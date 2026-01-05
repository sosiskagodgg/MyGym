using System.Collections;
using UnityEngine;

public class menuAnimationControler : MonoBehaviour
{
    bool active;
    Animator animator;
    Vector2 startPos;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        startPos = GetComponent<RectTransform>().anchoredPosition;
    }
    public void Click()
    {
        if (!active)
        {
            gameObject.SetActive(true);
            animator.SetTrigger("Enable");
            active = !active;
        }
        else
        {
            active = !active;
            animator.SetTrigger("Disable");
        }
    }
    public void Disable()
    {
        gameObject.SetActive(false);
        GetComponent<RectTransform>().anchoredPosition = startPos;
    }
}
