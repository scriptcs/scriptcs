using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using ScriptCs.Contracts;

namespace ScriptCs.Tests
{
    public class ScriptCsMoqCustomization : AutoMoqCustomization, ICustomization
    {
        void ICustomization.Customize(IFixture fixture)
        {
            this.Customize(fixture);

            fixture.Register<Mock<IFileSystem>>(() =>
                {
                    var fileSystem = new Mock<IFileSystem>();
                    fileSystem.SetupGet(f => f.PackagesFile).Returns("packages.config");
                    fileSystem.SetupGet(f => f.PackagesFolder).Returns("packages");
                    fileSystem.SetupGet(f => f.BinFolder).Returns("bin");
                    fileSystem.SetupGet(f => f.DllCacheFolder).Returns(".cache");
                    return fileSystem;
                });
        }
    }
}
