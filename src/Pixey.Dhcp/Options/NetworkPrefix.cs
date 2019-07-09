using System.Net;

namespace Pixey.Dhcp.Options
{
    public class NetworkPrefix
    {
        public NetworkPrefix(IPAddress prefix, int length)
        {
            Prefix = prefix;
            Length = length;
        }

        public IPAddress Prefix { get; }

        public int Length { get; }

        protected bool Equals(NetworkPrefix other)
        {
            return Equals(Prefix, other.Prefix) && Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NetworkPrefix)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Prefix != null ? Prefix.GetHashCode() : 0) * 397) ^ Length;
            }
        }
    }
}