namespace NotAloneAtHome.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NotAloneAtHome.Characters.DeadThiefCloset;
using NotAloneAtHome.Objects;

public partial class HideDeadBanditTask : TaskNode, ITask
{
    DeadBanditCloset closet;
    DeadBandit bandit;

    public HideDeadBanditTask(SceneTree ctx) : base(ctx)
    {
        AddStep(new PickUpBanditStep(this));
        AddStep(new HideBanditStep(this));

        closet = Ctx.GetNodeFromGroup<DeadBanditCloset>("task_hide_dead_bandit");
        if (closet == null)
        {
            Log("Didnt find DeadBanditCloset"); return;
        }
        bandit = Ctx.GetNodeFromGroup<DeadBandit>("task_hide_dead_bandit");
        if (bandit == null)
        {
            Log("Didnt find DeadBandit!"); return;
        }
    }
    
    public override void OnStart()
    {
        UpdateName("Hide laying bandit");
    }

    public override void OnEnd()
    {
    }

    public override void OnFinish()
    {
    }
}


