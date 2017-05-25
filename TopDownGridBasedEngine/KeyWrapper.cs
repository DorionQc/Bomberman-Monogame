/************************
 * Samuel Goulet
 * Novembre 2016
 * Structure KeyWrapper, projet final Comm
 ***********************/

using System;


namespace TopDownGridBasedEngine
{
    /// <summary>
    /// États possibles du clavier et de la souris
    /// </summary>
    [Flags]
    public enum KeyState
    {
        None = 0,
        Up = 1,
        Left = 2,
        Down = 4,
        Right = 8,
        LeftMouse = 16,
        RightMouse = 32,
        Space = 64
    };

    /// <summary>
    /// Structure contenant les états du clavier et de la souris
    /// </summary>
    public struct KeyWrapper
    {
        public KeyState State;
        public int MouseX;
        public int MouseY;
        private bool _rememberSpace;
        public KeyWrapper(KeyState state, int x, int y)
        {
            State = state;
            MouseX = x;
            MouseY = y;
            _rememberSpace = false;
        }

        public void ToggleSpace(bool isSpaceDown)
        {
            if (isSpaceDown && !_rememberSpace)
            {
                State |= KeyState.Space;
                _rememberSpace = true;
            }
            else if (!isSpaceDown && _rememberSpace)
            {
                State &= ~KeyState.Space;
                _rememberSpace = false;
            }
        }
        
        public void ResetSpace() => State &= ~KeyState.Space;
    }
}
