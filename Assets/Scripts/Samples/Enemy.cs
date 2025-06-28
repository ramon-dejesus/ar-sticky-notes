using ImprovedTimers;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] DamageNumberSpawner damageNumberSpawner;
    [SerializeField] int damageAmount = 10;
    [SerializeField] float spawnInterval = 0.2f;

    CountdownTimer timer;

    void Start()
    {
        //damageNumberSpawner = GetComponent<DamageNumberSpawner>();
        timer = new CountdownTimer(spawnInterval);
        timer.OnTimerStop += () =>
        {
            damageAmount = Random.Range(5, 25);
            damageNumberSpawner.SpawnDamageNumber(damageAmount, transform.position);
            timer.Start();
        };
        timer.Start();
    }

    public void TakeDamage(int damage) => damageNumberSpawner.SpawnDamageNumber(damage, transform.position);
}