using UnityEngine;

public class OpenClose : MonoBehaviour
{
    [SerializeField] GameObject[] toOpen;
    [SerializeField] GameObject[] toClose;
    bool run;
    public void Click()
    {
        foreach (GameObject go in toOpen)
        {
            go.SetActive(run);
        }
        foreach (GameObject go in toClose)
        {
            go.SetActive(!run);
        }
        run = !run;
    }
}
