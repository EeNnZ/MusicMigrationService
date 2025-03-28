namespace MusicMigrationService.Client.Common;

public abstract class OneOf<TValue, TUError>
{
    private OneOf() { }
    public abstract void Match(Action<TValue> first, Action<TUError> second);
    public abstract TResult Match<TResult>(Func<TValue, TResult> first, Func<TUError, TResult> second);
 
    public sealed class First(TValue value) : OneOf<TValue, TUError>
    {
        public override void Match(Action<TValue> first, Action<TUError> second) => first(value);
        public override TResult Match<TResult>(Func<TValue, TResult> first, Func<TUError, TResult> second) => first(value);
    }
    
    public sealed class Second(TUError value) : OneOf<TValue, TUError>
    {
        public override void Match(Action<TValue> first, Action<TUError> second) => second(value);
        public override TResult Match<TResult>(Func<TValue, TResult> first, Func<TUError, TResult> second) => second(value);
    }
}