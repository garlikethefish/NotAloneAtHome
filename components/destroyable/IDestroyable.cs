namespace NotAloneAtHome.Components.Destroyable;

public interface IDestroyable
{
    int Health { get; }
    void TakeDamage(int damage);
    void OnDeath();
}