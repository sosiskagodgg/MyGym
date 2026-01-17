using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SwapManager : MonoBehaviour
{
	#region Логика перетаскивания
	[SerializeField] List<SwapHelper> swapHelpers;
    List<SwapHelper> startSwapHelpers;
    class SwapHelper
    {
        [SerializeField] public RectTransform swapFields;
        [SerializeField] public RectTransform swapObjects;
    }
    private void Awake()
    {
        startSwapHelpers = new List<SwapHelper>();
        foreach (var helper in swapHelpers)
        {
            startSwapHelpers.Add(new SwapHelper
            {
                swapFields = helper.swapFields,
                swapObjects = helper.swapObjects
            });
        }
    }
    #endregion

    public void SwapDays(string dayName, string programName)
    {
        if (dayName != programName)
        {

            Day day1= Week.week.Days.FirstOrDefault(d=>d.name==dayName);
            Day day2 = Week.week.Days.FirstOrDefault(d=>d.name==programName);
            (day1,day2)=(day2,day1);
            Week.SaveDay(day1);
            Week.SaveDay(day2);

        }
    }
}

