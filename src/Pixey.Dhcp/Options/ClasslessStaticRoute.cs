using System.Net;

namespace Pixey.Dhcp.Options
{
    public class ClasslessStaticRoute
    {
        public ClasslessStaticRoute(NetworkPrefix prefix, IPAddress nextHop)
        {
            Prefix = prefix;
            NextHop = nextHop;
        }

        public NetworkPrefix Prefix { get; }

        public IPAddress NextHop { get; }

        protected bool Equals(ClasslessStaticRoute other)
        {
            return Equals(Prefix, other.Prefix) && Equals(NextHop, other.NextHop);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((ClasslessStaticRoute)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Prefix != null ? Prefix.GetHashCode() : 0) * 397) ^ (NextHop != null ? NextHop.GetHashCode() : 0);
            }
        }
    };
}