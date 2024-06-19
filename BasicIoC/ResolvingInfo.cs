namespace BasicIoC
{
    using System;

    internal class ResolvingInfo
    {
        internal ResolvingMethod ResolvingMethod
        {
            get; set;
        } = ResolvingMethod.None;

        internal Type ResolvingType
        {
            get; set;
        } = typeof(object);

        internal object? SingletonInstance
        {
            get; set;
        } = null;
    }
}