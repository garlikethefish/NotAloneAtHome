shader_type canvas_item;

void fragment() {
    // 1. Grab the pixels from the SubViewport's texture
    vec4 screen_color = texture(TEXTURE, UV);
    
    // 2. Calculate the brightness (1.0 = pure white, 0.0 = pure black)
    float brightness = dot(screen_color.rgb, vec3(0.299, 0.587, 0.114));
    
    // 3. Inverse brightness becomes the alpha.
    // Pure white (1.0) -> Alpha 0.0 (fully transparent)
    // Mid gray (0.5)    -> Alpha 0.5 (semi-transparent)
    // Pure black (0.0)  -> Alpha 1.0 (fully opaque)
    float new_alpha = screen_color.a * (1.0 - brightness);
    
    // 4. Force the RGB to be pure black (vec3(0.0)).
    // This removes the "white/gray" tint from the semi-transparent areas.
    COLOR = vec4(vec3(0.0), new_alpha);
}