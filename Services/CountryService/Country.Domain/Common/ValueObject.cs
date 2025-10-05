using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Country.Domain.Common
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable GetEqualityComponents();     
        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var valueObject = (ValueObject)obj;
             return GetEqualityComponents().Cast<object>().SequenceEqual(valueObject.GetEqualityComponents().Cast<object>());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Cast<object>()
                .Select(x => x?.GetHashCode() ?? 0)
                .Aggregate((x, y) => x ^ y);
        }
    }
}
