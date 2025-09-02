// File: Fluxion.Rendering/Camera/OrbitCamera.cs
using OpenTK.Mathematics;

namespace Fluxion.Rendering.Camera
{
    public sealed class OrbitCamera
    {
        public Vector3d Target { get; set; } = Vector3d.Zero;
        public double Distance { get; set; } = 10.0;
        public double Yaw { get; set; } = 0.9;
        public double Pitch { get; set; } = 0.6;

        public Matrix4 GetViewMatrix()
        {
            var cp = System.Math.Clamp(Pitch, -1.3, 1.3);
            var x = Distance * System.Math.Cos(cp) * System.Math.Cos(Yaw);
            var y = Distance * System.Math.Sin(cp);
            var z = Distance * System.Math.Cos(cp) * System.Math.Sin(Yaw);
            var eye = new Vector3((float)(Target.X + x), (float)(Target.Y + y), (float)(Target.Z + z));
            return Matrix4.LookAt(eye, new Vector3((float)Target.X, (float)Target.Y, (float)Target.Z), Vector3.UnitY);
        }

        public Matrix4 GetProjectionMatrix(float aspect, float fovDeg = 50f, float near = 0.05f, float far = 200f)
            => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovDeg), aspect, near, far);
    }
}
