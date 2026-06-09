shader_type canvas_item;

uniform sampler2D mask_texture;

void fragment() {
    vec4 mask = texture(mask_texture, UV);
    float brightness = dot(mask.rgb, vec3(0.333, 0.333, 0.333));
    COLOR = vec4(0.0, 0.0, 0.0, 1.0 - brightness);
}