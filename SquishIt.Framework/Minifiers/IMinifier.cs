namespace SquishIt.Framework.Minifiers
{
    public interface IMinifier<T> : IMinify where T : Base.BundleBase<T>
    {
    }
}