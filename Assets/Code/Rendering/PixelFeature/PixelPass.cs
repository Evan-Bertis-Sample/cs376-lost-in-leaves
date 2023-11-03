using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LostInLeaves.Rendering
{
    public class PixelPass : ScriptableRenderPass
    {
        private PixelComputer _pixelComputer;
        private PixelPassComponent _pixelPassComponent;

        private RenderTargetIdentifier _src;
        private RenderTargetIdentifier _out;
        private int _outID = Shader.PropertyToID("_PixelPassDst");

        public void Init()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            _pixelComputer = new PixelComputer("Shaders/PixelCompute");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.enableRandomWrite = true;
            _src = renderingData.cameraData.renderer.cameraColorTarget;
            _out = new RenderTargetIdentifier(_outID);


            cmd.GetTemporaryRT(_outID, descriptor.width, descriptor.height, descriptor.depthBufferBits, FilterMode.Bilinear,
                                descriptor.colorFormat, RenderTextureReadWrite.Default, 1, true, descriptor.memoryless);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Validate pass
            if (_pixelComputer == null || !_pixelComputer.CanCompute())
            {
                Debug.LogError("PixelPass: PixelComputer is null or cannot compute!");
                return;
            }

            VolumeStack stack = VolumeManager.instance.stack;
            if (_pixelPassComponent == null) _pixelPassComponent = stack.GetComponent<PixelPassComponent>();
            if (_pixelPassComponent == null)
            {
                Debug.LogError("PixelPass: PixelPassComponent is null!");
                return;
            }
            if (_pixelPassComponent.IsActive() == false)
            {
                return;
            }

            

            // Execute compute shader
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("PixelPass")))
            {
                RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
                int srcWidth = descriptor.width;
                int srcHeight = descriptor.height;
                Vector2Int resolution = new Vector2Int(srcWidth, srcHeight);

                // Configuration
                _pixelComputer.SetConfiguration(cmd, _pixelPassComponent.Config);
                _pixelComputer.SetInputTexture(cmd, _src, resolution);
                _pixelComputer.SetOutputTexture(cmd, _out);
                _pixelComputer.Compute(cmd);

                // Blit
                Blit(cmd, _out, renderingData.cameraData.renderer.cameraColorTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
            cmd.ReleaseTemporaryRT(_outID);
        }
    }
}
