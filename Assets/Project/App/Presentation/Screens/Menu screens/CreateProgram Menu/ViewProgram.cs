using UnityEngine;

public class ViewProgram : MonoBehaviour
{
    [SerializeField] GameObject programWindow;
    private void Awake()
    {
        GetComponent<DoubleClickEvent>().DoubleClick += (s, e) => OpenProgram();
    }
    public void OpenProgram()
    {
        programWindow.SetActive(true);
    }
}
