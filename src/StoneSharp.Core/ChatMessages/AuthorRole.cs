using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.ChatMessages
{
    public readonly struct AuthorRole : IEquatable<AuthorRole>
    {
        public static AuthorRole Developer { get; } = new("developer");

        public static AuthorRole System { get; } = new("system");

        public static AuthorRole Assistant { get; } = new("assistant");

        public static AuthorRole User { get; } = new("user");

        public static AuthorRole Tool { get; } = new("tool");

        public string Label { get; }

        public AuthorRole(string label)
        {
            Label = label!;
        }

        public static bool operator ==(AuthorRole left, AuthorRole right)
            => left.Equals(right);

        public static bool operator !=(AuthorRole left, AuthorRole right)
            => !(left == right);

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is AuthorRole otherRole && this == otherRole;

        /// <inheritdoc/>
        public bool Equals(AuthorRole other)
            => string.Equals(Label, other.Label, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public override int GetHashCode()
            => StringComparer.OrdinalIgnoreCase.GetHashCode(Label);

        /// <inheritdoc/>
        public override string ToString() => Label;
    }
}
