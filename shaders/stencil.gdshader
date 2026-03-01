shader_type canvas_item;

// This is the texture that will define the hole
uniform sampler2D mask_texture;
// Use this to move the hole around via code or inspector
uniform vec2 mask_offset = vec2(0.5, 0.5); 
uniform float mask_scale = 1.0;

void fragment() {
    vec4 main_color = texture(TEXTURE, UV);
    
    // Calculate UVs for the mask so it stays centered/scaled
    vec2 mask_uv = (UV - mask_offset) / mask_scale + vec2(0.5);
    
    float mask_alpha = 0.0;
    // Only sample if we are inside the mask's UV range (0.0 to 1.0)
    if (mask_uv.x >= 0.0 && mask_uv.x <= 1.0 && mask_uv.y >= 0.0 && mask_uv.y <= 1.0) {
        mask_alpha = texture(mask_texture, mask_uv).a;
    }

    // Subtract the mask alpha from the main sprite's alpha
    main_color.a -= mask_alpha;
    
    // Clamp to ensure alpha doesn't go below 0
    COLOR = vec4(main_color.rgb, max(0.0, main_color.a));
}