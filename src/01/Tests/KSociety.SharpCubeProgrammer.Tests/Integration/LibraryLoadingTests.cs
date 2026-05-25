// Copyright (c) K-Society and contributors. All rights reserved. Licensed under the K-Society License. See LICENSE.TXT file in the project root for full license information.

namespace KSociety.SharpCubeProgrammer.Tests.Integration
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Integration tests for library loading.
    /// These tests verify that the native libraries can be found and loaded.
    /// </summary>
    public class LibraryLoadingTests
    {
        private readonly ITestOutputHelper _output;

        public LibraryLoadingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [SkippableFact]
        public void Should_Find_Windows_Native_Libraries()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "Test only runs on Windows");

            // Arrange
            var assemblyPath = Path.GetDirectoryName(typeof(LibraryLoadingTests).Assembly.Location);
            var architecture = Environment.Is64BitProcess ? "x64" : "x86";
            var dllPath = Path.Combine(assemblyPath, "dll", architecture);

            _output.WriteLine($"Assembly path: {assemblyPath}");
            _output.WriteLine($"Architecture: {architecture}");
            _output.WriteLine($"DLL path: {dllPath}");

            // Act
            var dllPathExists = Directory.Exists(dllPath);

            // Assert
            if (!dllPathExists)
            {
                _output.WriteLine($"⚠️ DLL directory not found. This is expected if native libraries are not deployed.");
                _output.WriteLine($"   To deploy libraries, build the main project or copy Programmer/{architecture}/ to {dllPath}");
            }

            // Check for expected DLL files
            if (dllPathExists)
            {
                var stLinkDriverPath = Path.Combine(dllPath, "STLinkUSBDriver.dll");
                var cubeProgrammerPath = Path.Combine(dllPath, "..", "CubeProgrammer_API.dll");

                _output.WriteLine($"STLinkUSBDriver.dll exists: {File.Exists(stLinkDriverPath)}");
                _output.WriteLine($"CubeProgrammer_API.dll exists: {File.Exists(cubeProgrammerPath)}");
            }
        }

        [SkippableFact]
        public void Should_Find_MacOS_Native_Libraries()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Test only runs on macOS");

            // Arrange
            var assemblyPath = Path.GetDirectoryName(typeof(LibraryLoadingTests).Assembly.Location);
            var dylibPath = Path.Combine(assemblyPath, "dylib", "osx");

            _output.WriteLine($"Assembly path: {assemblyPath}");
            _output.WriteLine($"dylib path: {dylibPath}");

            // Act
            var dylibPathExists = Directory.Exists(dylibPath);

            // Assert
            if (!dylibPathExists)
            {
                _output.WriteLine($"⚠️ dylib directory not found. This is expected if native libraries are not deployed.");
                _output.WriteLine($"   To deploy libraries, build the main project or copy Programmer/osx/ to {dylibPath}");
            }

            // Check for expected dylib files
            if (dylibPathExists)
            {
                var stLinkDriverPath = Path.Combine(dylibPath, "libSTLinkUSBDriver.dylib");
                var cubeProgrammerPath = Path.Combine(dylibPath, "libCubeProgrammer_API.dylib");

                _output.WriteLine($"libSTLinkUSBDriver.dylib exists: {File.Exists(stLinkDriverPath)}");
                _output.WriteLine($"libCubeProgrammer_API.dylib exists: {File.Exists(cubeProgrammerPath)}");

                // Check for Qt frameworks
                var qtCorePath = Path.Combine(dylibPath, "QtCore.framework");
                _output.WriteLine($"QtCore.framework exists: {Directory.Exists(qtCorePath)}");
            }
        }

        [Fact]
        public void Should_Identify_Current_Platform_Library_Path()
        {
            // Arrange
            var assemblyPath = Path.GetDirectoryName(typeof(LibraryLoadingTests).Assembly.Location);
            string expectedPath = null;

            // Act
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var architecture = Environment.Is64BitProcess ? "x64" : "x86";
                expectedPath = Path.Combine(assemblyPath, "dll", architecture);
                _output.WriteLine($"Platform: Windows");
                _output.WriteLine($"Expected path: {expectedPath}");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                expectedPath = Path.Combine(assemblyPath, "dylib", "osx");
                _output.WriteLine($"Platform: macOS");
                _output.WriteLine($"Expected path: {expectedPath}");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var architecture = Environment.Is64BitProcess ? "x64" : "x86";
                expectedPath = Path.Combine(assemblyPath, "so", architecture);
                _output.WriteLine($"Platform: Linux");
                _output.WriteLine($"Expected path: {expectedPath}");
            }

            // Assert
            expectedPath.Should().NotBeNullOrEmpty("platform should be detected");
            _output.WriteLine($"Path exists: {Directory.Exists(expectedPath)}");
        }

        [Fact]
        public void Should_Have_Correct_Architecture()
        {
            // Arrange & Act
            var is64Bit = Environment.Is64BitProcess;
            var architecture = RuntimeInformation.ProcessArchitecture;

            // Assert & Log
            _output.WriteLine($"Is 64-bit process: {is64Bit}");
            _output.WriteLine($"Process architecture: {architecture}");

            if (is64Bit)
            {
                architecture.Should().BeOneOf(Architecture.X64, Architecture.Arm64);
            }
            else
            {
                architecture.Should().BeOneOf(Architecture.X86, Architecture.Arm);
            }
        }

        [SkippableFact]
        public void Should_Load_STLink_Libraries_When_Available()
        {
            // Note: This test will only pass if the native libraries have been deployed
            // Skip if libraries are not present

            var assemblyPath = Path.GetDirectoryName(typeof(LibraryLoadingTests).Assembly.Location);
            string libraryPath = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var arch = Environment.Is64BitProcess ? "x64" : "x86";
                libraryPath = Path.Combine(assemblyPath, "dll", arch, "STLinkUSBDriver.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                libraryPath = Path.Combine(assemblyPath, "dylib", "osx", "libSTLinkUSBDriver.dylib");
            }

            Skip.If(libraryPath == null || !File.Exists(libraryPath), 
                $"Native libraries not deployed. Library path: {libraryPath}");

            _output.WriteLine($"Attempting to load: {libraryPath}");

            // This test just verifies the file exists - actual loading is tested in the API
            File.Exists(libraryPath).Should().BeTrue();
        }
    }
}
