shader_type canvas_item;

uniform vec2 center = vec2(0.5, 0.5);
uniform float radius = 0.35;
uniform float softness = 0.2;

void fragment() {
	float dist = distance(SCREEN_UV, center);
	float alpha = smoothstep(radius, radius + softness, dist);
	COLOR = vec4(0.0, 0.0, 0.0, alpha);
}