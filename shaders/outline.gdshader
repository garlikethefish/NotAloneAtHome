shader_type canvas_item;

uniform float maxLineWidth: hint_range(0.0, 100.0) = 10.0;
uniform float minLineWidth: hint_range(0.0, 100.0) = 5.0;
uniform float speed: hint_range(0.0, 10.0) = 1.0;
uniform float blockSize: hint_range(0.001, 100.0) = 20.0;
uniform sampler2D color: source_color;
uniform int gradientResolution: hint_range(1, 30) = 10;
uniform float tolerance: hint_range(0.0, 0.999) = 0.0;

// Set to 1.0 for full padding. Decrease if you have some transparent space already.
uniform float expansion_intensity : hint_range(0.0, 2.0) = 1.0;

void vertex() {
	// Instead of multiplying the whole VERTEX, we calculate a specific pixel offset.
	// This expands the quad outward by the exact number of pixels needed for the outline.
	float expansion = max(maxLineWidth, minLineWidth) * expansion_intensity;
	
	// Shift vertices outward from the center (0.5, 0.5 in UV space)
	VERTEX += (UV * 2.0 - 1.0) * expansion;
}

// --- YOUR ORIGINAL HELPER FUNCTIONS ---

bool border(vec2 uv) {
	vec2 uvBorder = abs(uv - vec2(0.5));
	return max(step(0.5, uvBorder.x), step(0.5, uvBorder.y)) > 0.0;
}

float get_alpha(sampler2D tex, vec2 uv){
	float res = 0.0;
	if (!border(uv)) {
		res = texture(tex, uv).a;
	}
	return res;
}

float hash(vec2 p, float s) {
	return fract(35.1 * sin(dot(vec3(112.3, 459.2, 753.2), vec3(p, s))));
}

float noise(vec2 p, float s) {
	vec2 d = vec2(0, 1);
	vec2 b = floor(p);
	vec2 f = fract(p);
	return mix(
		mix(hash(b + d.xx, s), hash(b + d.yx, s), f.x),
		mix(hash(b + d.xy, s), hash(b + d.yy, s), f.x), f.y);
}

float get_line_width(vec2 p, float s) {
	p /= blockSize;
	float w = 0.0;
	float intensity = 1.0;
	for (int i = 0; i < 3; i++) {
		w = mix(w, noise(p, s), intensity);
		p /= 2.0;
		intensity /= 2.0;
	}
	return mix(maxLineWidth, minLineWidth, w);
}

float compute_outline(vec2 size, sampler2D tex, vec2 uv) {
	float res = 0.0;
	for (float i = -1.0; i < 2.0; i += 2.0) {
		res += get_alpha(tex, uv + vec2(i * size.x, 0.0));
		res += get_alpha(tex, uv + vec2(0.0, i * size.y));
		for (float j = -1.0; j < 2.0; j += 2.0) {
			res += get_alpha(tex, uv + vec2(i * size.x, j * size.y));
			res += get_alpha(tex, uv + vec2(i * size.x, j * size.y * 0.5));
		}
	}
	return res;
}

bool in_range(vec2 size, sampler2D tex, vec2 uv) {
	for (float i = -1.0; i < 2.0; i += 2.0) {
		if (get_alpha(tex, uv + vec2(i * size.x, 0.0)) > 0.0) {return true;};
		if (get_alpha(tex, uv + vec2(0.0, i * size.y)) > 0.0) {return true;};
		for (float j = -1.0; j < 2.0; j += 2.0) {
			if (get_alpha(tex, uv + vec2(i * size.x, j * size.y)) > 0.0) {return true;};
			if (get_alpha(tex, uv + vec2(i * size.x, j * size.y * 0.5)) > 0.0) {return true;};
		}
	}
	return false;
}

float get_distance(vec2 maxDistance, sampler2D tex, vec2 uv) {
	for (int i = 1; i < gradientResolution; i++) {
		vec2 actualDistance = float(i) / float (gradientResolution) * maxDistance;
		if (in_range(actualDistance, tex, uv)) {
			return float(i) / float (gradientResolution);
		}
	}
	return 0.0;
}

// --- FIXED FRAGMENT LOGIC ---

void fragment() {
	float max_line = max(maxLineWidth, minLineWidth);
	
	if (max_line > 0.0) {
		// Calculate the ratio between the original texture and the expanded quad
		vec2 texture_res = 1.0 / TEXTURE_PIXEL_SIZE;
		float expansion = max_line * expansion_intensity;
		vec2 expanded_res = texture_res + (expansion * 2.0);
		vec2 uv_ratio = expanded_res / texture_res;

		// Center the UVs so the texture stays in the middle of the expanded quad
		vec2 uv = (UV - 0.5) * uv_ratio + 0.5;
		
		// Sample the color from the remapped UVs
		vec4 newColor = texture(TEXTURE, uv);
		
		// If we are outside the 0.0-1.0 range (the added padding) or the pixel is transparent
		if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0 || newColor.a <= tolerance) {
			float timeStep = floor(TIME * speed);
			vec2 size = TEXTURE_PIXEL_SIZE;
			size *= get_line_width(uv / TEXTURE_PIXEL_SIZE, timeStep);
			
			// Compute outline if neighbors exist
			if (in_range(size, TEXTURE, uv)) {
				vec2 edge_for_gradient = TEXTURE_PIXEL_SIZE * max_line * 2.0;
				newColor = texture(color, vec2(get_distance(edge_for_gradient / 2.0 + 0.001, TEXTURE, uv)));
			} else {
				// Ensure pixels outside the outline remain fully transparent
				newColor = vec4(0.0);
			}
		}
		COLOR = newColor;
	}
}