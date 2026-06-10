namespace _Project.Code.Gameplay.Combat
{
    public interface ITargetable
    {
        bool IsTargetable { get; }
        float Priority { get; }
    }
}