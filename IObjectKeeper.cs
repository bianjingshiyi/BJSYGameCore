namespace BJSYGameCore
{
    public interface IObjectKeeper<T> where T : UnityEngine.Object
    {
        T getObjectById(int id);
        int getIdOfObject(T obj);
    }
}