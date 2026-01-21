using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ExercisesSwap : SwapLeftRight
{
    [SerializeField] float durationDelitAnimation;
    [SerializeField] float durationBackAnimation;
    [SerializeField] Color colorLeft;
    [SerializeField] Color colorRight;
    [SerializeField] Image image;
    protected override void StartDragLogic()
    {

    }
    protected override void DragLogic()
    {
        SetColor();
    }
    protected override void EndDragLogic()
    {
        if (rectTransform.anchoredPosition.x >= max || rectTransform.anchoredPosition.x <= min) StartCoroutine(DeliteAnimation());
        else StartCoroutine(BackAnimation());
    }

    void SetColor()
    {
        //if (!changeColor)
        //{
        //    if (isRightSwipe) image.color = colorRight;
        //    else image.color = colorLeft;
        //}
        //else
        //{
        if (rectTransform.anchoredPosition.x>0)
        {
            image.color = colorLeft;
        }
        else if (rectTransform.anchoredPosition.x<0)
        {
            image.color = colorRight;
        }
        //}
    }

    private IEnumerator DeliteAnimation()
    {
        contentSizeFitter.enabled = false;
        verticalLayoutGroup.enabled = false;

        float elapsed = 0f;


        Vector2 startPos = rectTransform.anchoredPosition;
        bool swipedRight = startPos.x > startPosition.x;
        bool swipedLeft = startPos.x < startPosition.x;
        Vector2 targetPos = new Vector2(swipedRight ? max + rectTransform.rect.width : min - rectTransform.rect.width, startPosition.y);

        while (elapsed < durationDelitAnimation)
        {
            
            elapsed += Time.deltaTime;
            float t = elapsed / durationDelitAnimation;
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            SetColor();
            yield return null;
        }
        gameObject.SetActive(false);
        DeliteExercise(swipedRight);
        if (swipedLeft) 
        {
            CompleteExercises.CompleteExercise(GetComponent<LowerCard>().exercise,true);
        }
        else
        {
            CompleteExercises.CompleteExercise(GetComponent<LowerCard>().exercise, false);
        }
            contentSizeFitter.enabled = true;
        verticalLayoutGroup.enabled = true;
        OpenStartTrening.UpdateActiveDayCards();
    }
    private IEnumerator BackAnimation()
    {
        contentSizeFitter.enabled = false;
        verticalLayoutGroup.enabled = false;
        Vector2 startPos = rectTransform.anchoredPosition;

        float elapsed = 0f;


        bool swipedRight = startPos.x > startPosition.x;
        bool swipedLeft = startPos.x < startPosition.x;
        float firstBonus = swipedRight ? -10f : 10f;


        Vector2 targetPos = new Vector2(startPosition.x + firstBonus, startPosition.y);

        while (elapsed < durationBackAnimation * 0.5)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (durationBackAnimation * 0.5f);
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            SetColor();
            yield return null;
        }
        elapsed = 0f;
        Vector2 bonusStagePos = rectTransform.anchoredPosition;
        while (elapsed < durationBackAnimation * 0.5)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (durationBackAnimation * 0.5f);
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(bonusStagePos, startPosition, t);

            yield return null;
        }



        contentSizeFitter.enabled = true;
        verticalLayoutGroup.enabled = true;
    }




    void DeliteExercise(bool right)
    {
        SetOfExercises setOfExercises = GetComponentInParent<UpperCard>().setOfExercises;
        Exercise exercise = GetComponent<LowerCard>().exercise;


        int setIndex = Day.ActiveDay.setsOfExercises.FindIndex(set => set.id == setOfExercises.id);
        int exIndex = Day.ActiveDay.setsOfExercises[setIndex].exercises.FindIndex(ex => ex.id == exercise.id);


        Day.ActiveDay.setsOfExercises[setIndex].exercises.Remove(Day.ActiveDay.setsOfExercises[setIndex].exercises[exIndex]);
        Day.ActiveDay = Day.ActiveDay;


    }
}
