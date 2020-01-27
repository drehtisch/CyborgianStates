using CyborgianStates;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CyboargianStates.Test
{
    public class MiscTests
    {
        [Fact]
        public void TestMain()
        {
            Mock<ILauncher> mock = new Mock<ILauncher>();
            mock.SetupGet(l => l.IsRunning).Returns(true);
            ILauncher launcher = mock.Object;
            Program.SetLauncher(launcher);
            Program.Main();
            mock.Verify(l => l.Run(), Times.Once);
            Assert.True(launcher.IsRunning);
        }
        [Fact]
        public void TestLauncher()
        {
            ILauncher launcher = new Launcher();
            launcher.Run();
            Assert.True(launcher.IsRunning);
        }
    }
}
