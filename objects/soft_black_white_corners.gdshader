shader_type canvas_item;

uniform sampler2D vision_mask;
uniform float blur_amount : hint_range(0.0, 10.0) = 3.0;
uniform float edge_inset : hint_range(0.0, 1.0) = 0.3;

void fragment() {
    vec4 col = vec4(0.0);
    float total = 0.0;
    
    for (int x = -3; x <= 3; x++) {
        for (int y = -3; y <= 3; y++) {
            vec2 offset = vec2(float(x), float(y)) * blur_amount / vec2(textureSize(vision_mask, 0));
            col += texture(vision_mask, SCREEN_UV + offset);
            total += 1.0;
        }
    }
    
    col /= total;
    
    float mask = smoothstep(edge_inset, 1.0, col.r);
    COLOR = vec4(0.0, 0.0, 0.0, 1.0 - mask);
}