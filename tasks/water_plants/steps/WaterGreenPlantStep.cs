namespace NotAloneAtHome.Tasks.WaterPlantsTask;

using NotAloneAtHome.Tasks.Interfaces;
using System;
using Godot;

public partial class WaterPlantsTask
{
    public class WaterGreenPlantStep : ITaskStep<WaterPlantsTask>
    {
        public string Name => "Water the green plant";

        public Node Context => Task.Context;

        public WaterPlantsTask Task { get; }

        public WaterGreenPlantStep(WaterPlantsTask task)
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