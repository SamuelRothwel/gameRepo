shader_type canvas_item;

uniform float value : hint_range(0.0, 1.0) = 1.0;

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void fragment() {
    vec2 center_uv = UV - vec2(0.5);
    
    float dist = length(center_uv) * 2.0; 
    float angle = atan(center_uv.y, center_uv.x);
    
    float hue = (angle + PI) / (2.0 * PI);
    
    vec3 hsv = vec3(hue, dist, value);
    vec3 rgb = hsv2rgb(hsv);

    float alpha = step(dist, 1.0);
    
    COLOR = vec4(rgb, alpha);
}