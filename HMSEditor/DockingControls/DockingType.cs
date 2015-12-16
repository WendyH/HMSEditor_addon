using System;
using System.Collections.Generic;
using System.Text;

using Darwen.Windows.Forms.General;

namespace Darwen.Windows.Forms.Controls.Docking
{
    public enum DockingType
    {
        Floating,
        Left,
        Right,
        Top,
        Bottom
    }

    public static class DockingTypeConverter
    {
        public static Direction ToDirection(DockingType type)
        {
            switch (type)
            {
                case DockingType.Floating:
                    return Direction.None;
                case DockingType.Left:
                    return Direction.Left;                    
                case DockingType.Right:
                    return Direction.Right;                    
                case DockingType.Top:
                    return Direction.Up;                    
                case DockingType.Bottom:
                    return Direction.Down;                    
                default:
                    return Direction.None;                   
            }
        }

        public static DockingType ToDockingType(Direction direction)
        {
            switch (direction)
            {
                case Direction.None:
                    return DockingType.Floating;
                case Direction.Left:
                    return DockingType.Left;
                case Direction.Right:
                    return DockingType.Right;
                case Direction.Up:
                    return DockingType.Top;
                case Direction.Down:
                    return DockingType.Bottom;
                default:
                    return DockingType.Floating;
            }
        }
    }
}
