shader_type spatial;
render_mode depth_draw_always, unshaded;

uniform vec3 line_color: source_color = vec3(0.0, 1.0, 0.0);
uniform vec3 base_color: source_color = vec3(0.0, 0.0, 0.0);
uniform float line_width = 2.0;

const vec3 vectors[3] = {
	vec3(1.0, 0.0, 0.0),
	vec3(0.0, 1.0, 0.0),
	vec3(0.0, 0.0, 1.0)
};

varying vec3 bary_coord;
varying vec3 weights;

void vertex() {
	int vtx = VERTEX_ID % 3;
	bary_coord = vectors[vtx];
	if (vtx == 0) {
		weights = vec3(CUSTOM0.x, 0.0, CUSTOM0.y);
	} else if (vtx == 1) {
		weights = vec3(0.0, CUSTOM0.y, CUSTOM0.x);
	} else if (vtx == 2) {
		weights = vec3(CUSTOM0.y, CUSTOM0.x, 0.0);
	}
}

void fragment() {
	float weight;
	if (bary_coord.x > bary_coord.y && bary_coord.y < bary_coord.z) {
		weight = weights.x;
	} else if (bary_coord.z > bary_coord.x && bary_coord.x < bary_coord.y) {
		weight = weights.y;
	} else { // if (bary_coord.y > bary_coord.z && bary_coord.z < bary_coord.x) {
		weight = weights.z;
	}
	vec3 col = mix(base_color, line_color, weight);

	vec3 bary_coord_dx = dFdx(bary_coord);
	vec3 bary_coord_dy = dFdy(bary_coord);
	vec3 bary_coord_d = sqrt(bary_coord_dx * bary_coord_dx + bary_coord_dy * bary_coord_dy);
	vec3 remap = smoothstep(
		vec3(0.0),
		bary_coord_d * line_width,
		bary_coord
	);

	float closest_edge = min(min(remap.x, remap.y), remap.z);

	ALBEDO = mix(col, base_color, closest_edge);
	ALPHA = 1.0;
}
