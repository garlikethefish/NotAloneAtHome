using Godot;
using NotAloneAtHome.Characters.Player;
public partial class NoiseMeter : TextureProgressBar
{
    NoiseReciever _noiseReciever;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        CallDeferred(MethodName.LinkPlayer);
    }

    void LinkPlayer()
    {
        _noiseReciever = GetTree().GetNodeFromGroup<Player>("player").GetChild<NoiseReciever>();
        if (_noiseReciever == null)
        {
            GD.Print("NO PLAYER");
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        Value = _noiseReciever.CurrentNoise;
    }
}
