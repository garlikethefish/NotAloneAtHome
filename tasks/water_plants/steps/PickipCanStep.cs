namespace NotAloneAtHome.Tasks.WaterPlantsTask;

using NotAloneAtHome.Tasks.Interfaces;
using System;
using Godot;

public partial class WaterPlantsTask
{
    public class PickupCan : ITaskStep<WaterPlantsTask>
    {
        public string Name => "Pick up watering can";

        public Node Context => Task.Context;

        public WaterPlantsTask Task { get; }

        public PickupCan(WaterPlantsTask task)
        {
            Task = task;
        }

        public void EmitNext()
        {
            Task.Next();
        }

        public void EmitBack()
        {
            Task.Back();
        }

        public void Start() {}
        
        public void End() {}

        public void Finish()
        {
            EmitNext();
        }
    }
}