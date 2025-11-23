using System;

namespace AutoVPT.Domain
{
    /// <summary>
    /// Core identity and immutable characteristics of a character.
    /// Represents "who" the character is.
    /// </summary>
    [Serializable]
    public class CharacterIdentity
    {
        /// <summary>
        /// Unique character identifier (used as window title and database key)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Game URL for this character
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Group identifier for party operations
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// VIP level of the character
        /// </summary>
        public int VipLevel { get; set; }

        /// <summary>
        /// Whether this character uses Chinese server (affects resource paths)
        /// </summary>
        public bool IsChinese { get; set; }

        /// <summary>
        /// Character position index in UI
        /// </summary>
        public int Position { get; set; } = 1;

        /// <summary>
        /// Whether to increase FPS for this character
        /// </summary>
        public bool IncreaseFps { get; set; }

        public CharacterIdentity()
        {
        }

        public CharacterIdentity(string id, string link)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Link = link;
        }

        public override string ToString()
        {
            return $"Character[{Id}] Group:{Group ?? "None"}";
        }
    }
}
