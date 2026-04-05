shader_type canvas_item;
uniform sampler2D vision_mask;

void fragment() {
    float mask = texture(vision_mask, SCREEN_UV).r;
    COLOR = vec4(0.0, 0.0, 0.0, 1.0 - mask);
}