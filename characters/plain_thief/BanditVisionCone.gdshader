shader_type canvas_item;

uniform vec2 forward = vec2(0.0, 1.0);

uniform float vision_angle = 70.0;uniform float vision_range = 220.0;
uniform float scan_offset = 0.0;

uniform float pulse;
uniform float alert_strength;
uniform float focus; // 0 = idle, 1 = locked on player

uniform float noise_strength = 0.12;

float hash(vec2 p)
{
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}


void fragment()
{
    vec2 uv = UV * 2.0 - 1.0;

    float dist = length(uv);
    if (dist > 1.0)
        discard;

    vec2 dir = normalize(uv + 0.0001);

    float angle = degrees(
		    acos(
		        clamp(dot(dir, forward), -1.0, 1.0)
		    )
		);

		angle += degrees(scan_offset);
	

    float cone = smoothstep(vision_angle, vision_angle * 0.4, angle);

    float range_fade = smoothstep(1.0, 0.0, dist);

    float n = hash(uv * 20.0 + pulse);
    float noise = (n - 0.5) * noise_strength;

    float edge_distortion = sin(uv.x * 30.0 + pulse * 4.0) * sin(uv.y * 30.0 + pulse * 3.0);
    edge_distortion *= noise_strength;

    float alpha = cone * range_fade;

    alpha += noise + edge_distortion;

    float alert = mix(0.6, 1.0, alert_strength);

    float focus_width = mix(vision_angle, vision_angle * 0.45, focus);
    float focus_mask = smoothstep(focus_width, focus_width * 0.5, angle);

    vec3 idle_color = vec3(1.0, 0.1, 0.1);
    vec3 alert_color = vec3(1.0, 0.6, 0.1);

    vec3 color = mix(idle_color, alert_color, alert_strength);

    alpha *= focus_mask;

    COLOR = vec4(color, alpha * alert);
}
