namespace NotAloneAtHome.Components.Destroyable;

public interface IDestroyable : IComponentInterface
{
    int Health { get; }
    void TakeDamage(int damage);
    void OnDeath();
}