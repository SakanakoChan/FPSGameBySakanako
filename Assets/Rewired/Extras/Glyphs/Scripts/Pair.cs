// Copyright (c) 2025 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

namespace Rewired.Glyphs {
    using System;
    using System.Collections.Generic;

    public struct Pair<T> : IEquatable<Pair<T>> {
        public T a;
        public T b;

        public Pair(T a, T b) {
            this.a = a;
            this.b = b;
        }

        public bool Equals(Pair<T> other) {
            return EqualityComparer<T>.Default.Equals(this.a, other.a) &&
                EqualityComparer<T>.Default.Equals(this.b, other.b);
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is Pair<T>)) return false;
            return this.Equals((Pair<T>)obj);
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 29 + a.GetHashCode();
            hash = hash * 29 + b.GetHashCode();
            return hash;
        }

        public static bool operator ==(Pair<T> a, Pair<T> b) {
            return EqualityComparer<T>.Default.Equals(a.a, b.a) &&
                EqualityComparer<T>.Default.Equals(a.b, b.b);
        }

        public static bool operator !=(Pair<T> a, Pair<T> b) {
            return !(a == b);
        }

        public override string ToString() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            return sb.Append("a: ").Append(a).AppendLine().Append("b: ").Append(b).ToString();
        }
    }
}
