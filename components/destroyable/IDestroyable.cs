namespace NotAloneAtHome.Components;

public interface IKillable
{
    int Health { get; }
    void TakeDamage(int damage);
    void Die();
}