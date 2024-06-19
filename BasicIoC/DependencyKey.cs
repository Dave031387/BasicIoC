namespace BasicIoC
{
    using System;

    internal class DependencyKey : IEquatable<DependencyKey>
    {
        internal Type DependencyType
        {
            get; set;
        } = typeof(object);

        internal int ResolvingKey
        {
            get; set;
        } = 0;

        public bool Equals(DependencyKey? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return DependencyType.GetType() == other.DependencyType.GetType() && ResolvingKey == other.ResolvingKey;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not null)
            {
                if (obj is DependencyKey key)
                {
                    return Equals(key);
                }
            }

            return false;
        }

        public override int GetHashCode() => HashCode.Combine(DependencyType, ResolvingKey);
    }
}