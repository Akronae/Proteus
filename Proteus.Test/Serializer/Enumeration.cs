using FluentAssertions;
using Xunit;

namespace Proteus.Test.Serializer
{
    public class Enumeration
    {
        public enum EnumData
        {
            a,
            b,
            c,
            d,
            e,
            f,
            g,
            h,
            i,
            j,
            k,
            l,
            m,
            n,
            o,
            p,
            q,
            r,
            s,
            t,
            u,
            v,
            w,
            x,
            y,
            z,
            aa,
            ba,
            ca,
            da,
            ea,
            fa,
            ga,
            ha,
            ia,
            ka,
            la,
            ma,
            na,
            oa,
            pa,
            qa,
            ra,
            ta,
            ua,
            va,
            wa,
            xa,
            ya,
            za,
            ab = int.MaxValue,
            bb = int.MinValue,
            cb = -1
        }

        [Theory]
        [InlineData(EnumData.a)]
        [InlineData(EnumData.za)]
        [InlineData(EnumData.v)]
        [InlineData(EnumData.ab)]
        [InlineData(EnumData.bb)]
        [InlineData(EnumData.cb)]
        public void Enum (EnumData value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<EnumData>(serialized);

            deserialized.Should().Be(value);
        }
    }
}