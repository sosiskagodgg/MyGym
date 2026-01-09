using UnityEngine;

public class DestroyOnDisable : MonoBehaviour
{
    private void OnDisable()
    {
        Destroy(gameObject);
    }
}
