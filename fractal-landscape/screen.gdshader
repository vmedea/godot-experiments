shader_type canvas_item;

uniform sampler2D palette : filter_nearest;

void fragment() {
	float idx = texture(TEXTURE, UV).r;
	COLOR = texture(palette, vec2(idx, 0.0));
}
