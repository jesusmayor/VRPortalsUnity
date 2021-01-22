Shader "TFG/portal"
{
	Properties
	{
		_LeftEyeTexture("Left Eye Texture", 2D) = "white" {}
		_RightEyeTexture("Right Eye Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True"
			"RenderType" = "Transparent" }
		Lighting Off
		Cull Back
		ZWrite On
		ZTest Less
	
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
		
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID 

			};
	
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}
			sampler2D _LeftEyeTexture, _RightEyeTexture;
			fixed4 frag(v2f i) : SV_Target
			{
				// Due to projective geometry we must divide between the last parameter
				// to get the screen projection of the texture
				i.screenPos /= i.screenPos.w;
				fixed4 col;
				if (unity_StereoEyeIndex == 0)
					col = tex2D(_LeftEyeTexture, float2(i.screenPos.x, i.screenPos.y));
				else
					col = tex2D(_RightEyeTexture, float2(i.screenPos.x, i.screenPos.y));

				return col;
			}
			ENDCG


		}
	}
}