shader_type canvas_item;

uniform vec2 forward = vec2(0.0, 1.0);
uniform float vision_angle = 70.0;
uniform float vision_range = 220.0;

uniform float pulse = 0.0;
uniform float noise_strength = 0.08;

void fragment()
{
    vec2 uv = UV * 2.0 - 1.0;

    float dist = length(uv);

    if (dist > 1.0)
        discard;

    vec2 dir = normalize(uv + 0.0001);

    float angle = degrees(acos(dot(dir, forward)));

    float cone_mask = smoothstep(vision_angle, vision_angle * 0.5, angle);

    float range_mask = smoothstep(1.0, 0.0, dist);

    float noise = sin(uv.x * 40.0 + TIME * 2.0) * cos(uv.y * 40.0 + TIME * 2.0);
    noise *= noise_strength;

    float alpha = cone_mask * range_mask;

    alpha += noise;

    float pulse_factor = 0.75 + 0.25 * sin(TIME * 6.0 + pulse);

    vec3 color = vec3(1.0, 0.1, 0.1);

    COLOR = vec4(color, alpha * pulse_factor);
}