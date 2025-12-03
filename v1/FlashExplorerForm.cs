using AutoVPT.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AutoVPT
{
    /// <summary>
    /// Diagnostic tool to explore Flash game structure and test ExternalInterface
    /// This form helps reverse engineer what data the Flash game exposes
    /// </summary>
    public partial class FlashExplorerForm : Form
    {
        private FlashGameReader _flashReader;
        private IntPtr _windowHandle;

        private TextBox txtVariablePath;
        private TextBox txtFunctionName;
        private TextBox txtFunctionArgs;
        private TextBox txtOutput;
        private Button btnGetVariable;
        private Button btnCallFunction;
        private Button btnAutoExplore;
        private Button btnTestQuest;
        private Button btnTestPosition;
        private Button btnClear;
        private ListBox lstFoundVariables;
        private Label lblStatus;

        public FlashExplorerForm(IntPtr flashWindowHandle)
        {
            _windowHandle = flashWindowHandle;
            InitializeComponents();
            InitializeFlashReader();
        }

        private void InitializeComponents()
        {
            this.Text = "Flash ExternalInterface Explorer - EXPERIMENTAL";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Instructions label
            var lblInstructions = new Label
            {
                Text = "This tool tests if the Flash game exposes data via ExternalInterface.\n" +
                       "Try the Auto-Explore first, then manually test specific variables.",
                Location = new Point(10, 10),
                Size = new Size(860, 40),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            this.Controls.Add(lblInstructions);

            // Variable Path section
            var lblVarPath = new Label
            {
                Text = "Variable Path (e.g., _root.player.x):",
                Location = new Point(10, 60),
                Size = new Size(250, 20)
            };
            this.Controls.Add(lblVarPath);

            txtVariablePath = new TextBox
            {
                Location = new Point(10, 85),
                Size = new Size(400, 25),
                Text = "_root"
            };
            this.Controls.Add(txtVariablePath);

            btnGetVariable = new Button
            {
                Text = "Get Variable",
                Location = new Point(420, 83),
                Size = new Size(120, 27)
            };
            btnGetVariable.Click += BtnGetVariable_Click;
            this.Controls.Add(btnGetVariable);

            // Function Call section
            var lblFunction = new Label
            {
                Text = "Function Name:",
                Location = new Point(10, 120),
                Size = new Size(150, 20)
            };
            this.Controls.Add(lblFunction);

            txtFunctionName = new TextBox
            {
                Location = new Point(10, 145),
                Size = new Size(200, 25),
                Text = "getQuestStatus"
            };
            this.Controls.Add(txtFunctionName);

            var lblArgs = new Label
            {
                Text = "Arguments (comma-separated):",
                Location = new Point(220, 120),
                Size = new Size(200, 20)
            };
            this.Controls.Add(lblArgs);

            txtFunctionArgs = new TextBox
            {
                Location = new Point(220, 145),
                Size = new Size(190, 25)
            };
            this.Controls.Add(txtFunctionArgs);

            btnCallFunction = new Button
            {
                Text = "Call Function",
                Location = new Point(420, 143),
                Size = new Size(120, 27)
            };
            btnCallFunction.Click += BtnCallFunction_Click;
            this.Controls.Add(btnCallFunction);

            // Quick Test Buttons
            btnAutoExplore = new Button
            {
                Text = "Auto-Explore Game",
                Location = new Point(10, 185),
                Size = new Size(150, 30),
                BackColor = Color.LightGreen
            };
            btnAutoExplore.Click += BtnAutoExplore_Click;
            this.Controls.Add(btnAutoExplore);

            btnTestQuest = new Button
            {
                Text = "Test Quest Status",
                Location = new Point(170, 185),
                Size = new Size(150, 30),
                BackColor = Color.LightBlue
            };
            btnTestQuest.Click += BtnTestQuest_Click;
            this.Controls.Add(btnTestQuest);

            btnTestPosition = new Button
            {
                Text = "Test Character Position",
                Location = new Point(330, 185),
                Size = new Size(150, 30),
                BackColor = Color.LightBlue
            };
            btnTestPosition.Click += BtnTestPosition_Click;
            this.Controls.Add(btnTestPosition);

            btnClear = new Button
            {
                Text = "Clear Output",
                Location = new Point(490, 185),
                Size = new Size(100, 30)
            };
            btnClear.Click += (s, e) => { txtOutput.Clear(); lstFoundVariables.Items.Clear(); };
            this.Controls.Add(btnClear);

            // Found Variables List
            var lblFound = new Label
            {
                Text = "Found Variables:",
                Location = new Point(10, 225),
                Size = new Size(150, 20)
            };
            this.Controls.Add(lblFound);

            lstFoundVariables = new ListBox
            {
                Location = new Point(10, 250),
                Size = new Size(540, 150),
                Font = new Font("Consolas", 9)
            };
            lstFoundVariables.DoubleClick += LstFoundVariables_DoubleClick;
            this.Controls.Add(lstFoundVariables);

            // Output text area
            var lblOutput = new Label
            {
                Text = "Output / Results:",
                Location = new Point(10, 410),
                Size = new Size(150, 20)
            };
            this.Controls.Add(lblOutput);

            txtOutput = new TextBox
            {
                Location = new Point(10, 435),
                Size = new Size(860, 180),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 9),
                ReadOnly = true
            };
            this.Controls.Add(txtOutput);

            // Status label
            lblStatus = new Label
            {
                Text = "Status: Initializing...",
                Location = new Point(10, 625),
                Size = new Size(860, 20),
                ForeColor = Color.Blue
            };
            this.Controls.Add(lblStatus);

            // Usage tips
            var lblTips = new Label
            {
                Text = "💡 Tips: Double-click a found variable to test it | Use Auto-Explore first to discover available data",
                Location = new Point(10, 645),
                Size = new Size(860, 20),
                Font = new Font("Segoe UI", 8, FontStyle.Italic)
            };
            this.Controls.Add(lblTips);
        }

        private void InitializeFlashReader()
        {
            try
            {
                // Try to get Flash control from window handle
                // This is a simplified approach - you may need to adjust based on actual Flash control access
                Log("Attempting to initialize Flash reader...");
                Log($"Target window handle: 0x{_windowHandle:X}");

                // Note: You'll need to pass the actual AxShockwaveFlash control from Form1
                // For now, this is a placeholder
                Log("⚠️ IMPORTANT: FlashGameReader needs the actual Flash control object.");
                Log("   Call this form with: new FlashExplorerForm(flashControl) instead of window handle");
                Log("");
                lblStatus.Text = "Status: Waiting for Flash control...";
            }
            catch (Exception ex)
            {
                Log($"❌ Failed to initialize: {ex.Message}");
                lblStatus.Text = "Status: Initialization failed";
                lblStatus.ForeColor = Color.Red;
            }
        }

        public void SetFlashReader(FlashGameReader reader)
        {
            _flashReader = reader;

            if (_flashReader.IsAvailable)
            {
                Log("✅ Flash ExternalInterface is AVAILABLE!");
                Log("   The game exposes data. You can read game state directly!");
                Log("");
                lblStatus.Text = "Status: Connected - ExternalInterface Available ✅";
                lblStatus.ForeColor = Color.Green;
            }
            else
            {
                Log("❌ Flash ExternalInterface is NOT available");
                Log("   The game does not expose ExternalInterface API.");
                Log("   You'll need to use memory reading or packet capture instead.");
                Log("");
                lblStatus.Text = "Status: ExternalInterface Not Available ❌";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void BtnGetVariable_Click(object sender, EventArgs e)
        {
            if (_flashReader == null)
            {
                Log("❌ Flash reader not initialized");
                return;
            }

            string path = txtVariablePath.Text.Trim();
            if (string.IsNullOrEmpty(path))
            {
                Log("❌ Please enter a variable path");
                return;
            }

            try
            {
                Log($"Getting variable: {path}");
                string value = _flashReader.GetVariable(path);

                if (string.IsNullOrEmpty(value) || value == "undefined" || value == "null")
                {
                    Log($"   Result: (not found or null)");
                }
                else
                {
                    Log($"   ✅ Result: {value}");
                    AddToFoundList(path, value);
                }
            }
            catch (Exception ex)
            {
                Log($"   ❌ Error: {ex.Message}");
            }

            Log("");
        }

        private void BtnCallFunction_Click(object sender, EventArgs e)
        {
            if (_flashReader == null)
            {
                Log("❌ Flash reader not initialized");
                return;
            }

            string funcName = txtFunctionName.Text.Trim();
            if (string.IsNullOrEmpty(funcName))
            {
                Log("❌ Please enter a function name");
                return;
            }

            try
            {
                // Parse arguments
                string[] args = string.IsNullOrEmpty(txtFunctionArgs.Text)
                    ? new string[0]
                    : txtFunctionArgs.Text.Split(',').Select(s => s.Trim()).ToArray();

                Log($"Calling function: {funcName}({string.Join(", ", args)})");
                string result = _flashReader.CallFunction(funcName, args);

                Log($"   ✅ Result: {result}");
            }
            catch (Exception ex)
            {
                Log($"   ❌ Error: {ex.Message}");
            }

            Log("");
        }

        private void BtnAutoExplore_Click(object sender, EventArgs e)
        {
            if (_flashReader == null)
            {
                Log("❌ Flash reader not initialized");
                return;
            }

            Log("========================================");
            Log("AUTO-EXPLORING FLASH GAME STRUCTURE...");
            Log("========================================");
            Log("");

            try
            {
                var found = _flashReader.ExploreGameStructure();

                if (found.Count == 0)
                {
                    Log("❌ No variables found.");
                    Log("   This means either:");
                    Log("   1. The game doesn't expose ExternalInterface");
                    Log("   2. The variable names are non-standard");
                    Log("   3. Variables are in different paths");
                    Log("");
                    Log("   Next steps:");
                    Log("   - Try memory reading approach instead");
                    Log("   - Or use Cheat Engine to find actual variable names");
                }
                else
                {
                    Log($"✅ FOUND {found.Count} ACCESSIBLE VARIABLES!");
                    Log("");

                    foreach (var kvp in found.OrderBy(k => k.Key))
                    {
                        Log($"   {kvp.Key} = {kvp.Value}");
                        AddToFoundList(kvp.Key, kvp.Value);
                    }

                    Log("");
                    Log("🎉 SUCCESS! The game exposes data via ExternalInterface!");
                    Log("   You can now replace image recognition with direct data reading!");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Error during exploration: {ex.Message}");
            }

            Log("");
        }

        private void BtnTestQuest_Click(object sender, EventArgs e)
        {
            if (_flashReader == null)
            {
                Log("❌ Flash reader not initialized");
                return;
            }

            Log("Testing quest status detection...");

            try
            {
                string questStatus = _flashReader.GetQuestStatus();

                if (questStatus != null)
                {
                    Log($"   ✅ Quest Status: {questStatus}");
                    Log("   🎉 Quest detection works! You can eliminate quest image recognition!");
                }
                else
                {
                    Log("   ❌ Quest status not found via standard paths");
                    Log("   Try exploring to find the actual quest variable path");
                }
            }
            catch (Exception ex)
            {
                Log($"   ❌ Error: {ex.Message}");
            }

            Log("");
        }

        private void BtnTestPosition_Click(object sender, EventArgs e)
        {
            if (_flashReader == null)
            {
                Log("❌ Flash reader not initialized");
                return;
            }

            Log("Testing character position detection...");

            try
            {
                Point? pos = _flashReader.GetCharacterPosition();

                if (pos.HasValue)
                {
                    Log($"   ✅ Character Position: ({pos.Value.X}, {pos.Value.Y})");
                    Log("   🎉 Position detection works! You can eliminate navigation image recognition!");
                }
                else
                {
                    Log("   ❌ Character position not found via standard paths");
                    Log("   Try exploring to find the actual position variable path");
                }
            }
            catch (Exception ex)
            {
                Log($"   ❌ Error: {ex.Message}");
            }

            Log("");
        }

        private void LstFoundVariables_DoubleClick(object sender, EventArgs e)
        {
            if (lstFoundVariables.SelectedItem == null)
                return;

            string item = lstFoundVariables.SelectedItem.ToString();
            string path = item.Split('=')[0].Trim();

            txtVariablePath.Text = path;
            BtnGetVariable_Click(sender, e);
        }

        private void AddToFoundList(string path, string value)
        {
            string item = $"{path} = {(value.Length > 50 ? value.Substring(0, 50) + "..." : value)}";

            if (!lstFoundVariables.Items.Contains(item))
            {
                lstFoundVariables.Items.Add(item);
            }
        }

        private void Log(string message)
        {
            txtOutput.AppendText(message + Environment.NewLine);
            txtOutput.SelectionStart = txtOutput.Text.Length;
            txtOutput.ScrollToCaret();
        }
    }
}
