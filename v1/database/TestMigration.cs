using AutoVPT.Objects;
using System;
using System.Text;
using System.Windows.Forms;

namespace AutoVPT.Database
{
    /// <summary>
    /// Test form for SQLite migration proof-of-concept
    /// This is a standalone test utility - integrate into main app as needed
    /// </summary>
    public partial class TestMigrationForm : Form
    {
        private TextBox txtLog;
        private Button btnInitialize;
        private Button btnMigrate;
        private Button btnVerify;
        private Button btnTestLoad;
        private Button btnTestSave;
        private Button btnShowIncomplete;
        private Button btnExportToXml;

        public TestMigrationForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "SQLite Migration Test";
            this.Size = new System.Drawing.Size(800, 600);

            // Log textbox
            txtLog = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Bottom,
                Height = 400,
                ReadOnly = true
            };

            // Buttons
            btnInitialize = new Button { Text = "1. Initialize Database", Left = 10, Top = 10, Width = 150 };
            btnMigrate = new Button { Text = "2. Migrate XML to SQLite", Left = 170, Top = 10, Width = 150 };
            btnVerify = new Button { Text = "3. Verify Migration", Left = 330, Top = 10, Width = 150 };
            btnTestLoad = new Button { Text = "4. Test Load", Left = 490, Top = 10, Width = 150 };
            btnTestSave = new Button { Text = "5. Test Save", Left = 10, Top = 50, Width = 150 };
            btnShowIncomplete = new Button { Text = "6. Show Incomplete Tasks", Left = 170, Top = 50, Width = 150 };
            btnExportToXml = new Button { Text = "7. Export to XML", Left = 330, Top = 50, Width = 150 };

            // Event handlers
            btnInitialize.Click += BtnInitialize_Click;
            btnMigrate.Click += BtnMigrate_Click;
            btnVerify.Click += BtnVerify_Click;
            btnTestLoad.Click += BtnTestLoad_Click;
            btnTestSave.Click += BtnTestSave_Click;
            btnShowIncomplete.Click += BtnShowIncomplete_Click;
            btnExportToXml.Click += BtnExportToXml_Click;

