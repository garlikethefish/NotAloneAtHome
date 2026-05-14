using Godot;

public interface IThrowable : IComponentInterface
{
    void OnThrowBy(IThrower thrower, Vector2 toPosition);
}

