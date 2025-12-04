using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AutoVPT.Presentation
{
    /// <summary>
    /// Interface for the character management view (Form1).
    /// Defines what UI operations the view must support.
    /// This enables testability and separates UI from business logic.
    /// </summary>
    public interface ICharacterView
    {
        #region Character Data

        /// <summary>
        /// Currently selected character.
        /// </summary>
        Character SelectedCharacter { get; set; }

        /// <summary>
        /// Currently selected character ID.
        /// </summary>
        string SelectedCharacterId { get; }

        /// <summary>
        /// Get all characters from the grid.
        /// </summary>
        IEnumerable<string> GetAllCharacterIds();

        /// <summary>
        /// Refresh the character list from data source.
        /// </summary>
        void RefreshCharacterList();

        /// <summary>
        /// Update the character in the data source.
        /// </summary>
        void UpdateCharacter();

        #endregion

        #region UI Operations

        /// <summary>
        /// Show a message to the user.
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="title">Message title</param>
        /// <param name="icon">Message icon</param>
        void ShowMessage(string message, string title = "Thông báo", MessageBoxIcon icon = MessageBoxIcon.Information);

        /// <summary>
        /// Show an error message.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="title">Error title</param>
        void ShowError(string message, string title = "Lỗi");

        /// <summary>
        /// Show a confirmation dialog.
        /// </summary>
        /// <param name="message">Confirmation message</param>
        /// <param name="title">Dialog title</param>
        /// <returns>True if user confirms, false otherwise</returns>
        bool ShowConfirmation(string message, string title = "Xác nhận");

        /// <summary>
        /// Append text to the status text box.
        /// </summary>
        /// <param name="status">Status text to append</param>
        void AppendStatus(string status);

        /// <summary>
        /// Clear the status text box.
        /// </summary>
        void ClearStatus();

        /// <summary>
        /// Get the status TextBox for services that need direct access.
        /// </summary>
        TextBox StatusTextBox { get; }

        #endregion

        #region Feature Configuration

        /// <summary>
        /// Get selected dungeon (Phu Ban) items.
        /// </summary>
        string[] GetSelectedPhuBan();

        /// <summary>
        /// Get selected STMT items.
        /// </summary>
        string[] GetSelectedSTMT();

        /// <summary>
        /// Get group members.
        /// </summary>
        string[] GetGroupMembers();

        /// <summary>
        /// Get the selected Mat Bao type.
        /// </summary>
        string GetMatBaoType();

        /// <summary>
        /// Get the selected Mat Bao level.
        /// </summary>
        int GetMatBaoLevel();

        #endregion

        #region State

        /// <summary>
        /// Enable or disable the form.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Set the cursor to wait/default.
        /// </summary>
        bool IsWaiting { get; set; }

        /// <summary>
        /// Whether config should be renewed.
        /// </summary>
        bool RenewConfig { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Event raised when a character is selected.
        /// </summary>
        event EventHandler<CharacterSelectedEventArgs> CharacterSelected;

        /// <summary>
        /// Event raised when a feature should be executed.
        /// </summary>
        event EventHandler<FeatureExecutionEventArgs> FeatureExecutionRequested;

        #endregion
    }

    /// <summary>
    /// Event args for character selection.
    /// </summary>
    public class CharacterSelectedEventArgs : EventArgs
    {
        public string CharacterId { get; set; }
        public Character Character { get; set; }
    }

    /// <summary>
    /// Event args for feature execution requests.
    /// </summary>
    public class FeatureExecutionEventArgs : EventArgs
    {
        public string FeatureName { get; set; }
        public bool ExecuteForAll { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public FeatureExecutionEventArgs()
        {
            Parameters = new Dictionary<string, object>();
        }
    }
}

