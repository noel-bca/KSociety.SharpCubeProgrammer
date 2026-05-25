// Copyright (c) K-Society and contributors. All rights reserved. Licensed under the K-Society License. See LICENSE.TXT file in the project root for full license information.

namespace KSociety.SharpCubeProgrammer.Tests.Native
{
    using System;
    using System.Runtime.InteropServices;
    using FluentAssertions;
    using global::SharpCubeProgrammer.Native;
    using Xunit;

    /// <summary>
    /// Tests for SafeLibraryHandle.
    /// </summary>
    public class SafeLibraryHandleTests
    {
        [SkippableFact]
        public void Windows_Handle_Should_Be_Released_Properly()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "Test only runs on Windows");

            // Arrange & Act
            SafeLibraryHandle handle = null;
            bool disposed = false;

            try
            {
                handle = Utility.LoadLibraryEx("kernel32.dll", IntPtr.Zero, 0);
                handle.Should().NotBeNull();
                handle.IsInvalid.Should().BeFalse();

                // Dispose
                handle.Dispose();
                disposed = true;

                // Assert
                handle.IsInvalid.Should().BeTrue("handle should be invalid after dispose");
            }
            catch
            {
                if (!disposed)
                {
                    handle?.Dispose();
                }
                throw;
            }
        }

        [SkippableFact]
        public void MacOS_Handle_Should_Be_Released_Properly()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Test only runs on macOS");

            // Arrange
            IntPtr rawHandle = Utility.dlopen("/usr/lib/libSystem.dylib", Utility.RTLD_NOW);
            rawHandle.Should().NotBe(IntPtr.Zero, "library should load");

            // Act - Create SafeLibraryHandle and set the handle
            var handle = new SafeLibraryHandle();
            typeof(SafeLibraryHandle)
                .GetField("handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(handle, rawHandle);

            // Assert before dispose
            handle.IsInvalid.Should().BeFalse("handle should be valid before dispose");

            // Dispose
            handle.Dispose();

            // Assert after dispose
            handle.IsInvalid.Should().BeTrue("handle should be invalid after dispose");
        }

        [Fact]
        public void Handle_Should_Not_Allow_Double_Dispose()
        {
            // Arrange
            SafeLibraryHandle handle = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                handle = Utility.LoadLibraryEx("kernel32.dll", IntPtr.Zero, 0);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var rawHandle = Utility.dlopen("/usr/lib/libSystem.dylib", Utility.RTLD_NOW);
                handle = new SafeLibraryHandle();
                typeof(SafeLibraryHandle)
                    .GetField("handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(handle, rawHandle);
            }
            else
            {
                return; // Skip test on unsupported platforms
            }

            // Act & Assert
            handle.Should().NotBeNull();
            handle.Dispose();

            // Should not throw on second dispose
            Action secondDispose = () => handle.Dispose();
            secondDispose.Should().NotThrow("SafeHandle should allow multiple dispose calls");
        }

        [Fact]
        public void Handle_Should_Support_Using_Statement()
        {
            // Arrange & Act
            Action testUsing = () =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (var handle = Utility.LoadLibraryEx("kernel32.dll", IntPtr.Zero, 0))
                    {
                        handle.Should().NotBeNull();
                        handle.IsInvalid.Should().BeFalse();
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    var rawHandle = Utility.dlopen("/usr/lib/libSystem.dylib", Utility.RTLD_NOW);
                    using (var handle = new SafeLibraryHandle())
                    {
                        typeof(SafeLibraryHandle)
                            .GetField("handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.SetValue(handle, rawHandle);
                        handle.IsInvalid.Should().BeFalse();
                    }
                }
            };

            // Assert
            testUsing.Should().NotThrow("using statement should work correctly");
        }
    }
}
