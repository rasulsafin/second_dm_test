using System;
using Newtonsoft.Json;

namespace Brio.Docs.Client
{
    public struct ID<T> : IEquatable<ID<T>>
    {
        [JsonProperty]
        private readonly int id;

        public ID(int id) => this.id = id;

        public static ID<T> InvalidID => new ID<T>(-1);

        [JsonIgnore]
        public bool IsValid => id > 0;

        public static explicit operator int(ID<T> ident) => ident.id;

        public static explicit operator ID<T>(int id) => new ID<T>(id);

        public static bool operator !=(ID<T> lhs, ID<T> rhs) => !lhs.Equals(rhs);

        public static bool operator ==(ID<T> lhs, ID<T> rhs) => lhs.Equals(rhs);

        public bool Equals(ID<T> other) => this.id == other.id;

        public override bool Equals(object obj)
        {
            if (!(obj is ID<T> other))
                return false;
            return this.Equals(other);
        }

        public override int GetHashCode() => id.GetHashCode();

        public override string ToString() => $"{id}";
    }
}
