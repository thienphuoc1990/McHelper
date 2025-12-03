// ============================================================================
// FLASH EXTERNALINTERFACE INTEGRATION EXAMPLE
// Copy this code into Form1.cs to add Flash Explorer functionality
// ============================================================================

using AutoVPT.Infrastructure;
using System;
using System.Windows.Forms;

namespace AutoVPT
{
    // Add this to your Form1 class
    public partial class Form1 : Form
    {
        // 1. ADD THIS FIELD to store the Flash reader
        private FlashGameReader _flashReader;

        // 2. ADD THIS METHOD to initialize Flash reader (call from Form1_Load or similar)
        private void InitializeFlashReader()
        {
            try
            {
                // TODO: Find your Flash control in Form1.Designer.cs
                // It's usually named something like:
                // - axShockwaveFlash1
                // - flashPlayer
                // - webBrowser1 (if using WebBrowser control)

                // Example if using AxShockwaveFlash:
                // var flashControl = this.axShockwaveFlash1;

                // Example if you need to get it from a character window:
                // You'll need to adjust this based on your actual Flash control location

                // For now, log that we're looking for it
                this.statusText.AppendText("Looking for Flash control for ExternalInterface integration...\n");
                this.statusText.AppendText("⚠️ You need to update FLASH_INTEGRATION_EXAMPLE.cs with your actual Flash control\n");

                // Uncomment and modify this once you find your Flash control:
                // _flashReader = new FlashGameReader(flashControl);
                //
                // if (_flashReader.IsAvailable)
                // {
                //     this.statusText.AppendText("✅ Flash ExternalInterface is AVAILABLE! 100x speedup enabled!\n");
                // }
                // else
                // {
                //     this.statusText.AppendText("⚠️ Flash ExternalInterface not available, using image recognition\n");
                // }
            }
            catch (Exception ex)
            {
                this.statusText.AppendText($"❌ Flash reader initialization error: {ex.Message}\n");
            }
        }

        // 3. ADD THIS BUTTON CLICK HANDLER to open Flash Explorer
        private void buttonFlashExplorer_Click(object sender, EventArgs e)
        {
            try
            {
                // Get Flash control (adjust this based on your code)
                // var flashControl = this.axShockwaveFlash1;

                // For testing, you can also get it from a character window if that's where Flash is
                if (dataGridViewCharacters.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a character first (need active Flash window)",
                                  "Flash Explorer",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                    return;
                }

                // TODO: Get the Flash control from the selected character's window
                // This depends on your architecture

                // Create Flash reader if not already created
                if (_flashReader == null)
                {
                    MessageBox.Show("Flash reader not initialized.\n\n" +
                                  "Please update FLASH_INTEGRATION_EXAMPLE.cs with your actual Flash control.\n\n" +
                                  "Look in Form1.Designer.cs for:\n" +
                                  "- axShockwaveFlash1\n" +
                                  "- flashPlayer\n" +
                                  "- Or find where flash.exe window is created",
                                  "Setup Required",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    return;
                }

                // Open Flash Explorer
                var explorer = new FlashExplorerForm(IntPtr.Zero);
                explorer.SetFlashReader(_flashReader);
                explorer.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Flash Explorer: {ex.Message}",
                              "Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        // 4. ADD THIS METHOD to pass Flash reader to executors
        private MainAuto CreateMainAutoWithFlashReader(Character character)
        {
            // Get window handle for character
            IntPtr hwnd = AutoControl.FindWindowHandle(character.ID);

            if (hwnd == IntPtr.Zero)
            {
                return null; // Window not found
            }

            // Create image recognition
            var imageRecognition = new EmguCvImageRecognition(hwnd);

            // Create input simulator
            var inputSimulator = new InputSimulator(hwnd);

            // Create logger
            var logger = new Logger(this.statusText);

            // Pass Flash reader to MainAuto
            // (You'll need to modify MainAuto constructor to accept FlashGameReader)
            var mainAuto = new MainAuto(character, hwnd, _flashReader);

            return mainAuto;
        }
    }

    // ============================================================================
    // EXAMPLE: How to add Flash Explorer button to your UI
    // ============================================================================
    //
    // In Form1.Designer.cs or using the Visual Studio designer:
    //
    // 1. Add a new button:
    //    - Name: buttonFlashExplorer
    //    - Text: "🔍 Flash Explorer (EXPERIMENTAL)"
    //    - Location: Near other debug/utility buttons
    //    - Size: 200x30
    //
    // 2. Double-click the button to create click handler
    //    (or manually add buttonFlashExplorer_Click method above)
    //
    // 3. The button should look like:
    /*
        this.buttonFlashExplorer = new System.Windows.Forms.Button();
        //
        // buttonFlashExplorer
        //
        this.buttonFlashExplorer.Location = new System.Drawing.Point(12, 500);
        this.buttonFlashExplorer.Name = "buttonFlashExplorer";
        this.buttonFlashExplorer.Size = new System.Drawing.Size(200, 30);
        this.buttonFlashExplorer.TabIndex = 99;
        this.buttonFlashExplorer.Text = "🔍 Flash Explorer (EXPERIMENTAL)";
        this.buttonFlashExplorer.UseVisualStyleBackColor = true;
        this.buttonFlashExplorer.Click += new System.EventHandler(this.buttonFlashExplorer_Click);
    */

    // ============================================================================
    // EXAMPLE: How to find your Flash control
    // ============================================================================
    //
    // METHOD 1: Search in Form1.Designer.cs
    //
    // Open Form1.Designer.cs and search for:
    // - "ShockwaveFlash"
    // - "AxShockwaveFlash"
    // - "flash"
    //
    // You should find something like:
    /*
        private AxShockwaveFlashObjects.AxShockwaveFlash axShockwaveFlash1;

        this.axShockwaveFlash1 = new AxShockwaveFlashObjects.AxShockwaveFlash();
        this.axShockwaveFlash1.Name = "axShockwaveFlash1";
        ...
    */
    //
    // Then use:
    //   var flashControl = this.axShockwaveFlash1;
    //
    // ============================================================================
    //
    // METHOD 2: If Flash is in external window (flash.exe process)
    //
    // You'll need to use a different approach - Flash ExternalInterface only works
    // with embedded Flash controls, not external flash.exe processes.
    //
    // In this case, you'll need to use Memory Reading instead.
    // See FLASH_MEMORY_READING_GUIDE.md for instructions.
    //
    // ============================================================================
}
