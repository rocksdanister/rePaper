//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class D2FogsPE : EffectBase
    {
        public Color Color = new Color(1f, 1f, 1f, 1f);
        public float Size = 1f;
        public float HorizontalSpeed = 0.2f;
        public float VerticalSpeed = 0f;
        [Range(0.0f,5)]
        public float Density = 2f;
        public Shader Shader;
        private Material _material;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (Shader == null)
            {
                Shader = Shader.Find("UB/PostEffects/D2Fogs");
            }

            if (_material)
            {
                DestroyImmediate(_material);
                _material = null;
            }
            if (Shader)
            {
                _material = new Material(Shader);
                _material.hideFlags = HideFlags.HideAndDontSave;

                if (_material.HasProperty("_Color"))
                {
                    _material.SetColor("_Color", Color);
                }
                if (_material.HasProperty("_Size"))
                {
                    _material.SetFloat("_Size", Size);
                }
                if (_material.HasProperty("_Speed"))
                {
                    _material.SetFloat("_Speed", HorizontalSpeed);
                }
                if (_material.HasProperty("_VSpeed"))
                {
                    _material.SetFloat("_VSpeed", VerticalSpeed);
                }
                if (_material.HasProperty("_Density"))
                {
                    _material.SetFloat("_Density", Density);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}