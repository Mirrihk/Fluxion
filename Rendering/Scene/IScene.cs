// Fluxion.Rendering/Scene/IScene.cs
namespace Fluxion.Rendering.Scene
{
    /// A self-contained OpenGL scene hosted by FluxWindow.
    public interface IScene : System.IDisposable
    {
        void OnLoad();                 // GL context is current here
        void OnResize(int width, int height);
        void OnUpdate(double dt);
        void OnRender();               // draw the frame
    }
}
