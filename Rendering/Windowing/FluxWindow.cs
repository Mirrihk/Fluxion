// Fluxion.Rendering/Windowing/FluxWindow.cs
using Fluxion.Rendering.Scene;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Fluxion.Rendering.Windowing
{
    public sealed class FluxWindow : GameWindow
    {
        private readonly IScene scene;

        public FluxWindow(string title, int width, int height, IScene scene)
            : base(GameWindowSettings.Default, new NativeWindowSettings
            {
                Title = title,
                ClientSize = new(width, height)
            })
        {
            this.scene = scene;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            // Do NOT create GL objects before this!
            scene.OnLoad();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            scene.OnResize(Size.X, Size.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            scene.OnUpdate(args.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            // Clear; scenes can override depth enable/disable as they like
            GL.ClearColor(0.06f, 0.07f, 0.09f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            scene.OnRender();
            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            scene.Dispose();
        }
    }
}
