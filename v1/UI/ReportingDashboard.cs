using AutoVPT.Database;
using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoVPT.UI
{
    /// <summary>
    /// Reporting dashboard showing character statistics and daily task progress
    /// </summary>
    public partial class ReportingDashboard : Form
    {
        private TabControl tabControl;
        private TabPage tabOverview;
        private TabPage tabIncomplete;
        private TabPage tabByGroup;
        private TabPage tabStats;

        // Overview tab controls
        private Label lblTotalCharacters;
        private Label lblCompletedToday;
        private Label lblIncompleteToday;
        private Label lblActiveGroups;
        private ProgressBar progressOverall;
        private Label lblOverallProgress;

        // Incomplete tasks tab
        private DataGridView dgvIncomplete;
        private Button btnRefreshIncomplete;
        private Label lblIncompleteCount;

        // By group tab
        private ComboBox cboGroups;
        private DataGridView dgvGroupCharacters;
        private Label lblGroupStats;
        private Button btnRefreshGroup;

        // Stats tab
        private DataGridView dgvStats;
        private Button btnRefreshStats;

        // Refresh all button
        private Button btnRefreshAll;

        public ReportingDashboard()
        {
            InitializeComponent();
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            this.Text = "Character Progress Dashboard";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 600);

            // Tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Create tabs
            tabOverview = new TabPage("Overview");
            tabIncomplete = new TabPage("Incomplete Tasks");
            tabByGroup = new TabPage("By Group");
            tabStats = new TabPage("Statistics");

            tabControl.TabPages.Add(tabOverview);
            tabControl.TabPages.Add(tabIncomplete);
            tabControl.TabPages.Add(tabByGroup);
            tabControl.TabPages.Add(tabStats);

            // Refresh all button (top right)
            btnRefreshAll = new Button
            {
                Text = "Refresh All",
                Location = new Point(this.Width - 120, 10),
                Width = 100,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRefreshAll.Click += BtnRefreshAll_Click;

            InitializeOverviewTab();
            InitializeIncompleteTab();
            InitializeByGroupTab();
            InitializeStatsTab();

            this.Controls.Add(tabControl);
            this.Controls.Add(btnRefreshAll);
        }

        private void InitializeOverviewTab()
        {
            int yPos = 20;
            int xPos = 20;

            // Total characters
            var lblTotalTitle = new Label
            {
                Text = "Total Characters:",
                Location = new Point(xPos, yPos),
                Width = 150,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            lblTotalCharacters = new Label
            {
                Text = "0",
                Location = new Point(xPos + 160, yPos),
                Width = 100,
                Font = new Font("Arial", 10)
            };

            yPos += 40;

            // Completed today
            var lblCompletedTitle = new Label
            {
                Text = "Fully Completed Today:",
                Location = new Point(xPos, yPos),
                Width = 150,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Green
            };
            lblCompletedToday = new Label
            {
                Text = "0",
                Location = new Point(xPos + 160, yPos),
                Width = 100,
                Font = new Font("Arial", 10),
                ForeColor = Color.Green
            };

            yPos += 40;

            // Incomplete today
            var lblIncompleteTitle = new Label
            {
                Text = "With Incomplete Tasks:",
                Location = new Point(xPos, yPos),
                Width = 150,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.OrangeRed
            };
            lblIncompleteToday = new Label
            {
                Text = "0",
                Location = new Point(xPos + 160, yPos),
                Width = 100,
                Font = new Font("Arial", 10),
                ForeColor = Color.OrangeRed
            };

            yPos += 40;

            // Active groups
            var lblActiveGroupsTitle = new Label
            {
                Text = "Active Groups:",
                Location = new Point(xPos, yPos),
                Width = 150,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            lblActiveGroups = new Label
            {
                Text = "0",
                Location = new Point(xPos + 160, yPos),
                Width = 100,
                Font = new Font("Arial", 10)
            };

            yPos += 60;

            // Overall progress bar
            var lblProgressTitle = new Label
            {
                Text = "Overall Daily Progress:",
                Location = new Point(xPos, yPos),
                Width = 150,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            yPos += 30;

            progressOverall = new ProgressBar
            {
                Location = new Point(xPos, yPos),
                Width = 400,
                Height = 30
            };

            lblOverallProgress = new Label
            {
                Location = new Point(xPos + 420, yPos + 5),
                Width = 100,
                Font = new Font("Arial", 10)
            };

            tabOverview.Controls.Add(lblTotalTitle);
            tabOverview.Controls.Add(lblTotalCharacters);
            tabOverview.Controls.Add(lblCompletedTitle);
            tabOverview.Controls.Add(lblCompletedToday);
            tabOverview.Controls.Add(lblIncompleteTitle);
            tabOverview.Controls.Add(lblIncompleteToday);
            tabOverview.Controls.Add(lblActiveGroupsTitle);
            tabOverview.Controls.Add(lblActiveGroups);
            tabOverview.Controls.Add(lblProgressTitle);
            tabOverview.Controls.Add(progressOverall);
            tabOverview.Controls.Add(lblOverallProgress);
        }

        private void InitializeIncompleteTab()
        {
            lblIncompleteCount = new Label
            {
                Location = new Point(20, 10),
                Width = 400,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            btnRefreshIncomplete = new Button
            {
                Text = "Refresh",
                Location = new Point(430, 8),
                Width = 80
            };
            btnRefreshIncomplete.Click += (s, e) => LoadIncompleteTasksTab();

            dgvIncomplete = new DataGridView
            {
                Location = new Point(20, 40),
                Width = 940,
                Height = 550,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            tabIncomplete.Controls.Add(lblIncompleteCount);
            tabIncomplete.Controls.Add(btnRefreshIncomplete);
            tabIncomplete.Controls.Add(dgvIncomplete);
        }

        private void InitializeByGroupTab()
        {
            var lblGroupTitle = new Label
            {
                Text = "Select Group:",
                Location = new Point(20, 15),
                Width = 100
            };

            cboGroups = new ComboBox
            {
                Location = new Point(130, 12),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboGroups.SelectedIndexChanged += (s, e) => LoadGroupTab();

            btnRefreshGroup = new Button
            {
                Text = "Refresh",
                Location = new Point(340, 10),
                Width = 80
            };
            btnRefreshGroup.Click += (s, e) => LoadGroupTab();

            lblGroupStats = new Label
            {
                Location = new Point(430, 15),
                Width = 500,
                Font = new Font("Arial", 9)
            };

            dgvGroupCharacters = new DataGridView
            {
                Location = new Point(20, 45),
                Width = 940,
                Height = 545,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            tabByGroup.Controls.Add(lblGroupTitle);
            tabByGroup.Controls.Add(cboGroups);
            tabByGroup.Controls.Add(btnRefreshGroup);
            tabByGroup.Controls.Add(lblGroupStats);
            tabByGroup.Controls.Add(dgvGroupCharacters);
        }

        private void InitializeStatsTab()
        {
            btnRefreshStats = new Button
            {
                Text = "Refresh",
                Location = new Point(20, 10),
                Width = 80
            };
            btnRefreshStats.Click += (s, e) => LoadStatsTab();

            dgvStats = new DataGridView
            {
                Location = new Point(20, 45),
                Width = 940,
                Height = 545,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            tabStats.Controls.Add(btnRefreshStats);
            tabStats.Controls.Add(dgvStats);
        }

        private void LoadDashboard()
        {
            LoadOverviewTab();
            LoadIncompleteTasksTab();
            LoadGroupComboBox();
            LoadStatsTab();
        }

        private void LoadOverviewTab()
        {
            try
            {
                var allCharacters = DatabaseHelper.LoadAllCharacters();
                var incomplete = DatabaseHelper.GetCharactersWithIncompleteTasks();

                int total = allCharacters.Count;
                int incompleteCount = incomplete.Count;
                int completed = total - incompleteCount;

                var groups = allCharacters
                    .Where(c => !string.IsNullOrEmpty(c.Group))
                    .Select(c => c.Group)
                    .Distinct()
                    .Count();

                lblTotalCharacters.Text = total.ToString();
                lblCompletedToday.Text = completed.ToString();
                lblIncompleteToday.Text = incompleteCount.ToString();
                lblActiveGroups.Text = groups.ToString();

                // Calculate overall progress
                int totalTasks = 0;
                int completedTasks = 0;

                foreach (var c in allCharacters)
                {
                    // Count enabled features
                    if (c.VipPromotion == 1) { totalTasks++; if (c.StatusVipPromotion == 1) completedTasks++; }
                    if (c.DoiNangNo == 1) { totalTasks++; if (c.StatusDoiNangNo == 1) completedTasks++; }
                    if (c.TriAn == 1) { totalTasks++; if (c.StatusTriAn == 1) completedTasks++; }
                    if (c.LatTheBai == 1) { totalTasks++; if (c.StatusLatTheBai == 1) completedTasks++; }
                    if (c.RutBo == 1) { totalTasks++; if (c.StatusRutBo == 1) completedTasks++; }
                    if (c.TruMa == 1) { totalTasks++; if (c.StatusTruMa == 1) completedTasks++; }
                    if (c.AutoPhuBan == 1) { totalTasks++; if (c.StatusAutoPhuBan == 1) completedTasks++; }
                    if (c.CheMatBao == 1) { totalTasks++; if (c.StatusCheMatBao == 1) completedTasks++; }
                }

                int progressPercent = totalTasks > 0 ? (completedTasks * 100 / totalTasks) : 0;
                progressOverall.Value = progressPercent;
                lblOverallProgress.Text = $"{progressPercent}% ({completedTasks}/{totalTasks})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading overview: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadIncompleteTasksTab()
        {
            try
            {
                var incomplete = DatabaseHelper.GetCharactersWithIncompleteTasks();

                lblIncompleteCount.Text = $"Characters with incomplete tasks: {incomplete.Count}";

                DataTable dt = new DataTable();
                dt.Columns.Add("Character ID");
                dt.Columns.Add("Group");
                dt.Columns.Add("Incomplete Tasks");

                foreach (var c in incomplete)
                {
                    List<string> incompleteTasks = new List<string>();

                    if (c.VipPromotion == 1 && c.StatusVipPromotion == 0) incompleteTasks.Add("VIP Promotion");
                    if (c.DoiNangNo == 1 && c.StatusDoiNangNo == 0) incompleteTasks.Add("Doi Nang No");
                    if (c.TriAn == 1 && c.StatusTriAn == 0) incompleteTasks.Add("Tri An");
                    if (c.LatTheBai == 1 && c.StatusLatTheBai == 0) incompleteTasks.Add("Lat The Bai");
                    if (c.RutBo == 1 && c.StatusRutBo == 0) incompleteTasks.Add("Rut Bo");
                    if (c.DoiKGDK == 1 && c.StatusDoiKGDK == 0) incompleteTasks.Add("Doi KGDK");
                    if (c.TuHanh == 1 && c.StatusTuHanh == 0) incompleteTasks.Add("Tu Hanh");
                    if (c.TruMa == 1 && c.StatusTruMa == 0) incompleteTasks.Add("Tru Ma");
                    if (c.AoMaThap == 1 && c.StatusAoMaThap == 0) incompleteTasks.Add("Ao Ma Thap");
                    if (c.TrongCay == 1 && c.StatusTrongCay == 0) incompleteTasks.Add("Trong Cay");
                    if (c.CheMatBao == 1 && c.StatusCheMatBao == 0) incompleteTasks.Add("Che Mat Bao");
                    if (c.AutoPhuBan == 1 && c.StatusAutoPhuBan == 0) incompleteTasks.Add("Auto Phu Ban");
                    if (c.UocNguyen == 1 && c.StatusUocNguyen == 0) incompleteTasks.Add("Uoc Nguyen");
                    if (c.NhanThuongHLVT == 1 && c.StatusNhanThuongHLVT == 0) incompleteTasks.Add("Nhan Thuong HLVT");
                    if (c.MeTran == 1 && c.StatusMeTran == 0) incompleteTasks.Add("Me Tran");
                    if (c.HaiThuoc == 1 && c.StatusHaiThuoc == 0) incompleteTasks.Add("Hai Thuoc");
                    if (c.CauCa == 1 && c.StatusCauCa == 0) incompleteTasks.Add("Cau Ca");
                    if (c.AutoThanTu == 1 && c.StatusAutoThanTu == 0) incompleteTasks.Add("Auto Than Tu");

                    dt.Rows.Add(c.ID, c.Group, string.Join(", ", incompleteTasks));
                }

                dgvIncomplete.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading incomplete tasks: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGroupComboBox()
        {
            try
            {
                var allCharacters = DatabaseHelper.LoadAllCharacters();
                var groups = allCharacters
                    .Where(c => !string.IsNullOrEmpty(c.Group))
                    .Select(c => c.Group)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                cboGroups.Items.Clear();
                cboGroups.Items.Add("(All Groups)");
                foreach (var group in groups)
                {
                    cboGroups.Items.Add(group);
                }

                if (cboGroups.Items.Count > 0)
                {
                    cboGroups.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading groups: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGroupTab()
        {
            try
            {
                if (cboGroups.SelectedItem == null) return;

                string selectedGroup = cboGroups.SelectedItem.ToString();
                List<Character> characters;

                if (selectedGroup == "(All Groups)")
                {
                    characters = DatabaseHelper.LoadAllCharacters();
                }
                else
                {
                    characters = DatabaseHelper.GetCharactersByGroup(selectedGroup);
                }

                int total = characters.Count;
                int withIncomplete = characters.Count(c =>
                    (c.DoiNangNo == 1 && c.StatusDoiNangNo == 0) ||
                    (c.TriAn == 1 && c.StatusTriAn == 0) ||
                    (c.TruMa == 1 && c.StatusTruMa == 0) ||
                    (c.AutoPhuBan == 1 && c.StatusAutoPhuBan == 0)
                );
                int completed = total - withIncomplete;

                lblGroupStats.Text = $"Total: {total} | Completed: {completed} | Incomplete: {withIncomplete}";

                DataTable dt = new DataTable();
                dt.Columns.Add("Character ID");
                dt.Columns.Add("Link");
                dt.Columns.Add("VIP Level");
                dt.Columns.Add("Enabled Features");
                dt.Columns.Add("Completed Features");
                dt.Columns.Add("Progress %");

                foreach (var c in characters)
                {
                    int enabled = (c.VipPromotion + c.DoiNangNo + c.TriAn + c.LatTheBai + c.RutBo +
                                  c.TruMa + c.AutoPhuBan + c.CheMatBao + c.UocNguyen + c.NhanThuongHLVT);

                    int completedFeatures = (c.StatusVipPromotion + c.StatusDoiNangNo + c.StatusTriAn +
                                           c.StatusLatTheBai + c.StatusRutBo + c.StatusTruMa +
                                           c.StatusAutoPhuBan + c.StatusCheMatBao + c.StatusUocNguyen +
                                           c.StatusNhanThuongHLVT);

                    int progress = enabled > 0 ? (completedFeatures * 100 / enabled) : 100;

                    dt.Rows.Add(c.ID, c.Link, c.VipLevel, enabled, completedFeatures, $"{progress}%");
                }

                dgvGroupCharacters.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading group data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStatsTab()
        {
            try
            {
                var allCharacters = DatabaseHelper.LoadAllCharacters();

                DataTable dt = new DataTable();
                dt.Columns.Add("Feature");
                dt.Columns.Add("Enabled Count");
                dt.Columns.Add("Completed Count");
                dt.Columns.Add("Completion %");

                void AddFeatureStat(string featureName, Func<Character, int> enabledSelector, Func<Character, int> statusSelector)
                {
                    int enabledCount = allCharacters.Count(c => enabledSelector(c) == 1);
                    int completedCount = allCharacters.Count(c => enabledSelector(c) == 1 && statusSelector(c) == 1);
                    int percent = enabledCount > 0 ? (completedCount * 100 / enabledCount) : 0;

                    dt.Rows.Add(featureName, enabledCount, completedCount, $"{percent}%");
                }

                AddFeatureStat("VIP Promotion", c => c.VipPromotion, c => c.StatusVipPromotion);
                AddFeatureStat("Doi Nang No", c => c.DoiNangNo, c => c.StatusDoiNangNo);
                AddFeatureStat("Tri An", c => c.TriAn, c => c.StatusTriAn);
                AddFeatureStat("Lat The Bai", c => c.LatTheBai, c => c.StatusLatTheBai);
                AddFeatureStat("Rut Bo", c => c.RutBo, c => c.StatusRutBo);
                AddFeatureStat("Doi KGDK", c => c.DoiKGDK, c => c.StatusDoiKGDK);
                AddFeatureStat("Tu Hanh", c => c.TuHanh, c => c.StatusTuHanh);
                AddFeatureStat("Tru Ma", c => c.TruMa, c => c.StatusTruMa);
                AddFeatureStat("Ao Ma Thap", c => c.AoMaThap, c => c.StatusAoMaThap);
                AddFeatureStat("Trong Cay", c => c.TrongCay, c => c.StatusTrongCay);
                AddFeatureStat("Che Mat Bao", c => c.CheMatBao, c => c.StatusCheMatBao);
                AddFeatureStat("Auto Phu Ban", c => c.AutoPhuBan, c => c.StatusAutoPhuBan);
                AddFeatureStat("Uoc Nguyen", c => c.UocNguyen, c => c.StatusUocNguyen);
                AddFeatureStat("Nhan Thuong HLVT", c => c.NhanThuongHLVT, c => c.StatusNhanThuongHLVT);
                AddFeatureStat("Me Tran", c => c.MeTran, c => c.StatusMeTran);
                AddFeatureStat("Hai Thuoc", c => c.HaiThuoc, c => c.StatusHaiThuoc);
                AddFeatureStat("Cau Ca", c => c.CauCa, c => c.StatusCauCa);
                AddFeatureStat("Auto Than Tu", c => c.AutoThanTu, c => c.StatusAutoThanTu);

                dgvStats.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading statistics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefreshAll_Click(object sender, EventArgs e)
        {
            LoadDashboard();
            MessageBox.Show("Dashboard refreshed!", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
