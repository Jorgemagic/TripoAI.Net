﻿using DonatelloAI.Importers.Images;
using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Services;
using Evergine.Framework.Threading;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DonatelloAI.ImGui
{
    public static class ImguiHelper
    {
        private static GraphicsContext graphicsContext = null;
        private static IntPtr previewPlaceholderPointer = IntPtr.Zero;        

        public static async Task<Texture> DownloadTextureFromUrl(string url)
        {
            Texture result = null;
            using (HttpClient cliente = new HttpClient())
            {
                using (var response = await cliente.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();

                    using (var fileStream = await response.Content.ReadAsStreamAsync())
                    {
                        result = await GenerateTexture(fileStream);
                        fileStream.Flush();
                    }

                    return result;
                }
            }
        }

        public static async Task<Texture> LoadTextureFromFile(string filepath)
        {            
            Texture result = null;
            using (var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                result = await GenerateTexture(fileStream);
                fileStream.Flush();
            }

            return result;
        }

        private static async Task<Texture> GenerateTexture(Stream stream)
        {
            if (graphicsContext == null)
            {
                graphicsContext = Application.Current.Container.Resolve<GraphicsContext>();
            }

            Texture result = null;
            using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(stream))
            {
                RawImageLoader.CopyImageToArrayPool(image, false, out _, out byte[] data);
                await EvergineForegroundTask.Run(() =>
                {
                    TextureDescription desc = new TextureDescription()
                    {
                        Type = TextureType.Texture2D,
                        Width = (uint)image.Width,
                        Height = (uint)image.Height,
                        Depth = 1,
                        ArraySize = 1,
                        Faces = 1,
                        Usage = ResourceUsage.Default,
                        CpuAccess = ResourceCpuAccess.None,
                        Flags = TextureFlags.ShaderResource,
                        Format = PixelFormat.R8G8B8A8_UNorm,
                        MipLevels = 1,
                        SampleCount = TextureSampleCount.None,
                    };
                    result = graphicsContext.Factory.CreateTexture(ref desc);

                    graphicsContext.UpdateTextureData(result, data);
                });
            }

            return result;
        }

        public static IntPtr SetNoPreviewImage(CustomImGuiManager imguiManager)
        {
            if (previewPlaceholderPointer == IntPtr.Zero)
            {                
                var assetsService = Evergine.Framework.Application.Current.Container.Resolve<AssetsService>();
                var previewPlaceholder = assetsService.Load<Texture>(EvergineContent.Textures.ImageIcon_png);
                previewPlaceholderPointer = imguiManager.CreateImGuiBinding(previewPlaceholder);               
            }

            return previewPlaceholderPointer;            
        }
    }
}
