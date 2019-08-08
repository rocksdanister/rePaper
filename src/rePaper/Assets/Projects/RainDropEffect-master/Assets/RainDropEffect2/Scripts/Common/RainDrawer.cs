using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RainDropEffect
{
    [ExecuteInEditMode]
    public class RainDrawer : MonoBehaviour
    {
        [HideInInspector]
        [System.NonSerialized]
        public int RenderQueue = 3000;

        [HideInInspector]
        [System.NonSerialized]
        public Vector3 CameraPos;

        [HideInInspector]
        [System.NonSerialized]
        public Color OverlayColor;

        [HideInInspector]
        [System.NonSerialized]
        public Texture NormalMap;

        [HideInInspector]
        [System.NonSerialized]
        public Texture ReliefTexture;

        [HideInInspector]
        [System.NonSerialized]
        public float DistortionStrength;

        [HideInInspector]
        [System.NonSerialized]
        public float ReliefValue;

        [HideInInspector]
        [System.NonSerialized]
        public float Shiness;

        [HideInInspector]
        [System.NonSerialized]
        public float Blur;

        [HideInInspector]
        [System.NonSerialized]
        public float Darkness;

        [HideInInspector]
        [System.NonSerialized]
        public RainDropTools.RainDropShaderType ShaderType;


        public bool IsEnabled
        {
            get
            {
                return meshRenderer != null && meshRenderer.enabled == true;
            }
        }

        Material material = null;
        MeshFilter meshFilter = null;
        Mesh mesh = null;
        MeshRenderer meshRenderer = null;
        bool changed = false;


        public void Refresh()
        {
            changed = true;
        }

        public void Hide()
        {
            if (meshRenderer != null)
                meshRenderer.enabled = false;
        }

        public void Show()
        {
            if (changed)
            {
                DestroyImmediate(meshRenderer);
                DestroyImmediate(meshFilter);
                meshRenderer = null;
                meshFilter = null;
                material = null;
                mesh = null;
                changed = false;
            }

            if (NormalMap != null)
            {
                if (ShaderType == RainDropTools.RainDropShaderType.Cheap)
                {
                    if (DistortionStrength == 0f)
                    {
                        Hide();
                        return;
                    }
                }
                else
                {
                    if (DistortionStrength == 0f && ReliefValue == 0f && OverlayColor.a == 0f && Blur == 0f)
                    {
                        Hide();
                        return;
                    }
                }
            }
            else
            {
                Debug.LogError("Normal Map is null!");
                Hide();
                return;
            }

            if (material == null)
            {
                material = RainDropTools.CreateRainMaterial(ShaderType, RenderQueue);
            }

            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            if (mesh == null)
            {
                mesh = RainDropTools.CreateQuadMesh();
            }

            // Update shader if needed
            if (material.shader.name != RainDropTools.GetShaderName(ShaderType))
            {
                material = RainDropTools.CreateRainMaterial(ShaderType, material.renderQueue);
            }

            if (material != null && mesh != null && meshFilter != null)
            {
                meshFilter.mesh = mesh;
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.material = material;
                meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                meshRenderer.enabled = true;

                RainDropTools.ApplyRainMaterialValue(
                    material,
                    ShaderType,
                    NormalMap,
                    ReliefTexture,
                    DistortionStrength,
                    OverlayColor,
                    ReliefValue,
                    Blur,
                    Darkness
                );
            }
        }
    }
}