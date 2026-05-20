namespace _Project.Code.Core.Services
{
    public interface IMonoBehaviourService : IService
    {
        void RegisterSelf();
        void ReplaceService();
        void UnregisterSelf();
    }
}
