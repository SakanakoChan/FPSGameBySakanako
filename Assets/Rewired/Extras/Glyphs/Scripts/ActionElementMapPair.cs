// Copyright (c) 2025 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

namespace Rewired.Glyphs {
    using System;

    public struct ActionElementMapPair : IEquatable<ActionElementMapPair> {

        public ActionElementMap a;
        public ActionElementMap b;

        public int Count {
            get {
                return (a != null ? 1 : 0) + (b != null ? 1 : 0);
            }
        }

        public ActionElementMapPair(
            ActionElementMap a,
            ActionElementMap b
        ) {
            this.a = a;
            this.b = b;
        }

        public bool Equals(ActionElementMapPair other) {
            return this.a == other.a &&
                this.b == other.b;
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is ActionElementMapPair)) return false;
            return this.Equals((ActionElementMapPair)obj);
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 29 + a.GetHashCode();
            hash = hash * 29 + b.GetHashCode();
            return hash;
        }

        public static bool operator ==(ActionElementMapPair a, ActionElementMapPair b) {
            return a.a == b.a &&
                a.b == b.b;
        }

        public static bool operator !=(ActionElementMapPair a, ActionElementMapPair b) {
            return !(a == b);
        }

        public override string ToString() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            return sb.Append("a: ").Append(a).AppendLine().Append("b: ").Append(b).ToString();
        }
    }
}
