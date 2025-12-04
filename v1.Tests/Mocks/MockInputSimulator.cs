using AutoVPT.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IInputSimulator for testing.
    /// Records all input actions for verification.
    /// </summary>
    public class MockInputSimulator : IInputSimulator
    {
        private readonly List<InputAction> _actions = new List<InputAction>();

        /// <summary>
        /// Gets all recorded input actions
        /// </summary>
        public IReadOnlyList<InputAction> Actions => _actions;

        /// <summary>
        /// Clear all recorded actions
        /// </summary>
        public void Reset()
        {
            _actions.Clear();
        }

        /// <summary>
        /// Verify a specific action was performed
        /// </summary>
        public bool HasAction(InputActionType type, int x = -1, int y = -1)
        {
            foreach (var action in _actions)
            {
                if (action.Type == type)
                {
                    if (x >= 0 && y >= 0)
                    {
                        if (action.X == x && action.Y == y)
                            return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get count of specific action type
        /// </summary>
        public int CountActions(InputActionType type)
        {
            int count = 0;
            foreach (var action in _actions)
            {
                if (action.Type == type)
                    count++;
            }
            return count;
        }

        // IInputSimulator implementation

        public Task ClickAsync(Point location, int delayAfterMs = 100)
        {
            _actions.Add(new InputAction(InputActionType.Click, location.X, location.Y));
            return Task.CompletedTask;
        }

        public Task DoubleClickAsync(Point location, int delayAfterMs = 100)
        {
            _actions.Add(new InputAction(InputActionType.DoubleClick, location.X, location.Y));
            return Task.CompletedTask;
        }

        public Task RightClickAsync(Point location, int delayAfterMs = 100)
        {
            _actions.Add(new InputAction(InputActionType.RightClick, location.X, location.Y));
            return Task.CompletedTask;
        }

        public Task SendTextAsync(string text, int delayAfterMs = 100)
        {
            _actions.Add(new InputAction(InputActionType.SendText, text));
            return Task.CompletedTask;
        }

        public Task SendKeyAsync(Keys key, int delayAfterMs = 100)
        {
            _actions.Add(new InputAction(InputActionType.SendKey, key.ToString()));
            return Task.CompletedTask;
        }

        public Task SendKeysAsync(Keys[] keys, int delayBetweenMs = 50)
        {
            foreach (var key in keys)
            {
                _actions.Add(new InputAction(InputActionType.SendKey, key.ToString()));
            }
            return Task.CompletedTask;
        }

        public Task MoveMouseAsync(Point location)
        {
            _actions.Add(new InputAction(InputActionType.MoveMouse, location.X, location.Y));
            return Task.CompletedTask;
        }

        public Task DragAsync(Point from, Point to, int durationMs = 500)
        {
            _actions.Add(new InputAction(InputActionType.Drag, from.X, from.Y, to.X, to.Y));
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Types of input actions
    /// </summary>
    public enum InputActionType
    {
        Click,
        DoubleClick,
        RightClick,
        SendKey,
        SendText,
        Drag,
        Scroll,
        MoveMouse
    }

    /// <summary>
    /// Recorded input action
    /// </summary>
    public class InputAction
    {
        public InputActionType Type { get; }
        public int X { get; }
        public int Y { get; }
        public int X2 { get; }
        public int Y2 { get; }
        public string Key { get; }
        public DateTime Timestamp { get; }

        public InputAction(InputActionType type, int x = 0, int y = 0, int x2 = 0, int y2 = 0)
        {
            Type = type;
            X = x;
            Y = y;
            X2 = x2;
            Y2 = y2;
            Timestamp = DateTime.Now;
        }

        public InputAction(InputActionType type, string key)
        {
            Type = type;
            Key = key;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case InputActionType.Click:
                case InputActionType.DoubleClick:
                case InputActionType.RightClick:
                case InputActionType.MoveMouse:
                    return $"{Type}({X}, {Y})";
                case InputActionType.SendKey:
                case InputActionType.SendText:
                    return $"{Type}(\"{Key}\")";
                case InputActionType.Drag:
                    return $"{Type}({X},{Y} -> {X2},{Y2})";
                default:
                    return Type.ToString();
            }
        }
    }
}
