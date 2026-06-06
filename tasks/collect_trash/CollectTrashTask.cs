namespace NotAloneAtHome.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public partial class CollectTrashTask : TaskNode, ITask
{
    protected int _trashToCollect;
    protected int _trashCollected;
    protected bool _isTrashSpawnedIn;

    public CollectTrashTask(SceneTree ctx, int trashToCollect) : base(ctx)
    {
        _trashToCollect = trashToCollect;
        _isTrashSpawnedIn = false;
        AddStep(new CollectTrashStep(this));
    }

    public override void OnStart()
    {
        UpdateName("Collect trash");
    }

    public override void OnEnd()
    {
        
    }

    public override void OnFinish()
    {
        
    }
}


