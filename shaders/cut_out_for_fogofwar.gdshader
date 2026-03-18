shader_type canvas_item;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    // white areas become transparent, black stays dark
    COLOR = vec4(0.0, 0.0, 0.0, 1.0 - tex.r);
}