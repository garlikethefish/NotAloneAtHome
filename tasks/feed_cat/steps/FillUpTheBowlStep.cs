namespace NotAloneAtHome.Tasks;
using NotAloneAtHome.Characters; 
using System.Linq;
using Godot;
using NotAloneAtHome.Components;

public partial class FeedCatTask
{
    public class FillUpTheBowlStep(FeedCatTask task) : TaskStepBase(task), ITaskStep<FeedCatTask>
    {
        public new FeedCatTask Task { get; private set; } = task;
        private CatBowl _catBowl;
        private InteractableComponent _bowlsInteractable;
        private int _tapsToFill = 5;
        public override void OnStart()
        {
            UpdateName($"Fill up the bowl");
            _catBowl = Ctx.GetNodeFromGroup<CatBowl>("task_feed_cat");
            if (!_catBowl.HasChild(out  _bowlsInteractable))
            {
                Log("Cat bowl doesnt have interactable component!");
                return;
            }
            _bowlsInteractable.OnInteractionFrom += InteractionFromPlayer;
        }

        public override void OnStepEnd()
        {
            _bowlsInteractable.OnInteractionFrom -= InteractionFromPlayer;
        }

        public override void OnTaskEnd()
        {
            _bowlsInteractable.OnInteractionFrom -= InteractionFromPlayer;
        }

        void InteractionFromPlayer(InteractorComponent interactor)
        {
            _tapsToFill--;
            if (_tapsToFill <= 0) {
                _catBowl.sprite.Texture = GD.Load<Texture2D>("res://sprites/catbow_filledl.png");
                GoStepForward();
            }
        }
    }
}