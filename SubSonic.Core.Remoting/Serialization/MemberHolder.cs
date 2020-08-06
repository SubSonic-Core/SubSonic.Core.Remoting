using System;
using System.Runtime.Serialization;

namespace SubSonic.Core.Remoting.Serialization
{
    internal class MemberHolder
    {
        private readonly Type memberType;
        private readonly StreamingContext context;

        internal MemberHolder(Type type, StreamingContext ctx)
        {
            this.memberType = type;
            this.context = ctx;
        }

        public Type MemberType => memberType;

        public override bool Equals(object obj)
        {
            return ((obj is MemberHolder holder) && (object.ReferenceEquals(holder.memberType, this.memberType) && (holder.context.State == this.context.State)));
        }

        public override int GetHashCode()
        {
            return this.memberType.GetHashCode();
        }
    }
}
