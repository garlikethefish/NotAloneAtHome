namespace NotAloneAtHome.Components.Destroyable;

public interface IKillable
{
    int Health { get; }
    void TakeDamage(int damage);
    void Die();
}