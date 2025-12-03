using AutoVPT.Objects;
using System.Collections.Generic;

namespace AutoVPT.Interfaces
{
    /// <summary>
    /// Repository for Character entity persistence.
    /// Abstracts data access layer for character management.
    /// </summary>
    public interface ICharacterRepository
    {
        /// <summary>
        /// Get character by ID
        /// </summary>
        Character GetById(string id);

        /// <summary>
        /// Get all characters
        /// </summary>
        IEnumerable<Character> GetAll();

        /// <summary>
        /// Save or update character
        /// </summary>
        void Save(Character character);

        /// <summary>
        /// Delete character by ID
        /// </summary>
        void Delete(string id);

        /// <summary>
        /// Check if character exists
        /// </summary>
        bool Exists(string id);

        /// <summary>
        /// Get characters by group
        /// </summary>
        IEnumerable<Character> GetByGroup(string group);
    }
}
