shader_type canvas_item;

uniform int slat_size : hint_range(1, 100) = 20;
uniform int gap_size : hint_range(1, 100) = 10;

varying vec2 local_pos;

void vertex() {
    local_pos = VERTEX;
}

void fragment() {
    int period = slat_size + gap_size;
    int y = int(local_pos.y);
    int pos = y % period;
    float visible = step(float(pos), float(slat_size) - 1.0);
    COLOR.a *= visible;
}