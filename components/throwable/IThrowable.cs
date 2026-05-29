using Godot;

public interface IThrowable
{
    void WhenThrownBy(IThrower thrower, Vector2 pos);
    void WhenLandedOn(Vector2 pos);
}

