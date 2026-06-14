shader_type canvas_item;

// Uniforms handled by the C# script
uniform vec4 cone_color : source_color = vec4(1.0, 0.0, 0.0, 0.25);
uniform float worn_mask_alpha_multiplier = 0.4;
uniform bool is_player_wearing_mask = false;

void fragment() {
	// 1. Get UV coordinates from center (0,0) to edges
	vec2 uv = UV * 2.0 - 1.0;
	
	// 2. Calculate distance from center (0.0 to 1.0)
	float dist = length(uv);
	
	// 3. Create a smooth gradient alpha based on distance
	// Shaders imply '1.0' is the edge of the polygon created by raycasts.
	float fade = smoothstep(1.0, 0.0, dist);
	
	// 4. Determine base alpha
	float final_alpha = cone_color.a;
	if (is_player_wearing_mask) {
		final_alpha *= worn_mask_alpha_multiplier;
	}
	
	// 5. Apply gradient fade to alpha
	final_alpha *= fade;
	
	// 6. Output final color
	COLOR = vec4(cone_color.rgb, final_alpha);
}