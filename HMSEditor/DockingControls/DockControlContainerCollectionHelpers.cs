using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Darwen.Windows.Forms.General;

namespace Darwen.Windows.Forms.Controls.Docking
{
    internal static class DockControlContainerCollectionHelpers
    {
        static public DockControlContainerCollection GetCollection(DockingManagerControl manager, DockStyle dockStyle)
        {
            DockControlContainerCollection containerCollection;

            switch (dockStyle)
            {
                case DockStyle.Bottom:
                    containerCollection = manager.Panels[DockingType.Bottom] as DockControlContainerCollection;
                    break;
                case DockStyle.Left:
                    containerCollection = manager.Panels[DockingType.Left] as DockControlContainerCollection;
                    break;
                case DockStyle.Right:
                    containerCollection = manager.Panels[DockingType.Right] as DockControlContainerCollection;
                    break;
                case DockStyle.Top:
                    containerCollection = manager.Panels[DockingType.Top] as DockControlContainerCollection;
                    break;
                default:
                    throw new InvalidOperationException("Invalid dock style");                    
            }

            return containerCollection;
        }

        static public DockControlContainerCollection GetCollection(DockingManagerControl manager, Direction direction)
        {
            DockControlContainerCollection containerCollection;

            switch (direction)
            {
                case Direction.Down:
                    containerCollection = manager.Panels[DockingType.Bottom] as DockControlContainerCollection;
                    break;
                case Direction.Left:
                    containerCollection = manager.Panels[DockingType.Left] as DockControlContainerCollection;
                    break;
                case Direction.Right:
                    containerCollection = manager.Panels[DockingType.Right] as DockControlContainerCollection;
                    break;
                case Direction.Up:
                    containerCollection = manager.Panels[DockingType.Top] as DockControlContainerCollection;
                    break;
                default:
                    throw new InvalidOperationException("Invalid dock style");
            }

            return containerCollection;
        }

        static public DockControlContainerCollection GetCollection(DockingManagerControl manager, DockingType type)
        {
            return manager.Panels[type] as DockControlContainerCollection;
        }
    }
}
