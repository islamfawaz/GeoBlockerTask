using Country.Domain.Common;
using Country.Domain.Exceptions;
using System.Collections;
using System.Text.RegularExpressions;

namespace Country.Domain.ValueObjects
{

    public sealed class CountryCode : ValueObject
    {
        public string Value { get; private set; }

        private CountryCode(string value)
        {
            Value = value;
        }

        public static CountryCode Create(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("Country code cannot be empty");

            code = code.Trim().ToUpperInvariant();

            if (!Regex.IsMatch(code, @"^[A-Z]{2}$"))
                throw new DomainException("Country code must be 2 uppercase letters");

            return new CountryCode(code);
        }

        protected override IEnumerable GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
        public static implicit operator string(CountryCode countryCode) => countryCode.Value;
    }
}
