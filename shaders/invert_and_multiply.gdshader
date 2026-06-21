shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

void fragment() {
    vec4 src = texture(TEXTURE, UV);
    vec4 dst = texture(screen_texture, SCREEN_UV);
    vec4 inverted = vec4(1.0 - src.rgb, src.a);
    COLOR = inverted * dst;
}