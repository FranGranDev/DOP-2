namespace Game.Services
{
    public interface IBindable<T>
    {
        public void Bind(T obj);
    }
}
