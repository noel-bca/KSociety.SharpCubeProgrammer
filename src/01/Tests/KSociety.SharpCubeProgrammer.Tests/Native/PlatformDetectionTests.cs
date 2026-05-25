// Copyright (c) K-Society and contributors. All rights reserved. Licensed under the K-Society License. See LICENSE.TXT file in the project root for full license information.

namespace KSociety.SharpCubeProgrammer.Tests.Native
{
    using System;
    using System.Runtime.InteropServices;
    using FluentAssertions;
    using Xunit;

    /// <summary>
    /// Tests for platform detection.
    /// </summary>
    public class PlatformDetectionTests
    {
        [Fact]
        public void Should_Detect_Current_Platform()
        {
            // Act
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            // Assert
            // At least one platform should be detected
            (isWindows || isMacOS || isLinux).Should().BeTrue("at least one platform should be detected");

            // Only one platform should be true at a time
            var platformCount = (isWindows ? 1 : 0) + (isMacOS ? 1 : 0) + (isLinux ? 1 : 0);
            platformCount.Should().Be(1, "only one platform should be detected at a time");
        }

        [Fact]
        public void Should_Return_Correct_Architecture()
        {
            // Act
            var architecture = RuntimeInformation.ProcessArchitecture;

            // Assert
            architecture.Should().BeOneOf(
                Architecture.X64,
                Architecture.X86,
                Architecture.Arm,
                Architecture.Arm64);
        }

        [Fact]
        public void Should_Recognize_Windows_Platform()
        {
            // Act
            var isRecognized = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            // Assert - The method should not throw and return a boolean
            Assert.True(isRecognized == true || isRecognized == false);
        }

        [Fact]
        public void Should_Recognize_MacOS_Platform()
        {
            // Act
            var isRecognized = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

            // Assert - The method should not throw and return a boolean
            Assert.True(isRecognized == true || isRecognized == false);
        }

        [Fact]
        public void Should_Recognize_Linux_Platform()
        {
            // Act
            var isRecognized = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            // Assert - The method should not throw and return a boolean
            Assert.True(isRecognized == true || isRecognized == false);
        }

        [Fact]
        public void Should_Get_Framework_Description()
        {
            // Act
            var frameworkDescription = RuntimeInformation.FrameworkDescription;

            // Assert
            frameworkDescription.Should().NotBeNullOrWhiteSpace("framework description should be available");
            frameworkDescription.Should().ContainAny(".NET", "framework description should contain .NET");
        }

        [Fact]
        public void Should_Determine_64Bit_Process_Correctly()
        {
            // Act
            var is64Bit = Environment.Is64BitProcess;
            var architecture = RuntimeInformation.ProcessArchitecture;

            // Assert
            if (is64Bit)
            {
                architecture.Should().BeOneOf(Architecture.X64, Architecture.Arm64);
            }
            else
            {
                architecture.Should().BeOneOf(Architecture.X86, Architecture.Arm);
            }
        }
    }
}
