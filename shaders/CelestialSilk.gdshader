shader_type spatial;
render_mode cull_disabled;

uniform vec4 base_color : source_color = vec4(0.38, 0.22, 0.60, 1.0);
uniform vec4 shade_color : source_color = vec4(0.32, 0.22, 0.48, 1.0);
uniform vec4 edge_color : source_color = vec4(0.80, 0.70, 1.0, 1.0);
uniform float sheen : hint_range(0.0, 1.0) = 0.45;

void fragment() {
	ALBEDO = base_color.rgb;
	ROUGHNESS = 0.4;
	float edge = pow(1.0 - max(dot(normalize(NORMAL), normalize(VIEW)), 0.0), 3.0);
	EMISSION = edge_color.rgb * edge * 0.16 + base_color.rgb * 0.08;
}

void light() {
	float ndotl = dot(NORMAL, LIGHT);
	float band = 0.30 + smoothstep(-0.35, 0.65, ndotl) * 0.70;
	vec3 tint = mix(shade_color.rgb, vec3(1.0), band);
	DIFFUSE_LIGHT += tint * band * LIGHT_COLOR * ATTENUATION / PI;
	vec3 half_direction = normalize(LIGHT + VIEW);
	float highlight = pow(max(dot(NORMAL, half_direction), 0.0), 48.0);
	SPECULAR_LIGHT += edge_color.rgb * highlight * sheen * LIGHT_COLOR * ATTENUATION;
}