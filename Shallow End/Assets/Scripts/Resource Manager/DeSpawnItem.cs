using UnityEngine;

public class DeSpawnItem : MonoBehaviour
{

    [SerializeField] private float lifetime = 120f;

    private float timer;

    private void Start()
    {
        timer = lifetime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
