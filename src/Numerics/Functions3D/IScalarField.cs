namespace Fluxion.src.Numerics.Functions3D
{
   /*
    * <summary>
      Represents a Scalar field z= f(x,y)
    * </summary>
    */

    public interface IScalarField
    {
        double Evaluate(double x, double y);
    }
    public sealed class DelegateScalarField : IScalarField
    {
        private readonly Func<double, double, double> f;
        public DelegateScalarField(Func<double, double, double> f) => this.f = f;
        public double Evaluate(double x, double y) => f(x, y);

    }
   

}