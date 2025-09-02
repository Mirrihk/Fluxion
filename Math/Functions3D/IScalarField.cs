namespace Fluxion.Math.Functions3D
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
        private readonly System.Func<double, double, double> f;
        public DelegateScalarField(System.Func<double, double, double> f) => this.f = f;
        public double Evaluate(double x, double y) => f(x, y);

    }
   

}