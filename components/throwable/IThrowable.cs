using Godot;

public interface IThrowable
{
    void GotThrownBy(IThrower thrower);
    void GotLandedOn(Vector2 pos);
}

