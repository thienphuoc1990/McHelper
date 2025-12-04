using AutoVPT.Libs;
using AutoVPT.Objects;
using AutoVPT.Services;
using KAutoHelper;
using System;
using System.Threading;

namespace AutoVPT.Presentation
{
    /// <summary>
    /// Presenter for character management operations.
    /// Coordinates between the view (Form1) and services.
    /// Implements business logic that was previously in Form1.cs.
    /// </summary>
    internal class CharacterPresenter
    {
        private readonly ICharacterView _view;
        private readonly IWindowManagementService _windowService;

        /// <summary>
        /// Create a new CharacterPresenter.
        /// </summary>
        /// <param name="view">The view to coordinate with</param>
        /// <param name="windowService">Optional window management service</param>
        public CharacterPresenter(ICharacterView view, IWindowManagementService windowService = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _windowService = windowService ?? new WindowManagementService();

            // Subscribe to view events
            _view.CharacterSelected += OnCharacterSelected;
            _view.FeatureExecutionRequested += OnFeatureExecutionRequested;
        }

        #region Character Management

        /// <summary>
        /// Check if a character is selected in the view.
        /// </summary>
        /// <returns>True if a character is selected, false otherwise</returns>
        public bool CheckCharacterSelected()
        {
            if (string.IsNullOrEmpty(_view.SelectedCharacterId))
            {
                _view.ShowMessage("Vui lòng chọn một nhân vật trước.", "Chưa chọn nhân vật", System.Windows.Forms.MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Load character settings and return the character.
        /// </summary>
        /// <param name="characterId">Character ID to load</param>
        /// <returns>Loaded character or null if not found</returns>
        public Character LoadCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return null;

            try
            {
                return Helper.loadSettingsFromXML(characterId);
            }
            catch (Exception ex)
            {
                Logger.LogError(characterId, "LoadCharacter", ex);
                _view.ShowError($"Không thể tải nhân vật: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if the game window is open for a character.
        /// </summary>
        /// <param name="characterId">Character ID to check</param>
        /// <returns>True if window is open, false otherwise</returns>
        public bool CheckWindowOpen(string characterId)
        {
            return _windowService.IsWindowOpen(characterId);
        }

        /// <summary>
        /// Open the game window for a character.
        /// </summary>
        /// <param name="character">Character to open window for</param>
        /// <returns>Window handle, or IntPtr.Zero if failed</returns>
        public IntPtr OpenWindow(Character character)
        {
            if (character == null)
                return IntPtr.Zero;

            try
            {
                return _windowService.OpenWindow(character);
            }
            catch (Exception ex)
            {
                Logger.LogError(character.ID, "OpenWindow", ex);
                _view.ShowError($"Không thể mở cửa sổ: {ex.Message}");
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Get the window handle for a character, opening window if needed.
        /// </summary>
        /// <param name="character">Character to get window for</param>
        /// <returns>Window handle</returns>
        public IntPtr GetHandledWindow(Character character)
        {
            if (character == null)
                return IntPtr.Zero;

            // Check if config needs renewal
            if (_view.RenewConfig)
            {
                character = LoadCharacter(character.ID);
                _view.SelectedCharacter = character;
                _view.RenewConfig = false;
            }

            // Set running state
            character.Running = 1;
            _view.UpdateCharacter();

            // Open window if needed
            if (!_windowService.IsWindowOpen(character.ID))
            {
                OpenWindow(character);
            }

            return _windowService.GetWindowHandle(character.ID);
        }

        #endregion

        #region Feature Execution

        /// <summary>
        /// Execute a feature for a single character.
        /// </summary>
        /// <param name="featureAction">Action to execute</param>
        /// <param name="featureName">Name for logging</param>
        /// <param name="character">Character to execute for (uses selected if null)</param>
        public void ExecuteFeature(Action<MainAuto> featureAction, string featureName, Character character = null)
        {
            character = character ?? _view.SelectedCharacter;

            if (character == null)
            {
                _view.ShowMessage("Chưa chọn nhân vật.", "Lỗi", System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }

            IntPtr hWnd = GetHandledWindow(character);
            if (hWnd == IntPtr.Zero)
            {
                _view.ShowMessage("Không tìm thấy nhân vật này đang được chạy.", "Lỗi", System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }

            var mainAuto = new MainAuto(hWnd, character, _view.StatusTextBox);
            RunTaskInThread(() => featureAction(mainAuto), featureName, character);
        }

        /// <summary>
        /// Execute a feature for all characters.
        /// </summary>
        /// <param name="featureAction">Action to execute</param>
        /// <param name="featureName">Name for logging</param>
        /// <param name="requireWindowOpen">Whether to skip characters without open windows</param>
        public void ExecuteFeatureForAll(Action<MainAuto> featureAction, string featureName, bool requireWindowOpen = false)
        {
            foreach (var characterId in _view.GetAllCharacterIds())
            {
                try
                {
                    var character = LoadCharacter(characterId);
                    if (character == null || string.IsNullOrEmpty(character.ID))
                        continue;

                    if (requireWindowOpen && !CheckWindowOpen(character.ID))
                        continue;

                    IntPtr hWnd = GetHandledWindow(character);
                    if (hWnd == IntPtr.Zero)
                    {
                        Logger.LogWarning(character.ID, featureName, "Window handle not found - skipping character");
                        continue;
                    }

                    var mainAuto = new MainAuto(hWnd, character, _view.StatusTextBox);
                    RunTaskInThread(() => featureAction(mainAuto), featureName, character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
                catch (Exception ex)
                {
                    Logger.LogError(characterId ?? "Unknown", featureName, ex);
                }
            }
        }

        /// <summary>
        /// Run a task in a separate thread.
        /// </summary>
        /// <param name="action">Action to run</param>
        /// <param name="taskName">Name for the thread</param>
        /// <param name="character">Character the task is for</param>
        private void RunTaskInThread(Action action, string taskName, Character character)
        {
            Thread thread = new Thread(new ThreadStart(action))
            {
                Name = character.ID + "-" + taskName
            };
            thread.Start();
            
            // Add thread to tracking list
            lock (Helper._threadLock)
            {
                Helper.threadList.Add(thread);
            }
        }

        #endregion

        #region Stop Operations

        /// <summary>
        /// Stop all running features for a character.
        /// </summary>
        /// <param name="character">Character to stop</param>
        public void StopCharacter(Character character)
        {
            if (character == null)
                return;

            character.Running = 0;
            _view.UpdateCharacter();
            _view.AppendStatus($"{character.ID}: Đã dừng");
        }

        /// <summary>
        /// Stop all running features for all characters.
        /// </summary>
        public void StopAllCharacters()
        {
            _view.AppendStatus("Đang dừng tất cả...");

            // Use Helper's built-in stop all method which sets the flag and stops characters
            Helper.StopAllRunningCharacters();

            // Cancel tokens for each character
            foreach (var characterId in _view.GetAllCharacterIds())
            {
                try
                {
                    Helper.CancelToken(characterId);
                }
                catch (Exception ex)
                {
                    Logger.LogError(characterId, "StopAllCharacters", ex);
                }
            }

            // Abort running threads
            Helper.AbortAllThreads();

            // Clear global stop flag after a delay
            Thread.Sleep(500);
            Helper.ResetStopAllFlag();

            _view.RefreshCharacterList();
            _view.AppendStatus("Đã dừng tất cả!");
        }

        #endregion

        #region Event Handlers

        private void OnCharacterSelected(object sender, CharacterSelectedEventArgs e)
        {
            // Load character data when selected
            if (!string.IsNullOrEmpty(e.CharacterId))
            {
                var character = LoadCharacter(e.CharacterId);
                _view.SelectedCharacter = character;
            }
        }

        private void OnFeatureExecutionRequested(object sender, FeatureExecutionEventArgs e)
        {
            // Handle feature execution requests from view
            // This can be extended to handle different features
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleanup resources when presenter is no longer needed.
        /// </summary>
        public void Dispose()
        {
            _view.CharacterSelected -= OnCharacterSelected;
            _view.FeatureExecutionRequested -= OnFeatureExecutionRequested;
        }

        #endregion
    }
}

