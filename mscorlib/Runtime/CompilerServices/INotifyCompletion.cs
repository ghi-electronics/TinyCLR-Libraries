namespace System.Runtime.CompilerServices {
    public interface INotifyCompletion {
        void OnCompleted(Action continuation);
    }

    public interface ICriticalNotifyCompletion : INotifyCompletion {
        void UnsafeOnCompleted(Action continuation);
    }
}
