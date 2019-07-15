// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/remoteRavatar"
{
	Properties 
	{
		_ColorTex ("Texture", 2D) = "white" {}
		_DepthTex ("TextureD", 2D) = "white" {}
		_SizeFilter("SizeFilter",Int) = 2
		_sigmaS("SigmaS",Range(0.1,20)) = 3
		_sigmaL("SigmaL",Range(0.1,20)) = 3
		[Toggle] _calculateNormals("Normals", Float) = 0
		_ShaderDistance ("ShaderDistance", Range(0, 1.0)) = 0.1
	}

	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Transparent" }
			
			Cull Off // render both back and front faces

			CGPROGRAM

				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc" 

				// **************************************************************
				// Data structures												*
				// **************************************************************
				struct v2f
				{
					float4	pos		: POSITION;
					float4 color	: COLOR;
					float4 normal	: NORMAL;
				};


				// **************************************************************
				// Vars															*
				// **************************************************************

				sampler2D _ColorTex;				
				sampler2D _DepthTex; 
				int _TexScale;
				float4 _Color;
				float _calculateNormals;
				int _SizeFilter;
				bool _swapBR;
				float _sigmaL;
				float _sigmaS;
				float _ShaderDistance;


				// VARS TO REMOVE HEAD

				int _RemoveHead;
				float3 _VRHead;
				float _HeadSize;
				float _Y_HeadOffset;

				// VARS PARA DISTORCER OS BRACITOS

				int _LeftWarping;
				int _RightWarping;
				//float _UpperArmDistance;
				//float _ForearmDistance;
				//float _HandDistance;
				int _Debug;

				float3 _LEFT_OriginalShoulder;
				float3 _LEFT_OriginalElbow;
				float3 _LEFT_OriginalWrist;
				float3 _LEFT_OriginalHandTip;

				float3 _RIGHT_OriginalShoulder;
				float3 _RIGHT_OriginalElbow;
				float3 _RIGHT_OriginalWrist;
				float3 _RIGHT_OriginalHandTip;

				float4x4 _LEFT_UpperArmMatrix;
				float4x4 _LEFT_ForearmMatrix;
				float4x4 _LEFT_HandMatrix;

				float4x4 _RIGHT_UpperArmMatrix;
				float4x4 _RIGHT_ForearmMatrix;
				float4x4 _RIGHT_HandMatrix;


				float3 head;
				float3 neck;
				float3 spineShoulder;
				float3 spineMid;
				float3 spineBase;
				float3 leftShoulder;
				float3 leftElbow;
				float3 leftWrist;
				float3 leftHand;
				float3 leftThumb;
				float3 leftHandTip;
				float3 leftHip;
				float3 leftKnee;
				float3 leftAnkle;
				float3 leftFoot;
				float3 rightShoulder;
				float3 rightElbow;
				float3 rightWrist;
				float3 rightHand;
				float3 rightThumb;
				float3 rightHandTip;
				float3 rightHip;
				float3 rightKnee;
				float3 rightAnkle;
				float3 rightFoot;
				float3 LEGBONE;


				// **************************************************************
				// Shader Programs												*
				// **************************************************************


				int textureToDepth(float x, float y)
				{
						float4 d = tex2Dlod(_DepthTex,float4(x, y,0,0));
						int dr = d.r*255;
						int dg = d.g*255;
						int db = d.b*255;
						int da = d.a*255;
						int dValue = (int)(db | (dg << 0x8) | (dr << 0x10) | (da << 0x18));
						return dValue;
				}


				#define EPS 1e-5
				float bilateralFilterDepth(float depth, float x, float y)
				{	
					if(_sigmaS ==0 || _sigmaL ==0) return depth;
					float sigS = max(_sigmaS, EPS);
					float sigL = max(_sigmaL, EPS);

					float facS = -1./(2.*sigS*sigS);
					float facL = -1./(2.*sigL*sigL);

					float sumW = 0.;
					float4  sumC = float4(0.0,0.0,0.0,0.0);
					float halfSize = sigS * 2;
					float2 textureSize2 = float2(512,424);
					float2 texCoord = float2(x,y);
					float l = depth;
				
					for (float i = -halfSize; i <= halfSize; i ++){
						for (float j = -halfSize; j <= halfSize; j ++){
						  float2 pos = float2(i, j);
						  
						  float2 coords = texCoord + pos/textureSize2;
						  int offsetDepth = textureToDepth(coords.x,coords.y);
						  float distS = length(pos);
						  float distL = offsetDepth-l;

						  float wS = exp(facS*float(distS*distS));
						  float wL = exp(facL*float(distL*distL));
						  float w = wS*wL;

						  sumW += w;
						  sumC += offsetDepth * w;
						}
					}
					return sumC/sumW;
				}

				
				float medianFilterDepth(int depth,float x, float y)
				{	
					if(_SizeFilter == 0) return depth;
					float2 texCoord = float2(x,y);
					float2 textureSize2 = float2(512,424);
					int sizeArray = (_SizeFilter*2 + 1)*(_SizeFilter*2 + 1);

					int arr[121];

					int k = 0;
					for (float i = -_SizeFilter; i <= _SizeFilter; i ++){
						for (float j = -_SizeFilter; j <= _SizeFilter; j ++){
						  float2 pos = float2(i, j);
						  float2 coords = texCoord + pos/textureSize2;
						  int d = textureToDepth(coords.x,coords.y);
						  arr[k] = d;
						  k++;
						}
					}

					for (int j = 1; j < sizeArray; ++j)
					{
						float key = arr[j];
						int i = j - 1;
						while (i >= 0 && arr[i] > key)
						{
							arr[i+1] = arr[i];
							--i;
						}
						arr[i+1] = key;
					}
					int index = (_SizeFilter*2)+1;
					return arr[index];
					//return depth;
				}

				float4 estimateNormal(float x, float y)
				{
					int width = 512;
					int height = 424;
					float yScale = 0.1;
					float xzScale = 1;
					float deltax =  1.0/width;
					float deltay = 1.0/height;
					float sx = textureToDepth(x< width-deltax ? x+deltax : x, y) -textureToDepth(x>0 ? x-deltax : x, y);
					if (x == 0 || x == width-deltax)
						sx *= 2;

					float sy = textureToDepth(x, y<height-deltay ? y+deltay : y) - textureToDepth(x, y>0 ?  y-deltay : y);
					if (y == 0 || y == height -deltay)
						sy *= 2;

					float4 n =  float4(-sx*yScale, sy*yScale,2*xzScale,1);
					n = normalize(n);
					return n;
				}

				float distToBone(float3 p, float3 start, float3 end)
				{
					float3 v = end - start;
					float3 w = p - start;

					double c1 = dot(w, v);
					if (c1 <= 0)
					{
						return distance(p, start);
					}

					double c2 = dot(v, v);
					if (c2 <= c1)
					{
						return distance(p, end);
					}

					double b = c1 / c2;
					float3 Pb = start + b * v;

					return distance(p, Pb);
				}

				/*
				float   dist_Point_to_Segment(Point P, Segment S)
				{
					Vector v = S.P1 - S.P0;
					Vector w = P - S.P0;

					double c1 = dot(w, v);
					if (c1 <= 0)
						return d(P, S.P0);

					double c2 = dot(v, v);
					if (c2 <= c1)
						return d(P, S.P1);

					double b = c1 / c2;
					Point Pb = S.P0 + b * v;
					return d(P, Pb);
				}
				*/

				// Vertex Shader ------------------------------------------------
				v2f VS_Main(appdata_full v)
				{
					v2f output = (v2f)0;

					float4 c = tex2Dlod(_ColorTex,float4(v.vertex.x,v.vertex.y,0,0));
					int dValue = textureToDepth(v.vertex.x,v.vertex.y);
					dValue = 5000;
					if(dValue == 0)	{
						output.color = float4(0,0,0,0);
						return output;
						}
					
					float4 pos;
					//Median
					dValue = medianFilterDepth(dValue,v.vertex.x,v.vertex.y);
					//float dValue2 =  dValue / 1000.0;
					
					//Bilateral
					float dValue2 = bilateralFilterDepth(dValue,v.vertex.x,v.vertex.y)/1000.0;
	
					pos.z = dValue2; 
					
					float x = 512*v.vertex.x;
					float y = 424*v.vertex.y;
					float vertx = float(x);
					float verty = float(424 -y);
					pos.x =  pos.z*(vertx- 255.5)/351.001462;
					pos.y =  pos.z*(verty-  211.5)/351.001462;
					pos.w = 1;	
					
					

					float4 worldPos = mul(unity_ObjectToWorld, pos);


					if (_RemoveHead == 1)
					{
						_VRHead.y += _Y_HeadOffset;
						if (distance(worldPos, float4(_VRHead, 1.0f)) < _HeadSize)
						{	
							c.a = 0;
							//c.r = 1; c.g = 0; c.b = 0;
						}
					}



					// WARPS
					float4 A;
					float4 B;
					int i;
					float4 p;
					float inc = 0.1f;

					bool leftWarped = false;
					bool rightWarped = false;

					/*

                    // LEFT
                    A = float4(_LEFT_OriginalShoulder, 1.0);
                    B = float4(_LEFT_OriginalElbow, 1.0);
                    for (i = 0; i <= (distance(A, B) / inc) && !leftWarped; i++)
                    {
                        p = A + normalize(B - A) * i * inc;
                        if (distance(worldPos, p) < _UpperArmDistance)
                        {
                            if (_Warping == 1)
                            {
                                pos = mul(unity_WorldToObject, mul(_LEFT_UpperArmMatrix, worldPos));
                                leftWarped = true;
                            }
                            if (_Debug == 1) 
                            { 
                                c.r = 1; c.g = 0; c.b = 0; 
                            }
                            break;
                        }
                    }
                    A = float4(_LEFT_OriginalElbow, 1.0);
                    B = float4(_LEFT_OriginalWrist, 1.0);
                    for (i = 1; i <= (distance(A, B) / inc) && !leftWarped; i++)
                    {
                        p = A + normalize(B - A) * i * inc;
                        if (distance(worldPos, p) < _ForearmDistance)
                        {
                            if (_Warping == 1)
                            {
                                //pos = worldPos;
                                pos =  mul(_LEFT_UpperArmMatrix, worldPos);
                                pos = mul(unity_WorldToObject, mul(_LEFT_ForearmMatrix, pos));
                                leftWarped = true;
                            }								
                            if (_Debug == 1)
                            {
                                c.r = 0; c.g = 1; c.b = 0;
                            }
                            break;
                        }
                    }
                    A = float4(_LEFT_OriginalWrist, 1.0);
                    B = float4(_LEFT_OriginalHandTip, 1.0);
                    for (i = 1; i <= (distance(A, B) / inc) && !leftWarped; i++)
                    {
                        p = A + normalize(B - A) * i * inc;
                        if (distance(worldPos, p) < _HandDistance)
                        {
                            if (_Warping == 1)
                            {
                                pos = mul(_LEFT_UpperArmMatrix, worldPos);
                                pos = mul(_LEFT_ForearmMatrix, pos);
                                pos = mul(unity_WorldToObject, mul(_LEFT_HandMatrix, pos));
                                leftWarped = true;
                            }	
                            if (_Debug == 1)
                            {
                                c.r = 1; c.g = 0; c.b = 1;
                            }
                            break;
                        }
                    }

                    // RIGHT arm
					A = float4(_RIGHT_OriginalShoulder, 1.0);
					B = float4(_RIGHT_OriginalElbow, 1.0);
					for (i = 0; i <= (distance(A, B) / inc) && !rightWarped; i++)
					{
						p = A + normalize(B - A) * i * inc;
						if (distance(worldPos, p) < _UpperArmDistance)
						{
							if (_Warping == 1)
							{
								pos = mul(unity_WorldToObject, mul(_RIGHT_UpperArmMatrix, worldPos));
								rightWarped = true;
							}
							if (_Debug == 1)
							{
								c.r = 1; c.g = 0; c.b = 0;
							}
							break;
						}
					}
					A = float4(_RIGHT_OriginalElbow, 1.0);
					B = float4(_RIGHT_OriginalWrist, 1.0);
					for (i = 1; i <= (distance(A, B) / inc) && !rightWarped; i++)
					{
						p = A + normalize(B - A) * i * inc;
						if (distance(worldPos, p) < _ForearmDistance)
						{
							if (_Warping == 1)
							{
								//pos = worldPos;
								pos = mul(_RIGHT_UpperArmMatrix, worldPos);
								pos = mul(unity_WorldToObject, mul(_RIGHT_ForearmMatrix, pos));
								rightWarped = true;
							}
							if (_Debug == 1)
							{
								c.r = 0; c.g = 1; c.b = 0;
							}
							break;
						}
					}
					A = float4(_RIGHT_OriginalWrist, 1.0);
					B = float4(_RIGHT_OriginalHandTip, 1.0);
					for (i = 1; i <= (distance(A, B) / inc) && !rightWarped; i++)
					{
						p = A + normalize(B - A) * i * inc;
						if (distance(worldPos, p) < _HandDistance)
						{
							if (_Warping == 1)
							{
								pos = mul(_RIGHT_UpperArmMatrix, worldPos);
								pos = mul(_RIGHT_ForearmMatrix, pos);
								pos = mul(unity_WorldToObject, mul(_RIGHT_HandMatrix, pos));
								rightWarped = true;
							}
							if (_Debug == 1)
							{
								c.r = 1; c.g = 0; c.b = 1;
							}
							break;
						}
					}
					// END WARPS
					*/


					// FIND NEAREST BONE


					

					double d = 100000;
					int bone = -1;

					float dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), head, neck);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), neck, spineShoulder);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), spineShoulder, spineMid);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}


					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), spineShoulder, leftShoulder);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), spineShoulder, rightShoulder);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), spineMid, spineBase);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), neck, leftShoulder);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), neck, rightShoulder);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), spineBase, leftHip);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), spineBase, rightHip);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), spineMid, LEGBONE);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), leftHip, LEGBONE);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), rightHip, LEGBONE);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					//dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), rightHip, rightKnee);
					//if (dist < d)
					//{
					//	d = dist;
					//	bone = 0;
					//}

					//dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), rightKnee, rightAnkle);
					//if (dist < d)
					//{
					//	d = dist;
					//	bone = 0;
					//}

					//dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), rightAnkle, rightFoot);
					//if (dist < d)
					//{
					//	d = dist;
					//	bone = 0;
					//}

					//dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), leftHip, leftKnee);
					//if (dist < d)
					//{
					//	d = dist;
					//	bone = 0;
					//}

					//dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), leftHip, leftKnee);
					//if (dist < d)
					//{
					//	d = dist;
					//	bone = 0;
					//}

					//dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), leftKnee, leftAnkle);
					//if (dist < d)
					//{
					//	d = dist;
					//	bone = 0;
					//}

					//dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), leftAnkle, leftFoot);
					//if (dist < d)
					//{
					//	d = dist;
					//	bone = 0;
					//}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), rightShoulder, rightHip);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), leftShoulder, leftHip);
					if (dist < d)
					{
						d = dist;
						bone = 0;
					}

					// right upperarm
					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), rightShoulder, rightElbow);
					//if (dist < d && d < 0.2f) uat?
					if (dist < d)
					{
						d = dist;
						bone = 1;
					}

					// right forearm
					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), rightElbow, rightWrist);
					if (dist < d)
					{
						d = dist;
						bone = 2;
					}

					// right hand
					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), rightWrist, rightHand);
					if (dist < d)
					{
						d = dist;
						bone = 3;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), rightHand, rightThumb);
					if (dist < d)
					{
						d = dist;
						bone = 3;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), rightHand, rightHandTip);
					if (dist < d)
					{
						d = dist;
						bone = 3;
					}

					// left upperarm
					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), leftShoulder, leftElbow);
					if (dist < d)
					{
						d = dist;
						bone = 4;
					}

					// left forearm
					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), leftElbow, leftWrist);
					if (dist < d)
					{
						d = dist;
						bone = 5;
					}

					// left hand
					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), leftWrist, leftHand);
					if (dist < d)
					{
						d = dist;
						bone = 6;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), leftHand, leftThumb);
					if (dist < d)
					{
						d = dist;
						bone = 6;
					}

					dist = distToBone(float3(worldPos.x, worldPos.y, worldPos.z), leftHand, leftHandTip);
					if (dist < d)
					{
						d = dist;
						bone = 6;
					}

					//right upper arm
					if (bone == 1)
					{
						if (_Debug == 1)
						{
							c.r = 1; c.g = 0; c.b = 1;
						}
						if (_RightWarping == 1)
						{
							pos = mul(unity_WorldToObject, mul(_RIGHT_UpperArmMatrix, worldPos));
						}
					}
					//right forearm
					else if (bone == 2)
					{
						if (_Debug == 1)
						{
							c.r = 1; c.g = 1; c.b = 0;
						}
						if (_RightWarping == 1)
						{
							pos = mul(_RIGHT_UpperArmMatrix, worldPos);
							pos = mul(unity_WorldToObject, mul(_RIGHT_ForearmMatrix, pos));
						}
					}
					//right hand
					else if (bone == 3)
					{
						if (_Debug == 1)
						{
							c.r = 0; c.g = 1; c.b = 1;
						}
						if (_RightWarping == 1)
						{
							pos = mul(_RIGHT_UpperArmMatrix, worldPos);
							pos = mul(_RIGHT_ForearmMatrix, pos);
							pos = mul(unity_WorldToObject, mul(_RIGHT_HandMatrix, pos));
						}
					}
					//left upper arm
					else if (bone == 4)
					{
						if (_Debug == 1)
						{
							c.r = 1; c.g = 0; c.b = 1;
						}
						if (_LeftWarping == 1)
						{
							pos = mul(unity_WorldToObject, mul(_LEFT_UpperArmMatrix, worldPos));
						}
					}
					//left forearm
					else if (bone == 5)
					{
						if (_Debug == 1)
						{
							c.r = 1; c.g = 1; c.b = 0;
						}
						if (_LeftWarping == 1)
						{
							pos = mul(_LEFT_UpperArmMatrix, worldPos);
							pos = mul(unity_WorldToObject, mul(_LEFT_ForearmMatrix, pos));
						}
					}
					//left hand
					else if (bone == 6)
					{
						if (_Debug == 1)
						{
							c.r = 0; c.g = 1; c.b = 1;
						}
						if (_LeftWarping == 1)
						{
							pos = mul(_LEFT_UpperArmMatrix, worldPos);
							pos = mul(_LEFT_ForearmMatrix, pos);
							pos = mul(unity_WorldToObject, mul(_LEFT_HandMatrix, pos));
						}
					}
					else if (bone == -1)
					{
						if (_Debug == 1)
						{
							c.r = 1; c.g = 0; c.b = 0;
						}
					}
					else if (bone == 0)
					{
						if (_Debug == 1)
						{
							//c.r = 0; c.g = 0; c.b = 0;
						}
					}
				


					// END NEAREST BONE

					output.pos =  pos;
					output.color = c;
					//int intpart;
					//float dColor = modf(dValue2,intpart);
					//output.color = float4(dColor,dColor,dColor,1);
					if(_calculateNormals != 0)
					{
						output.normal = estimateNormal(v.vertex.x,v.vertex.y);
					}
					else
					{
						output.normal= float4(0,0,0,0);
					}
					//output.color = output.n;
					
					return output;
				}

			
				// Geometry Shader -----------------------------------------------------
			[maxvertexcount(3)]
			void GS_Main(triangle v2f input[3], inout TriangleStream<v2f> OutputStream)
			{

				float lod = 0; // your lod level ranging from 0 to number of mipmap levels.
				float c0 = input[0].color.a;
				float c1 = input[1].color.a;
				float c2 = input[2].color.a;

				if (distance(input[0].pos, input[1].pos) < _ShaderDistance & distance(input[0].pos, input[2].pos) < _ShaderDistance & distance(input[1].pos, input[2].pos) < _ShaderDistance
					& c0 != 0 & c1 != 0 & c2 != 0)
				{
					v2f outV;
					outV.pos = UnityObjectToClipPos(input[0].pos);
					outV.color = input[0].color;
					outV.normal = input[0].normal;
					OutputStream.Append(outV);
					outV.pos = UnityObjectToClipPos(input[1].pos);
					outV.color = input[1].color;
					outV.normal = input[1].normal;
					OutputStream.Append(outV);
					outV.pos = UnityObjectToClipPos(input[2].pos);
					outV.color = input[2].color;
					outV.normal = input[2].normal;
					OutputStream.Append(outV);	
				}
			}
			
			// Fragment Shader -----------------------------------------------
			float4 FS_Main(v2f input) : COLOR
			{
				// sample the texture
				fixed4 col = input.color;
				// apply fog
				UNITY_APPLY_FOG(input.fogCoord, col);
				return col;
			}

			ENDCG
		}
	} 
}
