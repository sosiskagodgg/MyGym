using UnityEngine;

public class UpdateActiveCards : MonoBehaviour
{
    [SerializeField] GameObject updateObject;
    static GameObject _updateObject;
    private void Awake()
    {
        _updateObject = updateObject;
    }
    public static void UpdateCards()
    {
        _updateObject.SetActive(false);
        _updateObject.SetActive(true);
    }
}
