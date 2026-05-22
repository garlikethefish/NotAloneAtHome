using System.Collections.Generic;
using Godot;

namespace NotAloneAtHome.Valuables;

public enum ValuableType {
	TV,
	Bed,
	Chair1,
	Chair2,
	Closet,
	Sofa,
	Table,
	Vase,
	Cabinet,
	Sink,
	Fridge,
	Rug1,
	Rug2,
	Pillow,
	Small_Lamp,
	Tall_Lamp1,
	Tall_Lamp2,
	Bowl,
	Broom,
	Kitchen_Rack,
	None
}

public class Valuable
{
    public float StealValue;
    public Texture2D Sprite;

    public Valuable(float stealValue, string spritePath)
    {
        StealValue = stealValue;
        Sprite = GD.Load<Texture2D>(spritePath);
    }
}

public static class ValuableData
{
    public static readonly Dictionary<ValuableType, Valuable> Valuables = new()
    {
        { ValuableType.TV,           new Valuable(200, "res://canvas_textures/tv_texture.tres") },
        { ValuableType.Bed,          new Valuable(140, "res://canvas_textures/bed_texture.tres") },
        { ValuableType.Chair1,       new Valuable(20,  "res://canvas_textures/chair1_texture.tres") },
        { ValuableType.Chair2,       new Valuable(20,  "res://canvas_textures/chair2_texture.tres") },
        { ValuableType.Closet,       new Valuable(70,  "res://canvas_textures/closet_open_texture.tres") },
        { ValuableType.Sofa,         new Valuable(50,  "res://canvas_textures/sofa_texture.tres") },
        { ValuableType.Table,        new Valuable(40,  "res://canvas_textures/table_texture.tres") },
        { ValuableType.Vase,         new Valuable(10,  "res://canvas_textures/vase1_texture.tres") },
        { ValuableType.Cabinet,      new Valuable(130, "uid://dm1g430wl7ev7") },
        { ValuableType.Sink,         new Valuable(140, "uid://bd8gc4qv0chry") },
        { ValuableType.Fridge,       new Valuable(180, "uid://dwcgwdlp2xq1c") },
        { ValuableType.Rug1,         new Valuable(70,  "uid://c2weke3lo5oqh") },
        { ValuableType.Rug2,         new Valuable(70,  "uid://bbl8ane67drad") },
        { ValuableType.Pillow,       new Valuable(15,  "uid://b5ydv1twi7dqu") },
        { ValuableType.Small_Lamp,   new Valuable(15,  "uid://br3io5crcfp64") },
        { ValuableType.Tall_Lamp1,   new Valuable(25,  "uid://bdve4x8cidaop") },
        { ValuableType.Tall_Lamp2,   new Valuable(25,  "uid://b8q632x7iymn") },
        { ValuableType.Bowl,         new Valuable(8,   "uid://c1w601uykp21g") },
        { ValuableType.Broom,        new Valuable(10,  "uid://fpejoqq76tfe") },
        { ValuableType.Kitchen_Rack, new Valuable(15,  "uid://cvk1m1afh0our") },
    };
}