            // Add controls
            this.Controls.Add(txtLog);
            this.Controls.Add(btnInitialize);
            this.Controls.Add(btnMigrate);
            this.Controls.Add(btnVerify);
            this.Controls.Add(btnTestLoad);
            this.Controls.Add(btnTestSave);
            this.Controls.Add(btnShowIncomplete);
            this.Controls.Add(btnExportToXml);
        }

        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }

        private string PromptForInput(string prompt, string title)
        {
            Form inputForm = new Form
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label label = new Label { Left = 20, Top = 20, Width = 350, Text = prompt };
            TextBox textBox = new TextBox { Left = 20, Top = 50, Width = 350 };
            Button confirmation = new Button { Text = "OK", Left = 220, Width = 70, Top = 80, DialogResult = DialogResult.OK };
            Button cancel = new Button { Text = "Cancel", Left = 300, Width = 70, Top = 80, DialogResult = DialogResult.Cancel };

            confirmation.Click += (sender, e) => { inputForm.Close(); };
            cancel.Click += (sender, e) => { inputForm.Close(); };

            inputForm.Controls.Add(label);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(confirmation);
            inputForm.Controls.Add(cancel);
            inputForm.AcceptButton = confirmation;
            inputForm.CancelButton = cancel;

            return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void BtnInitialize_Click(object sender, EventArgs e)
        {
            try
            {
                Log("Initializing SQLite database...");
                DatabaseHelper.Initialize();
                Log("✓ Database initialized successfully!");
                Log($"Database location: {Application.StartupPath}\\database\\mchelper.db");
            }
            catch (Exception ex)
            {
                Log($"✗ Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMigrate_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will migrate all XML character files to SQLite.\n" +
                "XML files will be backed up automatically.\n\n" +
                "Proceed with migration?",
                "Confirm Migration",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes) return;

            try
            {
                Log("Starting migration from XML to SQLite...");

                var migrationResult = MigrationUtility.MigrateXmlToSqlite(backupXmlFiles: true);

                if (migrationResult.Success)
                {
                    Log($"✓ Migration successful!");
                    Log($"  Characters migrated: {migrationResult.CharactersMigrated}");

                    foreach (var id in migrationResult.MigratedCharacterIds)
                    {
                        Log($"  - {id}");
                    }

                    if (migrationResult.Errors.Count > 0)
                    {
                        Log($"  Warnings: {migrationResult.Errors.Count}");
                        foreach (var error in migrationResult.Errors)
                        {
                            Log($"    ! {error}");
                        }
                    }
                }
                else
                {
                    Log($"✗ Migration failed: {migrationResult.ErrorMessage}");

                    foreach (var error in migrationResult.Errors)
                    {
                        Log($"  ! {error}");
                    }
                }

                MessageBox.Show(migrationResult.ToString(), "Migration Result",
                    MessageBoxButtons.OK,
                    migrationResult.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Log($"✗ Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnVerify_Click(object sender, EventArgs e)
        {
            try
            {
                Log("Verifying migration by comparing XML and SQLite data...");

                var verifyResult = MigrationUtility.VerifyMigration();

                if (verifyResult.Success)
                {
                    Log($"✓ Verification successful!");
                    Log($"  Matched characters: {verifyResult.MatchedCharacters}");
                }
                else
                {
                    Log($"✗ Verification failed!");

                    if (verifyResult.Mismatches.Count > 0)
                    {
                        Log($"  Mismatches ({verifyResult.Mismatches.Count}):");
                        foreach (var mismatch in verifyResult.Mismatches)
                        {
                            Log($"    ! {mismatch}");
                        }
                    }

                    if (verifyResult.Errors.Count > 0)
                    {
                        Log($"  Errors ({verifyResult.Errors.Count}):");
                        foreach (var error in verifyResult.Errors)
                        {
                            Log($"    ! {error}");
                        }
                    }
                }

                MessageBox.Show(verifyResult.ToString(), "Verification Result",
                    MessageBoxButtons.OK,
                    verifyResult.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Log($"✗ Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTestLoad_Click(object sender, EventArgs e)
        {
            // Use a simple form-based input dialog instead of VB InputBox
            string input = PromptForInput("Enter character ID to load:", "Test Load");

            if (string.IsNullOrEmpty(input)) return;

            try
            {
                Log($"Loading character: {input}");

                var character = DatabaseHelper.LoadCharacter(input);

                if (character != null && !string.IsNullOrEmpty(character.ID))
                {
                    Log($"✓ Character loaded successfully!");
                    Log($"  ID: {character.ID}");
                    Log($"  Link: {character.Link}");
                    Log($"  Group: {character.Group}");
                    Log($"  VIP Level: {character.VipLevel}");
                    Log($"  Date: {character.Date}");
                    Log($"  DoiNangNo: {character.DoiNangNo} (Status: {character.StatusDoiNangNo})");
                    Log($"  TriAn: {character.TriAn} (Status: {character.StatusTriAn})");
                }
                else
                {
                    Log($"✗ Character not found: {input}");
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTestSave_Click(object sender, EventArgs e)
        {
            try
            {
                Log("Creating and saving test character...");

                var testCharacter = new Character
                {
                    ID = "TEST_" + DateTime.Now.Ticks,
                    Link = "http://test.com",
                    Group = "TestGroup",
                    VipLevel = 5,
                    Date = DateTime.Now.ToString("yyyy-MM-dd"),
                    DoiNangNo = 1,
                    StatusDoiNangNo = 0,
                    TriAn = 1,
                    StatusTriAn = 1
                };

                DatabaseHelper.SaveCharacter(testCharacter);

                Log($"✓ Test character saved successfully!");
                Log($"  ID: {testCharacter.ID}");
                Log($"  Link: {testCharacter.Link}");
                Log($"  Group: {testCharacter.Group}");

                // Verify by loading it back
                var loaded = DatabaseHelper.LoadCharacter(testCharacter.ID);

                if (loaded != null && loaded.ID == testCharacter.ID)
                {
                    Log($"✓ Verification: Character loaded back successfully!");
                }
                else
                {
                    Log($"✗ Verification failed: Could not load saved character");
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnShowIncomplete_Click(object sender, EventArgs e)
        {
            try
            {
                Log("Querying characters with incomplete tasks...");

                var incomplete = DatabaseHelper.GetCharactersWithIncompleteTasks();

                Log($"Found {incomplete.Count} character(s) with incomplete tasks:");

                if (incomplete.Count == 0)
                {
                    Log("  (All characters have completed their daily tasks)");
                }
                else
                {
                    foreach (var c in incomplete)
                    {
                        StringBuilder tasks = new StringBuilder();

                        if (c.DoiNangNo == 1 && c.StatusDoiNangNo == 0) tasks.Append("DoiNangNo, ");
                        if (c.TriAn == 1 && c.StatusTriAn == 0) tasks.Append("TriAn, ");
                        if (c.LatTheBai == 1 && c.StatusLatTheBai == 0) tasks.Append("LatTheBai, ");
                        if (c.TruMa == 1 && c.StatusTruMa == 0) tasks.Append("TruMa, ");
                        if (c.AutoPhuBan == 1 && c.StatusAutoPhuBan == 0) tasks.Append("AutoPhuBan, ");

                        Log($"  - {c.ID} (Group: {c.Group}): {tasks.ToString().TrimEnd(',', ' ')}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportToXml_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will export all SQLite data back to XML files.\n" +
                "Existing XML files will be overwritten.\n\n" +
                "Proceed?",
                "Confirm Export",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes) return;

            try
            {
                Log("Exporting SQLite data to XML files...");

                MigrationUtility.ExportSqliteToXml();

                Log("✓ Export completed successfully!");
                Log($"  XML files saved to: {Application.StartupPath}\\database\\");
            }
            catch (Exception ex)
            {
                Log($"✗ Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
