shader_type canvas_item;

uniform sampler2D texture_b : filter_nearest;

void fragment() {
    vec4 src = texture(TEXTURE, UV);
    vec4 dst = texture(texture_b, UV);
    float blackness = 1.0 - src.r;
    COLOR = mix(dst, vec4(0.0, 0.0, 0.0, 1.0), blackness);
}