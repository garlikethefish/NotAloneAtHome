shader_type canvas_item;
render_mode unshaded;

uniform float edge_softness : hint_range(0.01, 0.5) = 0.2;

void fragment() {
    vec2 center = UV - vec2(0.5);
    float dist = length(center) * 2.0;
    float alpha = 1.0 - smoothstep(1.0 - edge_softness, 1.0, dist);
    COLOR = vec4(1.0, 1.0, 1.0, alpha);
}