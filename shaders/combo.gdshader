shader_type canvas_item;

//////////////////////
// VERTEX PARAMETERS //
//////////////////////
uniform bool enable_jitter = true;      // Toggle vertex jitter
uniform float interval = 0.5;           // seconds per displacement
uniform float timeDelay = 0.0;          // phase offset
uniform float intensityX = 20.0;
uniform float intensityY = 20.0;
uniform float seed = 42.0;

////////////////////////
// FRAGMENT PARAMETERS //
////////////////////////
uniform bool enable_outline = true;     // Toggle outline
uniform float maxLineWidth: hint_range(0.0, 100.0) = 10.0;
uniform float minLineWidth: hint_range(0.0, 100.0) = 5.0;
uniform float speed: hint_range(0.0, 10.0) = 1.0;
uniform float blockSize: hint_range(0.001, 100.0) = 20.0;
uniform sampler2D color: source_color;
uniform int gradientResolution: hint_range(1, 30) = 10;
uniform float tolerance: hint_range(0.0, 0.999) = 0.0;

//////////////////////
// VERTEX FUNCTION  //
//////////////////////
void vertex() {
    if (enable_jitter) {
        float chunk = floor((TIME + timeDelay) / interval);
        float seedNum = VERTEX.x + VERTEX.y + chunk + seed;
        float offsetX = sin(seedNum * 12.9898) * 43758.5453;
        float offsetY = sin(seedNum * 32.9472) * 94726.0482;
        offsetX = fract(offsetX);
        offsetX = offsetX * 2.0 - 1.0;
        offsetY = fract(offsetY);
        offsetY = offsetY * 2.0 - 1.0;
        VERTEX += vec2(offsetX * intensityX, offsetY * intensityY);
    }

    if (enable_outline) {
        VERTEX = vec2(
            VERTEX.x * (1.0 + TEXTURE_PIXEL_SIZE.x * max(maxLineWidth, minLineWidth) * 2.0),
            VERTEX.y * (1.0 + TEXTURE_PIXEL_SIZE.y * max(maxLineWidth, minLineWidth) * 2.0)
        );
    }
}

////////////////////////
// HELPER FUNCTIONS  //
////////////////////////
bool border(vec2 uv) {
    vec2 uvBorder = abs(uv - vec2(0.5));
    return max(step(0.5, uvBorder.x), step(0.5, uvBorder.y)) > 0.0;
}

float get_alpha(sampler2D tex, vec2 uv){
    if (!border(uv)) {
        return texture(tex, uv).a;
    }
    return 0.0;
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
        mix(hash(b + d.xy, s), hash(b + d.yy, s), f.x), f.y
    );
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

bool in_range(vec2 size, sampler2D tex, vec2 uv) {
    for (float i = -1.0; i < 2.0; i += 2.0) {
        if (get_alpha(tex, uv + vec2(i * size.x, 0.0)) > 0.0) return true;
        if (get_alpha(tex, uv + vec2(0.0, i * size.y)) > 0.0) return true;
        for (float j = -1.0; j < 2.0; j += 2.0) {
            if (get_alpha(tex, uv + vec2(i * size.x, j * size.y)) > 0.0) return true;
            if (get_alpha(tex, uv + vec2(i * size.x, j * size.y * 0.5)) > 0.0) return true;
        }
    }
    return false;
}

float get_distance(vec2 maxDistance, sampler2D tex, vec2 uv) {
    for (int i = 1; i < gradientResolution; i++) {
        vec2 actualDistance = float(i) / float(gradientResolution) * maxDistance;
        if (in_range(actualDistance, tex, uv)) {
            return float(i) / float(gradientResolution);
        }
    }
    return 0.0;
}

////////////////////////
// FRAGMENT FUNCTION //
////////////////////////
void fragment() {
    vec2 uv = UV;
    vec4 newColor = texture(TEXTURE, uv);

    if (enable_outline) {
        uv -= vec2(0.5);
        vec2 edge = TEXTURE_PIXEL_SIZE * max(maxLineWidth, minLineWidth) * 2.0;
        uv = uv + uv * edge;
        uv += vec2(0.5);

        if (newColor.a <= tolerance || border(uv)) {
            float timeStep = floor(TIME * speed);
            vec2 size = TEXTURE_PIXEL_SIZE * get_line_width(uv / TEXTURE_PIXEL_SIZE, timeStep);
            float inside = in_range(size, TEXTURE, uv) ? 1.0 : 0.0;
            vec4 finalColor = inside * texture(color, vec2(get_distance(edge / 2.0 + 0.001, TEXTURE, uv)));
            newColor = finalColor;
        }
    }

    COLOR = newColor;
}
