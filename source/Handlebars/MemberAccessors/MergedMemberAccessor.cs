using HandlebarsDotNet.PathStructure;

namespace HandlebarsDotNet.MemberAccessors
{
    public sealed class MergedMemberAccessor : IMemberAccessor
    {
        private readonly IMemberAccessor[] _accessors;

        public MergedMemberAccessor(params IMemberAccessor[] accessors)
        {
            _accessors = accessors;
        }

        public bool TryGetValue(object instance, ChainSegment memberName, out object value)
        {
            for (var index = 0; index < _accessors.Length; index++)
            {
                if (_accessors[index].TryGetValue(instance, memberName, out value)) return true;
            }

            value = default;
            return false;
        }
    }
}