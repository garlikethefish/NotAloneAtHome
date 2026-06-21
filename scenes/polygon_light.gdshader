shader_type canvas_item;
render_mode blend_add;

uniform float energy = 1.0;
uniform vec4 light_color : source_color = vec4(1.0, 0.9, 0.7, 1.0);
uniform float falloff = 2.0;

void fragment() {
    float dist = distance(UV, vec2(0.5));
    float attenuation = clamp(1.0 - (dist * 2.0), 0.0, 1.0);
    attenuation = pow(attenuation, falloff);
    COLOR = light_color * attenuation * energy;
}