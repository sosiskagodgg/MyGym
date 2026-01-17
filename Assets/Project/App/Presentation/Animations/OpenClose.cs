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
            if (go != null) go.SetActive(run);
        }
        foreach (GameObject go in toClose)
        {
            if(go!=null) go.SetActive(!run);
        }
        run = !run;
    }
}
