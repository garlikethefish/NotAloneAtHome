shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear;

void fragment() {
    // 1. Grab the pixels from the screen
    vec4 screen_color = texture(screen_texture, SCREEN_UV);
    
    // 2. Calculate the brightness (1.0 means pure white)
    float brightness = dot(screen_color.rgb, vec3(0.333, 0.333, 0.333));
    
    // 3. Keep the original colors, but multiply the original alpha by the inverse brightness
    // If it's pure white (brightness = 1.0), alpha becomes 0.0 (completely transparent)
    // If it's any other color, it keeps its original colors and alpha
    float new_alpha = screen_color.a * (1.0 - brightness);
    
    COLOR = vec4(screen_color.rgb, new_alpha);
}