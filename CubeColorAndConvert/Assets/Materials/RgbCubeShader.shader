// Based on https://en.wikibooks.org/wiki/Cg_Programming/Unity/RGB_Cube (Public Domain)
// Redid the variables to be more descriptive.

// Shader program used to draw RGB color values into RGB Cube
Shader "Custom/RgbCubeShader"
{
    SubShader { 
        Pass { 
            CGPROGRAM 
            #pragma vertex vert
            #pragma fragment frag
            
            // Strucutre containing position of cube's vertice and assigned color.
            // This allows for processing of multiple vertices by GPU.
            struct CubeVertexOutput {
                float4 position : SV_POSITION; // XYZ Position in camera space as used by Unity (Y up)
                float4 color : TEXCOORD0; // RGB color value stored as float value between 0 and 1.
            };
            
            // Vertex shader used to prepare pixel values for cube.
            CubeVertexOutput vert(float4 vertexObjectSpacePos : POSITION) 
            {
                CubeVertexOutput output;
                // transform input vertice position in object (local) space to camera space (how far it is from camera)
                // This will be used to correctly map shader to cube. 
                output.position = UnityObjectToClipPos(vertexObjectSpacePos);
                // Set color for our vertice based on its local position.
                // Since by default, the vertice position of cube in Unity is between -0.5 and +0.5,
                // we add 0.5 to make it between min/max color values (0 and 1).
                output.color = vertexObjectSpacePos + float4(0.5, 0.5, 0.5, 0.0);
                return output;
            }
            
            // Fragment (pixel) shader used to output visible color into material.
            float4 frag(CubeVertexOutput input) : COLOR 
            {
                return input.color; 
            }
 
            ENDCG  
        }
    }
}
