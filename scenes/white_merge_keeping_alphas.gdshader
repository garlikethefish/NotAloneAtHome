shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float intensity : hint_range(0.0, 2.0) = 1.0;

void fragment() {
    vec4 src = texture(TEXTURE, UV);
    vec4 dst = texture(screen_texture, SCREEN_UV);
    vec4 result = max(src, dst);
    COLOR = vec4(clamp(result.rgb * intensity, 0.0, 1.0), result.a);
}