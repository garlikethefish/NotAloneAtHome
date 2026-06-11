using Godot;
using NotAloneAtHome.Components;
using NotAloneAtHome.Tasks;
using System;

namespace NotAloneAtHome.Characters;

[Scene]
public partial class CatBowl : Node2D
{
    // Assign the empty-bowl texture in the Inspector.
    [Export] public Texture2D EmptyTexture;

    [Node("DetectableComponent")] private DetectableComponent _detectableComponent; 
    [Node("Sprite2D")] public Sprite2D sprite; 

    private Texture2D _fullTexture;
    
    // Bowls usually start empty until the player fills them!
    public bool HasFood { get; private set; } = false; 

    public override void _Ready()
    {
        // 1. The BOWL belongs to the group, not the food item!
        AddToGroup("food_bowl");

        // Cache the default texture as the "full" state
        _fullTexture = sprite?.Texture;

        if (_detectableComponent != null)
            _detectableComponent.CustomCanBeDetectedBy = CanBeDetectedBy;

        // Initialize sprite matching starting food state
        SwapSprite(empty: !HasFood);
    }

    bool CanBeDetectedBy(AreaDetectorComponent detector)
    {
        // The player needs to detect the bowl only if the task is active AND it's empty
        return TaskManager.Instance.CurrentTask is FeedCatTask && !HasFood;
    }

    // Called by Cat when it finishes its eating timer
    public void ConsumeFood()
    {
        HasFood = false;
        SwapSprite(empty: true);
    }

    // Called by your Task Step when the player drops food here
    public void RefillFood()
    {
        HasFood = true;
        SwapSprite(empty: false);
    }

    private void SwapSprite(bool empty)
    {
        if (sprite == null) return;

        if (empty && EmptyTexture != null)
            sprite.Texture = EmptyTexture;
        else if (!empty && _fullTexture != null)
            sprite.Texture = _fullTexture;
        else
            Modulate = empty ? new Color(0.5f, 0.5f, 0.5f) : Colors.White;
    }
}