Shader "Masked/Mask" {
 
	SubShader {
		// Rrenderender the mask after regular geometry
 
		Tags {"Queue" = "Geometry-1" }
 
		Cull Off
		ColorMask 0
 
		// Do nothing specific in the pass:
 
		Pass {}
	}
}