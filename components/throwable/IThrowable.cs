using Godot;

public interface IThrowable : IComponentInterface
{
    void WhenThrownBy(IThrower thrower, Vector2 toPosition);
}

