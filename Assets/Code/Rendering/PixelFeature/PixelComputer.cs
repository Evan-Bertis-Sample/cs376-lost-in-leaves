using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LostInLeaves.Rendering
{
    public class PixelComputer : IDisposable
    {
        [System.Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct TextureInfo
        {
            public int TextureWidth;
            public int TextureHeight;
        }

        private static ComputeShader _pixelComputeShader;
        private ComputeBuffer _paramBuffer;
        private ComputeBuffer _textureInfoBuffer;

        private int _mainKernel = -1;

        private Vector3Int _groups = new Vector3Int(8, 8, 1);
        private Vector2Int _resolution = new Vector2Int(256, 256);
        private static string _shaderPath;

#if UNITY_EDITOR
        [MenuItem("Tools/Reload Pixel Compute Shader")]
        public static void ReloadShader()
        {
            if (_shaderPath != "")
            {
                _pixelComputeShader = Resources.Load<ComputeShader>(_shaderPath);
                if (_pixelComputeShader == null)
                {
                    Debug.LogError("PixelComputer: Could not load compute shader at path: " + _shaderPath);
                    return;
                }
                
                Debug.Log("PixelComputer: Reload successful!");
            }
            else
            {
                Debug.LogError("PixelComputer: Shader path is empty!");
            }
        }
#endif
        public PixelComputer(string shaderPath)
        {
            _shaderPath = shaderPath;
            _pixelComputeShader = Resources.Load<ComputeShader>(shaderPath);
            if (_pixelComputeShader == null)
            {
                Debug.LogError("PixelComputer: Could not load compute shader at path: " + shaderPath);
                return;
            }

            _mainKernel = _pixelComputeShader.FindKernel("CSMain");
            _paramBuffer = new ComputeBuffer(1, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PixelPassComponent.PixelPassConfiguration)));
            CommandBuffer cmd = CommandBufferPool.Get();
            SetConfiguration(cmd, new PixelPassComponent.PixelPassConfiguration());
            CommandBufferPool.Release(cmd);
            _textureInfoBuffer = new ComputeBuffer(1, System.Runtime.InteropServices.Marshal.SizeOf(typeof(TextureInfo)));

        }

        public void SetInputTexture(CommandBuffer cmd, RenderTargetIdentifier identifier, Vector2Int resolution)
        {
            cmd.SetComputeTextureParam(_pixelComputeShader, _mainKernel, "InputTexture", identifier);
            _resolution = resolution;
        }

        public void SetOutputTexture(CommandBuffer cmd, RenderTargetIdentifier identifier)
        {
            cmd.SetComputeTextureParam(_pixelComputeShader, _mainKernel, "OutputTexture", identifier);
        }

        public void SetConfiguration(CommandBuffer cmd, PixelPassComponent.PixelPassConfiguration config)
        {
            PixelPassComponent.PixelPassConfiguration[] data = new PixelPassComponent.PixelPassConfiguration[1];
            data[0] = config;
            _paramBuffer.SetData(data);
            cmd.SetComputeConstantBufferParam(_pixelComputeShader,"Configuration", _paramBuffer, 0, _paramBuffer.stride);
        }

        public void Compute(CommandBuffer cmd)
        {
            Vector3Int groupCount = new Vector3Int(
                Mathf.CeilToInt(_resolution.x / (float)_groups.x),
                Mathf.CeilToInt(_resolution.y / (float)_groups.y),
                _groups.z
            );

            TextureInfo textureInfo = new TextureInfo()
            {
                TextureWidth = _resolution.x,
                TextureHeight = _resolution.y
            };

            SetTextureInfo(cmd, textureInfo);

            cmd.DispatchCompute(_pixelComputeShader, _mainKernel, groupCount.x, groupCount.y, groupCount.z);
        }

        public bool CanCompute()
        {
            return _pixelComputeShader != null;
        }

        public void Dispose()
        {
            _paramBuffer.Dispose();
            _textureInfoBuffer.Dispose();
        }

        private void SetTextureInfo(CommandBuffer cmd, TextureInfo info)
        {
            TextureInfo[] data = new TextureInfo[1];
            data[0] = info;
            _textureInfoBuffer.SetData(data);
            // Debug.Log($"PixelComputer : Set Texture Size to {info.TextureWidth}x{info.TextureHeight}");
            cmd.SetComputeConstantBufferParam(_pixelComputeShader, "TextureInfo", _textureInfoBuffer, 0, _textureInfoBuffer.stride);
        }
    }
}
