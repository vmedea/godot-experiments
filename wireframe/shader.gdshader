shader_type spatial;
render_mode unshaded;

uniform vec3 line_color = vec3(0.0, 1.0, 0.0);
uniform float line_width = 2.0;

const vec3 vectors[3] = {
	vec3(1.0, 0.0, 0.0),
	vec3(0.0, 1.0, 0.0),
	vec3(0.0, 0.0, 1.0)
};

varying vec3 bary_coord;
varying vec3 color0;
varying vec3 color1;
varying vec3 color2;

void vertex() {
	int vtx = VERTEX_ID % 3;
	bary_coord = vectors[vtx];
	if (vtx == 0) {
		color0 = line_color * CUSTOM0.x;
		color1 = vec3(0.0, 0.0, 0.0); // irrelevant
		color2 = line_color * CUSTOM0.y;
	} else if (vtx == 1) {
		color0 = vec3(0.0, 0.0, 0.0); // irrelevant
		color1 = line_color * CUSTOM0.y;
		color2 = line_color * CUSTOM0.x;
	} else if (vtx == 2) {
		color0 = line_color * CUSTOM0.y;
		color1 = line_color * CUSTOM0.x;
		color2 = vec3(0.0, 0.0, 0.0); // irrelevant
	}
}

void fragment() {
	vec3 col = vec3(0.0, 0.0, 0.0);
	if (bary_coord.x > bary_coord.y && bary_coord.y < bary_coord.z) {
		col = color0;
	} else if (bary_coord.z > bary_coord.x && bary_coord.x < bary_coord.y) {
		col = color1;
	} else { // if (bary_coord.y > bary_coord.z && bary_coord.z < bary_coord.x) {
		col = color2;
	}
	
	vec3 bary_coord_dx = dFdx(bary_coord);
	vec3 bary_coord_dy = dFdy(bary_coord);
	vec3 bary_coord_d = sqrt(bary_coord_dx * bary_coord_dx + bary_coord_dy * bary_coord_dy);
	vec3 remap = smoothstep(
		vec3(0.0),
		bary_coord_d * line_width,
		bary_coord
	);
	
	float closest_edge = min(min(remap.x, remap.y), remap.z);
	
	ALBEDO = mix(col, vec3(0.0, 0.0, 0.0), closest_edge);
	ALPHA = 1.0;
}
