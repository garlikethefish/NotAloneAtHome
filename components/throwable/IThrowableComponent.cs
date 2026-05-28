using Godot;

public interface IThrowableComponent
{
    void HandleThrownBy(IThrower thrower, Vector2 toPosition);
}

