using UnityEngine;

public class ExperiencePickup : MonoBehaviour
{
    [SerializeField] private int experienceAmount = 1;
    [SerializeField] private float lifetime = 20f;

    public void Initialize(int amount)
    {
        experienceAmount = Mathf.Max(1, amount);
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        player.GainExperience(experienceAmount);
        Destroy(gameObject);
    }
}
