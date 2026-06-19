using Godot;
using System;

public partial class FuckassSubviewportUpdater : Node
{
    [Export] public SubViewport[] Subs = [];

    public override void _Ready()
    {
        // Force this node to run LATE so cameras have finished moving
        ProcessPriority = 100;
    }

    public override void _Process(double delta)
    {
        if (Subs == null || Subs.Length == 0) return;

        // 1. Tell ALL subviewports to prepare a single frame update
        foreach (var sub in Subs)
        {
            if (IsInstanceValid(sub))
            {
                // In C#, it's UpdateMode.Once
                sub.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
            }
        }

        // 2. CRITICAL: Force the GPU to draw them all at once OUTSIDE the loop.
        // This is the C# equivalent of RenderingServer.force_draw(false)
        RenderingServer.ForceDraw(false);
    }
}