using AutoVPT.Interfaces;
using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoVPT.Repositories
{
    /// <summary>
    /// XML-based character repository implementation.
    /// Wraps existing Helper.cs XML serialization logic.
    /// </summary>
    public class XmlCharacterRepository : ICharacterRepository
    {
        private readonly string _databasePath;

        public XmlCharacterRepository(string databasePath = "database")
        {
            _databasePath = databasePath;

            // Ensure database directory exists
            if (!Directory.Exists(_databasePath))
            {
                Directory.CreateDirectory(_databasePath);
            }
        }

        public Character GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            // Use existing Helper method for backward compatibility
            return Helper.loadSettingsFromXML(id);
        }

        public IEnumerable<Character> GetAll()
        {
            var characters = new List<Character>();

            if (!Directory.Exists(_databasePath))
                return characters;

            var xmlFiles = Directory.GetFiles(_databasePath, "*.xml");

            foreach (var file in xmlFiles)
            {
                try
                {
                    var characterId = Path.GetFileNameWithoutExtension(file);
                    var character = GetById(characterId);
                    if (character != null)
                    {
                        characters.Add(character);
                    }
                }
                catch
                {
                    // Skip invalid files
                }
            }

            return characters;
        }

        public void Save(Character character)
        {
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            if (string.IsNullOrWhiteSpace(character.ID))
                throw new InvalidOperationException("Character ID cannot be null or empty");

            // Use existing Helper method for backward compatibility
            Helper.saveSettingsToXML(character);
        }

        public void Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            var filePath = Path.Combine(_databasePath, $"{id}.xml");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public bool Exists(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            var filePath = Path.Combine(_databasePath, $"{id}.xml");
            return File.Exists(filePath);
        }

        public IEnumerable<Character> GetByGroup(string group)
        {
            if (string.IsNullOrWhiteSpace(group))
                return Enumerable.Empty<Character>();

            return GetAll().Where(c => c.Group == group);
        }
    }
}
