namespace ProjectBloodbath.Prototype
{
    public interface IPrototypeModalView
    {
        bool IsOpen { get; }

        void CloseFromCoordinator();
    }
}
