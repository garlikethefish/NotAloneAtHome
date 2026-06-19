shader_type canvas_item;

uniform float intensity : hint_range(1.0, 10.0) = 1.0;

void fragment() {
    vec4 src = texture(TEXTURE, UV);
    vec3 inverted = vec3(1.0 - src.r, 1.0 - src.g, 1.0 - src.b);
    vec3 boosted = vec3(1.0) - clamp((vec3(1.0) - inverted) * intensity, 0.0, 1.0);
    COLOR = vec4(boosted, 1.0);
}