using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LostInLeaves.Rendering
{
    [VolumeComponentMenuForRenderPipeline("Lost In Leaves/Rendering/Pixel Pass", typeof(UniversalRenderPipeline))]
    public class PixelPassComponent : VolumeComponent, IPostProcessComponent
    {
        [System.Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct PixelPassConfiguration
        {
            public int DownscaleAmount;
        }

        [System.Serializable]
        public sealed class PixelConfigurationParameter : VolumeParameter<PixelPassConfiguration>
        {
            public PixelConfigurationParameter(PixelPassConfiguration value, bool overrideState = false)
                : base(value, overrideState) { }
        }

        public PixelConfigurationParameter ConfigurationParam = new PixelConfigurationParameter(default(PixelPassConfiguration));
        public PixelPassConfiguration Config => ConfigurationParam.value;

        public bool IsActive()
        {
            return true;
        }

        public bool IsTileCompatible()
        {
            return false;
        }
    }
}
