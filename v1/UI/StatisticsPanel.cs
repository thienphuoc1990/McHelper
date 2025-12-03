using AutoVPT.Database;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AutoVPT.UI
{
    /// <summary>
    /// Compact statistics panel for embedding in main form
    /// Shows quick overview of character progress
    /// </summary>
    public partial class StatisticsPanel : UserControl
    {
        private Label lblTitle;
        private Label lblTotalCharacters;
        private Label lblCompletedCount;
        private Label lblIncompleteCount;
        private ProgressBar progressBar;
        private Label lblProgressText;
        private Button btnViewDetails;
        private Button btnRefresh;
        private Timer autoRefreshTimer;

        public event EventHandler ViewDetailsClicked;

        public StatisticsPanel()
        {
            InitializeComponent();
            LoadStatistics();
            StartAutoRefresh();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(300, 180);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.WhiteSmoke;

            // Title
            lblTitle = new Label
            {
                Text = "Daily Progress",
                Location = new Point(10, 10),
                Width = 280,
                Font = new Font("Arial", 11, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            // Total characters
            lblTotalCharacters = new Label
            {
                Location = new Point(10, 40),
                Width = 280,
                Font = new Font("Arial", 9),
                Text = "Total Characters: 0"
            };

            // Completed count
            lblCompletedCount = new Label
            {
                Location = new Point(10, 60),
                Width = 280,
                Font = new Font("Arial", 9),
                ForeColor = Color.Green,
                Text = "✓ Completed: 0"
            };

            // Incomplete count
            lblIncompleteCount = new Label
            {
                Location = new Point(10, 80),
                Width = 280,
                Font = new Font("Arial", 9),
                ForeColor = Color.OrangeRed,
                Text = "○ Incomplete: 0"
            };

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(10, 110),
                Width = 200,
                Height = 20
            };

            lblProgressText = new Label
            {
                Location = new Point(220, 110),
                Width = 70,
                Font = new Font("Arial", 9),
                Text = "0%"
            };

            // View details button
            btnViewDetails = new Button
            {
                Text = "View Details",
                Location = new Point(10, 145),
                Width = 100,
                Height = 25
            };
            btnViewDetails.Click += BtnViewDetails_Click;

            // Refresh button
            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(120, 145),
                Width = 80,
                Height = 25
            };
            btnRefresh.Click += (s, e) => LoadStatistics();

            // Auto-refresh timer (every 30 seconds)
            autoRefreshTimer = new Timer
            {
                Interval = 30000
            };
            autoRefreshTimer.Tick += (s, e) => LoadStatistics();

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblTotalCharacters);
            this.Controls.Add(lblCompletedCount);
            this.Controls.Add(lblIncompleteCount);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblProgressText);
            this.Controls.Add(btnViewDetails);
            this.Controls.Add(btnRefresh);
        }

        public void LoadStatistics()
        {
            try
            {
                var allCharacters = DatabaseHelper.LoadAllCharacters();
                var incomplete = DatabaseHelper.GetCharactersWithIncompleteTasks();

                int total = allCharacters.Count;
                int incompleteCount = incomplete.Count;
                int completed = total - incompleteCount;

                lblTotalCharacters.Text = $"Total Characters: {total}";
                lblCompletedCount.Text = $"✓ Completed: {completed}";
                lblIncompleteCount.Text = $"○ Incomplete: {incompleteCount}";

                // Calculate overall progress
                int totalTasks = 0;
                int completedTasks = 0;

                foreach (var c in allCharacters)
                {
                    // Count enabled features and their completion status
                    if (c.VipPromotion == 1) { totalTasks++; if (c.StatusVipPromotion == 1) completedTasks++; }
                    if (c.DoiNangNo == 1) { totalTasks++; if (c.StatusDoiNangNo == 1) completedTasks++; }
                    if (c.TriAn == 1) { totalTasks++; if (c.StatusTriAn == 1) completedTasks++; }
                    if (c.LatTheBai == 1) { totalTasks++; if (c.StatusLatTheBai == 1) completedTasks++; }
                    if (c.RutBo == 1) { totalTasks++; if (c.StatusRutBo == 1) completedTasks++; }
                    if (c.TruMa == 1) { totalTasks++; if (c.StatusTruMa == 1) completedTasks++; }
                    if (c.AutoPhuBan == 1) { totalTasks++; if (c.StatusAutoPhuBan == 1) completedTasks++; }
                    if (c.CheMatBao == 1) { totalTasks++; if (c.StatusCheMatBao == 1) completedTasks++; }
                    if (c.UocNguyen == 1) { totalTasks++; if (c.StatusUocNguyen == 1) completedTasks++; }
                    if (c.NhanThuongHLVT == 1) { totalTasks++; if (c.StatusNhanThuongHLVT == 1) completedTasks++; }
                }

                int progressPercent = totalTasks > 0 ? (completedTasks * 100 / totalTasks) : 0;
                progressBar.Value = progressPercent;
                lblProgressText.Text = $"{progressPercent}%";

                // Update progress bar color based on progress
                if (progressPercent == 100)
                {
                    progressBar.ForeColor = Color.Green;
                }
                else if (progressPercent >= 75)
                {
                    progressBar.ForeColor = Color.LightGreen;
                }
                else if (progressPercent >= 50)
                {
                    progressBar.ForeColor = Color.Orange;
                }
                else
                {
                    progressBar.ForeColor = Color.OrangeRed;
                }
            }
            catch (Exception ex)
            {
                lblTotalCharacters.Text = "Error loading statistics";
                lblTotalCharacters.ForeColor = Color.Red;
                Console.WriteLine($"Error loading statistics: {ex.Message}");
            }
        }

        private void BtnViewDetails_Click(object sender, EventArgs e)
        {
            ViewDetailsClicked?.Invoke(this, EventArgs.Empty);
        }

        public void StartAutoRefresh()
        {
            autoRefreshTimer?.Start();
        }

        public void StopAutoRefresh()
        {
            autoRefreshTimer?.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                autoRefreshTimer?.Stop();
                autoRefreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
