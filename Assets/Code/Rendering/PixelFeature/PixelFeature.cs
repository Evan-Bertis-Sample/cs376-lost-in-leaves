using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LostInLeaves.Rendering
{
    public class PixelFeature : ScriptableRendererFeature
    {
        private PixelPass _pixelPass;
        private bool _initialized = false;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!_initialized)
            {
                // init the pixel pass
                _pixelPass.Init();
                _initialized = true;
            }
            renderer.EnqueuePass(_pixelPass);
        }

        public override void Create()
        {
            _pixelPass = new PixelPass();
            _initialized = false;
        }
    }
}
