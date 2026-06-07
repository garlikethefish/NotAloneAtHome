namespace NotAloneAtHome.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public partial class FeedCatTask : TaskNode, ITask
{
    public FeedCatTask(SceneTree ctx) : base(ctx)
    {
        AddStep(new PickupCatFoodStep(this));
        AddStep(new FillUpTheBowlStep(this));
    }

    public override void OnStart()
    {
        UpdateName("Feed the cat");
    }

    public override void OnEnd()
    {
        
    }

    public override void OnFinish()
    {
        
    }
}


