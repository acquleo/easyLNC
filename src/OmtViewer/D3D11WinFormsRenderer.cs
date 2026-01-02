using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace D3D11Rendering
{
    public class D3D11WinFormsRenderer : IDisposable
    {
        private readonly Device _device;
        private readonly SwapChain _swapChain;
        private RenderTargetView _renderTargetView;
        private VertexShader _vertexShader;
        private PixelShader _pixelShader;
        private InputLayout _inputLayout;
        private Buffer _vertexBuffer;
        private Buffer _constantBuffer;
        private SamplerState _samplerState;
        private int _windowWidth;
        private int _windowHeight;

        // Constant buffer structure
        private struct Constants
        {
            public Matrix Transform;
        }

        // Vertex structure
        private struct Vertex
        {
            public Vector3 Position;
            public Vector2 TexCoord;
        }

        public D3D11WinFormsRenderer(Device device, IntPtr windowHandle, int width, int height)
        {
            _device = device;
            _windowWidth = width;
            _windowHeight = height;

            // Create swap chain
            var swapChainDesc = new SwapChainDescription
            {
                BufferCount = 2,
                ModeDescription = new ModeDescription(
                    width,
                    height,
                    new Rational(60, 1),
                    Format.B8G8R8A8_UNorm),
                Usage = Usage.RenderTargetOutput,
                OutputHandle = windowHandle,
                SampleDescription = new SampleDescription(1, 0),
                IsWindowed = true,
                SwapEffect = SwapEffect.FlipDiscard,
                Flags = SwapChainFlags.None
            };

            using (var factory = new Factory1())
            {
                _swapChain = new SwapChain(factory, _device, swapChainDesc);
            }

            // Create render target view
            CreateRenderTarget();

            // Create shaders
            CreateShaders();

            // Create fullscreen quad
            CreateQuad();

            // Create sampler state
            CreateSamplerState();

            // Create constant buffer
            _constantBuffer = new Buffer(
                _device,
                Utilities.SizeOf<Constants>(),
                ResourceUsage.Dynamic,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.Write,
                ResourceOptionFlags.None,
                0);
        }

        private void CreateRenderTarget()
        {
            _renderTargetView?.Dispose();

            using (var backBuffer = _swapChain.GetBackBuffer<Texture2D>(0))
            {
                _renderTargetView = new RenderTargetView(_device, backBuffer);
            }
        }

        private void CreateShaders()
        {
            // Vertex shader HLSL
            string vertexShaderCode = @"
                cbuffer Constants : register(b0)
                {
                    matrix Transform;
                };

                struct VS_INPUT
                {
                    float3 Position : POSITION;
                    float2 TexCoord : TEXCOORD0;
                };

                struct PS_INPUT
                {
                    float4 Position : SV_POSITION;
                    float2 TexCoord : TEXCOORD0;
                };

                PS_INPUT main(VS_INPUT input)
                {
                    PS_INPUT output;
                    output.Position = mul(float4(input.Position, 1.0f), Transform);
                    output.TexCoord = input.TexCoord;
                    return output;
                }
            ";

            // Pixel shader HLSL
            string pixelShaderCode = @"
                Texture2D textureSampler : register(t0);
                SamplerState samplerState : register(s0);

                struct PS_INPUT
                {
                    float4 Position : SV_POSITION;
                    float2 TexCoord : TEXCOORD0;
                };

                float4 main(PS_INPUT input) : SV_TARGET
                {
                    return textureSampler.Sample(samplerState, input.TexCoord);
                }
            ";

            // Compile vertex shader
            using (var vertexShaderByteCode = ShaderBytecode.Compile(
                vertexShaderCode, "main", "vs_5_0", ShaderFlags.None, EffectFlags.None))
            {
                _vertexShader = new VertexShader(_device, vertexShaderByteCode);

                // Create input layout
                var inputElements = new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
                };
                _inputLayout = new InputLayout(_device, vertexShaderByteCode, inputElements);
            }

            // Compile pixel shader
            using (var pixelShaderByteCode = ShaderBytecode.Compile(
                pixelShaderCode, "main", "ps_5_0", ShaderFlags.None, EffectFlags.None))
            {
                _pixelShader = new PixelShader(_device, pixelShaderByteCode);
            }
        }

        private void CreateQuad()
        {
            // Fullscreen quad in normalized device coordinates (-1 to 1)
            var vertices = new[]
            {
                new Vertex { Position = new Vector3(-1,  1, 0), TexCoord = new Vector2(0, 0) },
                new Vertex { Position = new Vector3( 1,  1, 0), TexCoord = new Vector2(1, 0) },
                new Vertex { Position = new Vector3(-1, -1, 0), TexCoord = new Vector2(0, 1) },
                new Vertex { Position = new Vector3( 1, -1, 0), TexCoord = new Vector2(1, 1) }
            };

            _vertexBuffer = Buffer.Create(
                _device,
                BindFlags.VertexBuffer,
                vertices);
        }

        private void CreateSamplerState()
        {
            var samplerDesc = new SamplerStateDescription
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MinimumLod = 0,
                MaximumLod = float.MaxValue
            };

            _samplerState = new SamplerState(_device, samplerDesc);
        }

        private Matrix CalculateAspectRatioTransform(int textureWidth, int textureHeight)
        {
            float textureAspect = (float)textureWidth / textureHeight;
            float windowAspect = (float)_windowWidth / _windowHeight;

            float scaleX = 1.0f;
            float scaleY = 1.0f;

            if (windowAspect > textureAspect)
            {
                // Window is wider - fit to height, letterbox sides
                scaleX = textureAspect / windowAspect;
            }
            else
            {
                // Window is taller - fit to width, letterbox top/bottom
                scaleY = windowAspect / textureAspect;
            }

            // Create scaling matrix (centered)
            return Matrix.Scaling(scaleX, scaleY, 1.0f);
        }

        public void Render(Texture2D texture)
        {
            // Clear the back buffer
            _device.ImmediateContext.ClearRenderTargetView(
                _renderTargetView,
                new RawColor4(0, 0, 0, 1)); // Black background

            // Set viewport
            _device.ImmediateContext.Rasterizer.SetViewport(0, 0, _windowWidth, _windowHeight, 0.0f, 1.0f);

            // Set render target
            _device.ImmediateContext.OutputMerger.SetRenderTargets(_renderTargetView);

            // Calculate transform to maintain aspect ratio
            var transform = CalculateAspectRatioTransform(
                texture.Description.Width,
                texture.Description.Height);

            // Update constant buffer
            var dataBox = _device.ImmediateContext.MapSubresource(
                _constantBuffer,
                0,
                MapMode.WriteDiscard,
                SharpDX.Direct3D11.MapFlags.None);

            Utilities.Write<Matrix>(dataBox.DataPointer, ref transform);
            _device.ImmediateContext.UnmapSubresource(_constantBuffer, 0);

            // Create shader resource view
            using (var srv = new ShaderResourceView(_device, texture))
            {
                // Set shaders and resources
                _device.ImmediateContext.VertexShader.Set(_vertexShader);
                _device.ImmediateContext.VertexShader.SetConstantBuffer(0, _constantBuffer);
                _device.ImmediateContext.PixelShader.Set(_pixelShader);
                _device.ImmediateContext.PixelShader.SetShaderResource(0, srv);
                _device.ImmediateContext.PixelShader.SetSampler(0, _samplerState);

                // Set input layout and vertex buffer
                _device.ImmediateContext.InputAssembler.InputLayout = _inputLayout;
                _device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
                _device.ImmediateContext.InputAssembler.SetVertexBuffers(
                    0,
                    new VertexBufferBinding(_vertexBuffer, Utilities.SizeOf<Vertex>(), 0));

                // Draw
                _device.ImmediateContext.Draw(4, 0);
            }

            // Present
            _swapChain.Present(1, PresentFlags.None);
        }

        public void Resize(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;

            // Release render target
            _renderTargetView?.Dispose();

            // Resize swap chain buffers
            _swapChain.ResizeBuffers(
                2,
                width,
                height,
                Format.B8G8R8A8_UNorm,
                SwapChainFlags.None);

            // Recreate render target
            CreateRenderTarget();
        }

        public void Dispose()
        {
            _constantBuffer?.Dispose();
            _samplerState?.Dispose();
            _vertexBuffer?.Dispose();
            _inputLayout?.Dispose();
            _pixelShader?.Dispose();
            _vertexShader?.Dispose();
            _renderTargetView?.Dispose();
            _swapChain?.Dispose();
        }
    }
}