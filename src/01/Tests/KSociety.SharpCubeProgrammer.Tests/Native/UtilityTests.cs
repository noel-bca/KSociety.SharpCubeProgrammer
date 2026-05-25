// Copyright (c) K-Society and contributors. All rights reserved. Licensed under the K-Society License. See LICENSE.TXT file in the project root for full license information.

namespace KSociety.SharpCubeProgrammer.Tests.Native
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using FluentAssertions;
    using global::SharpCubeProgrammer.Native;
    using Xunit;

    /// <summary>
    /// Tests for native library utilities.
    /// </summary>
    public class UtilityTests
    {
        [Fact]
        public void Constants_Should_Have_Valid_Values()
        {
            // Assert - Windows constants
            Utility.LOAD_WITH_ALTERED_SEARCH_PATH.Should().Be(0x00000008);
            Utility.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS.Should().Be(0x00001000);

            // Assert - dlopen constants
            Utility.RTLD_LAZY.Should().Be(0x00001);
            Utility.RTLD_NOW.Should().Be(0x00002);

            // Assert - library names
            Utility.KernelLibName.Should().Be("kernel32.dll");
            Utility.LibDlName.Should().Be("libdl");
        }

        [SkippableFact]
        public void Windows_LoadLibraryEx_Should_Load_System_Library()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "Test only runs on Windows");

            // Arrange - Use a system library that's always present
            var libraryPath = "kernel32.dll";

            // Act
            SafeLibraryHandle handle = null;
            try
            {
                handle = Utility.LoadLibraryEx(libraryPath, IntPtr.Zero, 0);

                // Assert
                handle.Should().NotBeNull("library should load successfully");
                handle.IsInvalid.Should().BeFalse("handle should be valid");
            }
            finally
            {
                handle?.Dispose();
            }
        }

        [SkippableFact]
        public void Windows_GetProcAddress_Should_Find_Known_Function()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "Test only runs on Windows");

            // Arrange
            var libraryPath = "kernel32.dll";
            SafeLibraryHandle handle = null;

            try
            {
                handle = Utility.LoadLibraryEx(libraryPath, IntPtr.Zero, 0);

                // Act
                var address = Utility.GetProcAddress(handle, "GetCurrentProcess");

                // Assert
                address.Should().NotBe(IntPtr.Zero, "GetCurrentProcess should be found in kernel32.dll");
            }
            finally
            {
                handle?.Dispose();
            }
        }

        [SkippableFact]
        public void MacOS_dlopen_Should_Load_System_Library()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Test only runs on macOS");

            // Arrange - Use libSystem which is always available on macOS
            var libraryPath = "/usr/lib/libSystem.dylib";

            // Act
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = Utility.dlopen(libraryPath, Utility.RTLD_NOW);

                // Assert
                handle.Should().NotBe(IntPtr.Zero, "library should load successfully");

                // Check for error
                if (handle == IntPtr.Zero)
                {
                    var errorPtr = Utility.dlerror();
                    var error = errorPtr != IntPtr.Zero ? Marshal.PtrToStringAnsi(errorPtr) : "Unknown error";
                    throw new Exception($"dlopen failed: {error}");
                }
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    Utility.dlclose(handle);
                }
            }
        }

        [SkippableFact]
        public void MacOS_dlsym_Should_Find_Known_Function()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Test only runs on macOS");

            // Arrange
            var libraryPath = "/usr/lib/libSystem.dylib";
            IntPtr handle = IntPtr.Zero;

            try
            {
                handle = Utility.dlopen(libraryPath, Utility.RTLD_NOW);
                handle.Should().NotBe(IntPtr.Zero, "library should load successfully");

                // Act
                var address = Utility.dlsym(handle, "printf");

                // Assert
                address.Should().NotBe(IntPtr.Zero, "printf should be found in libSystem");
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    Utility.dlclose(handle);
                }
            }
        }

        [SkippableFact]
        public void Linux_dlopen_Should_Load_System_Library()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Linux), "Test only runs on Linux");

            // Arrange - Use libc which is always available on Linux
            var libraryPath = "libc.so.6";

            // Act
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = Utility.dlopen(libraryPath, Utility.RTLD_NOW);

                // Assert
                handle.Should().NotBe(IntPtr.Zero, "library should load successfully");
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    Utility.dlclose(handle);
                }
            }
        }
    }
}
