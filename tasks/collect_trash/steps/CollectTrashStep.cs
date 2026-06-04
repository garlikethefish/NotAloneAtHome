namespace NotAloneAtHome.Tasks;

using System.Linq;
using Godot;
using NotAloneAtHome.Components;

public partial class CollectTrashTask
{
    public class CollectTrashStep(CollectTrashTask task) : TaskStepBase(task), ITaskStep<CollectTrashTask>
    {
        private readonly PackedScene _trash = GD.Load<PackedScene>("res://objects/trash/Trash.tscn");
        public new CollectTrashTask Task { get; private set; } = task;

        public override void OnStart()
        {
            UpdateName("Trash collected: " + 0);
            SpawnInTrash();
        }

        public override void OnEnd()
        {
            UpdateName("");
        }

        void SpawnInTrash()
        {
            if (Task._isTrashSpawnedIn) return;

            var spawners = Ctx.GetNodesInGroup("task_collect_trash_spawners").Where(s => s.GetChild<SpawnerComponent>() != null);
            foreach (var spawner in spawners) {
                var trashNode = spawner.GetChild<SpawnerComponent>()?.HandleSpawn(_trash);
                trashNode.GetChild<HealthComponent>().OnDeath += CollectTrash;
            }
        }

        public void CollectTrash()
        {
            Task._trashCollected++;
            UpdateName("Trash collected: " + Task._trashCollected);
            if (Task._trashCollected >= Task._trashToCollect) Finish();
        }
    }
}