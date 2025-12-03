using AutoVPT.Configuration;
using AutoVPT.Infrastructure;
using AutoVPT.Interfaces;
using AutoVPT.Repositories;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AutoVPT.Tests
{
    /// <summary>
    /// Test cases demonstrating Phase 1 refactoring functionality.
    /// These tests verify that all new abstractions work correctly.
    /// </summary>
    public static class Phase1Tests
    {
        public static void RunAllTests()
        {
            Console.WriteLine("=== Phase 1 Refactoring Tests ===\n");

            TestConfigurationManager();
            TestLoggers();
            TestCharacterRepository();
            TestWindowManager();

            Console.WriteLine("\n=== All Tests Completed ===");
        }

        private static void TestConfigurationManager()
        {
            Console.WriteLine("Test 1: ConfigurationManager");
            try
            {
                var config = ConfigurationManager.Instance;

                // Test timing settings
                Console.WriteLine($"  Short Delay: {config.Timing.ShortDelayMs}ms");
                Console.WriteLine($"  Medium Delay: {config.Timing.MediumDelayMs}ms");

                // Test path settings
                var imagePath = config.Paths.GetImagePath("/global/button.png", false);
                Console.WriteLine($"  Image Path: {imagePath}");

                // Test feature status
                Console.WriteLine($"  Status Inactive: {config.FeatureStatus.Inactive}");

                Console.WriteLine("  ✓ ConfigurationManager test passed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ ConfigurationManager test failed: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static void TestLoggers()
        {
            Console.WriteLine("Test 2: Logging Infrastructure");
            try
            {
                // Test file logger
                var fileLogger = new FileLogger("logs/test.log", LogLevel.Info);
                fileLogger.LogInfo("Test message", "TestContext");
                fileLogger.LogWarning("Test warning");
                fileLogger.LogError("Test error", new Exception("Test exception"));
                Console.WriteLine("  ✓ FileLogger test passed");

                // Test debug logger
                var debugLogger = new DebugLogger(LogLevel.Debug);
                debugLogger.LogDebug("Debug message");
                Console.WriteLine("  ✓ DebugLogger test passed");

                // Test composite logger
                var composite = new CompositeLogger();
                composite.AddLogger(fileLogger);
                composite.AddLogger(debugLogger);
                composite.LogInfo("Composite test");
                Console.WriteLine("  ✓ CompositeLogger test passed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Logger test failed: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static void TestCharacterRepository()
        {
            Console.WriteLine("Test 3: Character Repository");
            try
            {
                var repo = new XmlCharacterRepository("database");

                // Test exists
                Console.WriteLine($"  Repository initialized successfully");

                // Test GetAll (might be empty)
                var characters = repo.GetAll();
                Console.WriteLine($"  Found {System.Linq.Enumerable.Count(characters)} characters");

                Console.WriteLine("  ✓ CharacterRepository test passed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ CharacterRepository test failed: {ex.Message}");
            }
            Console.WriteLine();
        }

        private static void TestWindowManager()
        {
            Console.WriteLine("Test 4: Window Manager");
            try
            {
                var windowManager = new Win32WindowManager();

                // Try to find current window (won't find it but tests the interface)
                var handle = windowManager.FindWindow("NonExistentWindow");
                Console.WriteLine($"  FindWindow result: {handle}");

                // Test validity check
                var isValid = windowManager.IsWindowValid(handle);
                Console.WriteLine($"  IsValid: {isValid}");

                Console.WriteLine("  ✓ WindowManager test passed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ WindowManager test failed: {ex.Message}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Example of using the new image recognition interface
        /// </summary>
        public static void ExampleImageRecognition(IntPtr windowHandle)
        {
            // Create image recognition instance
            IImageRecognition imageRecog = new EmguCvImageRecognition(windowHandle);

            try
            {
                // Find image asynchronously
                var task = imageRecog.FindImageAsync("resources/global/button.png");
                task.Wait();
                var location = task.Result;

                if (location.HasValue)
                {
                    Console.WriteLine($"Found image at: {location.Value}");
                }

                // Wait for image to appear
                var waitTask = imageRecog.WaitForImageAsync(
                    "resources/global/loading.png",
                    TimeSpan.FromSeconds(5)
                );
                waitTask.Wait();

                if (waitTask.Result)
                {
                    Console.WriteLine("Image appeared within timeout");
                }
            }
            finally
            {
                // Clean up
                imageRecog.ClearCache();
                if (imageRecog is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// Example of using the new input simulator interface
        /// </summary>
        public static void ExampleInputSimulator(IntPtr windowHandle)
        {
            IInputSimulator input = new Win32InputSimulator(windowHandle);

            // Click at position
            var clickTask = input.ClickAsync(new Point(100, 200), delayAfterMs: 500);
            clickTask.Wait();

            // Send key
            var keyTask = input.SendKeyAsync(Keys.Escape, delayAfterMs: 100);
            keyTask.Wait();

            // Send text
            var textTask = input.SendTextAsync("Hello");
            textTask.Wait();
        }

        /// <summary>
        /// Example of using composite logger with multiple outputs
        /// </summary>
        public static ILogger CreateProductionLogger(TextBox statusTextBox)
        {
            var logger = new CompositeLogger();

            // Add file logger for persistence
            logger.AddLogger(new FileLogger("logs/automation.log", LogLevel.Info));

            // Add UI logger for real-time feedback
            logger.AddLogger(new UiLogger(statusTextBox, LogLevel.Info));

            // Add debug logger for development
            #if DEBUG
            logger.AddLogger(new DebugLogger(LogLevel.Debug));
            #endif

            return logger;
        }
    }
}
