using UnityEngine;

public class OpenClose : MonoBehaviour
{
    public void Click()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }
}
