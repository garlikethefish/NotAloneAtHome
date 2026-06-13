shader_type canvas_item;

uniform sampler2D screen_tex : hint_screen_texture, filter_linear_mipmap;

void fragment() {
    vec3 screen_color = texture(screen_tex, SCREEN_UV).rgb;
    COLOR = vec4(1.0 - screen_color, 1.0);
}