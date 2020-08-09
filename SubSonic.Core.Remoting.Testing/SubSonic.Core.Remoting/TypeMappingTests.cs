using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting
{
    [TestFixture]
    public class TypeMappingTests
    {

        [Test]
        public void CanMapUriTypes()
        {
            Type uriType = typeof(Uri);

            string framworkDiff = $"{uriType.FullName}, System.Unknown";

            Type mapType = framworkDiff.ToType();

            mapType.Should().NotBeNull();

            uriType.Should().BeAssignableTo(mapType);
        }

        [Test]
        public async Task CanMapUriTypesAsync()
        {
            Type uriType = typeof(Uri);

            string framworkDiff = $"{uriType.FullName}, System.Unknown";

            Type mapType = await framworkDiff.ToTypeAsync();

            mapType.Should().NotBeNull();

            uriType.Should().BeAssignableTo(mapType);
        }

        [Test]
        public void CanMapUriArrayTypes()
        {
            Type uriType = typeof(Uri[]);

            string framworkDiff = $"{uriType.FullName}, System.Unknown";

            Type mapType = framworkDiff.ToType();

            mapType.Should().NotBeNull();

            uriType.Should().BeAssignableTo(mapType);
        }

        [Test]
        public async Task CanMapUriArrayTypesAsync()
        {
            Type uriType = typeof(Uri[]);

            string framworkDiff = $"{uriType.FullName}, System.Unknown";

            Type mapType = await framworkDiff.ToTypeAsync();

            mapType.Should().NotBeNull();

            uriType.Should().BeAssignableTo(mapType);
        }
    }
}