using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections;
public class CreateFile : MonoBehaviour
{
    [SerializeField] GameObject description;
    private void Awake()
    {
        //StartCoroutine(CreateActiveDay());
    }
    IEnumerator CreateActiveDay()
    {
        var d = Day.ActiveDay;
        yield return null;
    }
}
