namespace NotAloneAtHome.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public partial class HideDeadBanditTask : TaskNode, ITask
{
    public HideDeadBanditTask(SceneTree ctx) : base(ctx)
    {
        AddStep(new PickUpBanditStep(this));
        AddStep(new HideBanditStep(this));
    }
    public override void OnStart()
    {
        UpdateName("Hide laying bandit");
    }

    public override void OnFinish()
    {
        UpdateName("");
    }
}


