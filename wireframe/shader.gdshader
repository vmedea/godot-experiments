shader_type spatial;
render_mode depth_draw_always, unshaded;

uniform vec3 line_color: source_color = vec3(0.0, 1.0, 0.0);
uniform vec3 base_color: source_color = vec3(0.0, 0.0, 0.0);
uniform float line_width = 2.0;

varying vec3 bary_coord;

void vertex() {
	int vtx = VERTEX_ID % 3;
	if (vtx == 0) {
		bary_coord = vec3(1.0, 1.0 - CUSTOM0.x, 1.0 - CUSTOM0.y);
	} else if (vtx == 1) {
		bary_coord = vec3(1.0 - CUSTOM0.y, 1.0, 1.0 - CUSTOM0.x);
	} else if (vtx == 2) {
		bary_coord = vec3(1.0 - CUSTOM0.x, 1.0 - CUSTOM0.y, 1.0);
	}
}

void fragment() {
	vec3 bary_coord_dx = dFdx(bary_coord);
	vec3 bary_coord_dy = dFdy(bary_coord);
	vec3 bary_coord_d = sqrt(bary_coord_dx * bary_coord_dx + bary_coord_dy * bary_coord_dy);

	vec3 remap = smoothstep(
		vec3(0.0),
		bary_coord_d * line_width,
		bary_coord
	);

	float closest_edge = min(min(remap.x, remap.y), remap.z);

	ALBEDO = mix(line_color, base_color, closest_edge);
	ALPHA = 1.0;
}
