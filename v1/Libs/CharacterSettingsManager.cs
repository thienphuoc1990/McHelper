using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace AutoVPT.Libs
{
    /// <summary>
    /// Manages character settings with batched saves to reduce I/O operations
    /// </summary>
    public class CharacterSettingsManager : IDisposable
    {
        private static CharacterSettingsManager _instance;
        private static object _instanceLock = new object();

        private Timer _saveTimer;
        private HashSet<string> _dirtyCharacters = new HashSet<string>();
        private Dictionary<string, Character> _characterCache = new Dictionary<string, Character>();
        private object _lock = new object();
        private bool _disposed = false;
        private int _debounceMs;

        public static CharacterSettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CharacterSettingsManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private CharacterSettingsManager(int debounceMs = 500)
        {
            _debounceMs = debounceMs;
            _saveTimer = new Timer(_debounceMs);
            _saveTimer.Elapsed += OnTimerElapsed;
            _saveTimer.AutoReset = false; // Only fire once per trigger
        }

        /// <summary>
        /// Get character from cache or load from XML
        /// </summary>
        public Character GetCharacter(string characterId)
        {
            lock (_lock)
            {
                if (!_characterCache.ContainsKey(characterId))
                {
                    _characterCache[characterId] = Helper.loadSettingsFromXML(characterId);
                }
                return _characterCache[characterId];
            }
        }

        /// <summary>
        /// Mark character as dirty (needs saving) and schedule a save
        /// </summary>
        public void MarkDirty(Character character)
        {
            if (character == null || string.IsNullOrEmpty(character.ID))
            {
                return;
            }

            lock (_lock)
            {
                _characterCache[character.ID] = character;
                _dirtyCharacters.Add(character.ID);

                // Restart the timer
                _saveTimer.Stop();
                _saveTimer.Start();
            }
        }

        /// <summary>
        /// Save immediately without waiting for timer
        /// </summary>
        public void SaveNow(string characterId)
        {
            lock (_lock)
            {
                if (_characterCache.ContainsKey(characterId))
                {
                    try
                    {
                        Helper.saveSettingsToXML(_characterCache[characterId]);
                        _dirtyCharacters.Remove(characterId);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(characterId, "CharacterSettingsManager.SaveNow", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Flush all pending saves immediately
        /// </summary>
        public void FlushAll()
        {
            lock (_lock)
            {
                var charactersToSave = _dirtyCharacters.ToList();
                foreach (var characterId in charactersToSave)
                {
                    if (_characterCache.ContainsKey(characterId))
                    {
                        try
                        {
                            Helper.saveSettingsToXML(_characterCache[characterId]);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(characterId, "CharacterSettingsManager.FlushAll", ex);
                        }
                    }
                }
                _dirtyCharacters.Clear();
            }
        }

        /// <summary>
        /// Clear character from cache (useful when character is deleted)
        /// </summary>
        public void RemoveFromCache(string characterId)
        {
            lock (_lock)
            {
                // Save before removing if dirty
                if (_dirtyCharacters.Contains(characterId))
                {
                    SaveNow(characterId);
                }

                _characterCache.Remove(characterId);
                _dirtyCharacters.Remove(characterId);
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            FlushAll();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Flush all pending saves before disposing
                    FlushAll();

                    _saveTimer?.Stop();
                    _saveTimer?.Dispose();
                }
                _disposed = true;
            }
        }

        ~CharacterSettingsManager()
        {
            Dispose(false);
        }
    }
}
