using System.Drawing;

namespace AutoVPT.Infrastructure
{
    /// <summary>
    /// Predefined search regions for common UI areas.
    /// OPTIMIZATION 1: Regional search for 2-3x speedup by limiting search area.
    ///
    /// Usage: Instead of searching entire screen (800x600 = 480k pixels),
    /// search only relevant regions (200x150 = 30k pixels = 16x faster).
    /// </summary>
    public static class SearchRegions
    {
        // Standard game window size (typical Flash Player size)
        public const int GAME_WIDTH = 800;
        public const int GAME_HEIGHT = 600;

        /// <summary>
        /// Full screen search (default, slowest)
        /// </summary>
        public static Rectangle FullScreen => new Rectangle(0, 0, GAME_WIDTH, GAME_HEIGHT);

        /// <summary>
        /// Top-left area (character portrait, buffs, quest indicators)
        /// Expanded to 300x200 to fit wide UI elements like nhiemvuhangngay_check.png (195x28)
        /// ~12.5% of screen area = 8x faster
        /// </summary>
        public static Rectangle TopLeft => new Rectangle(0, 0, 300, 200);

        /// <summary>
        /// Top-right corner (minimap, menu buttons, settings)
        /// ~6% of screen area = 16x faster
        /// </summary>
        public static Rectangle TopRight => new Rectangle(600, 0, 200, 150);

        /// <summary>
        /// Bottom-left corner (chat, party panel)
        /// ~8% of screen area = 12x faster
        /// </summary>
        public static Rectangle BottomLeft => new Rectangle(0, 450, 250, 150);

        /// <summary>
        /// Bottom-right corner (skill bar, inventory shortcuts)
        /// ~8% of screen area = 12x faster
        /// </summary>
        public static Rectangle BottomRight => new Rectangle(550, 450, 250, 150);

        /// <summary>
        /// Center area (dialog boxes, confirmation buttons, main gameplay)
        /// ~25% of screen area = 4x faster
        /// </summary>
        public static Rectangle Center => new Rectangle(200, 150, 400, 300);

        /// <summary>
        /// Top bar (title, menu, system buttons)
        /// ~5% of screen area = 20x faster
        /// </summary>
        public static Rectangle TopBar => new Rectangle(0, 0, 800, 50);

        /// <summary>
        /// Bottom bar (action bar, skills)
        /// Expanded to 100px height for skill buttons
        /// ~16% of screen area = 6x faster
        /// </summary>
        public static Rectangle BottomBar => new Rectangle(0, 500, 800, 100);

        /// <summary>
        /// Left panel (character stats, quests)
        /// ~15% of screen area = 6x faster
        /// </summary>
        public static Rectangle LeftPanel => new Rectangle(0, 0, 200, 600);

        /// <summary>
        /// Right panel (minimap, menu)
        /// ~15% of screen area = 6x faster
        /// </summary>
        public static Rectangle RightPanel => new Rectangle(600, 0, 200, 600);

        /// <summary>
        /// Dialog area (popup dialogs, confirmation windows)
        /// ~33% of screen area = 3x faster
        /// </summary>
        public static Rectangle DialogArea => new Rectangle(150, 100, 500, 400);

        /// <summary>
        /// Main gameplay area (character movement, monsters, NPCs)
        /// ~50% of screen area = 2x faster
        /// </summary>
        public static Rectangle GameplayArea => new Rectangle(100, 50, 600, 500);

        /// <summary>
        /// Get appropriate region for specific UI element type
        /// </summary>
        public static Rectangle GetRegionForElement(string elementType)
        {
            switch (elementType?.ToLower())
            {
                case "minimap":
                case "menu":
                    return TopRight;

                case "portrait":
                case "buff":
                case "quest_indicator":
                    return TopLeft;

                case "chat":
                case "party":
                    return BottomLeft;

                case "skill":
                case "inventory":
                    return BottomRight;

                case "dialog":
                case "confirmation":
                case "popup":
                    return DialogArea;

                case "button_top":
                case "title":
                    return TopBar;

                case "button_bottom":
                case "action_bar":
                    return BottomBar;

                case "gameplay":
                case "character":
                case "monster":
                case "npc":
                    return GameplayArea;

                default:
                    return FullScreen;
            }
        }

        /// <summary>
        /// Create custom region
        /// </summary>
        public static Rectangle CreateRegion(int x, int y, int width, int height)
        {
            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Expand region by margin
        /// </summary>
        public static Rectangle ExpandRegion(Rectangle region, int margin)
        {
            return new Rectangle(
                region.X - margin,
                region.Y - margin,
                region.Width + (margin * 2),
                region.Height + (margin * 2)
            );
        }
    }
}
