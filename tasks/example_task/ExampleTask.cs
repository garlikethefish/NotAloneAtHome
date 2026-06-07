namespace NotAloneAtHome.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public partial class ExampleTask : TaskNode, ITask
{
    public ExampleTask(SceneTree ctx) : base(ctx)
    {
        AddStep(new ExampleTaskStep(this));
    }

    public override void OnStart()
    {
        UpdateName("new Tasks title");
    }

    public override void OnEnd()
    {
        
    }

    public override void OnFinish()
    {
        
    }
}