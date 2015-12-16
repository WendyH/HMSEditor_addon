using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Darwen.Windows.Forms.Controls.Docking
{
    internal static class DockControlHelpers
    {
        static public DockControlContainer GetDropContainer(DockingManagerControl manager, Point screenDropPoint, int dockingBarSize)
        {
            Point managerLocalDropPoint = manager.PointToClient(screenDropPoint);
            DockControlContainer container = manager.GetContainerAtPoint(managerLocalDropPoint);

            if (container == null)
            {
                Rectangle managerClientBounds = manager.DockingBounds;

                if (!HasVisibleContainer(manager.Panels[DockingType.Left]))
                {
                    if (managerLocalDropPoint.X < managerClientBounds.Left + dockingBarSize)
                    {
                        container = manager.Panels[DockingType.Left][0] as DockControlContainer;
                    }
                }

                if (!HasVisibleContainer(manager.Panels[DockingType.Right]))
                {
                    if (managerLocalDropPoint.X > managerClientBounds.Right - dockingBarSize)
                    {
                        container = manager.Panels[DockingType.Right][0] as DockControlContainer;
                    }
                }

                if (!HasVisibleContainer(manager.Panels[DockingType.Top]))
                {
                    if (managerLocalDropPoint.Y < managerClientBounds.Top + dockingBarSize)
                    {
                        container = manager.Panels[DockingType.Top][0] as DockControlContainer;
                    }
                }

                if (!HasVisibleContainer(manager.Panels[DockingType.Bottom]))
                {
                    if (managerLocalDropPoint.Y > managerClientBounds.Bottom - dockingBarSize)
                    {
                        container = manager.Panels[DockingType.Bottom][0] as DockControlContainer;
                    }
                }
            }

            return container;
        }

        static public Rectangle GetDropXorDragRect(DockingManagerControl manager, Point screenDropPoint, int dockingBarSize)
        {
            Point managerLocalDropPoint = manager.PointToClient(screenDropPoint);
            Rectangle managerClientBounds = manager.DockingBounds;
            Rectangle dropRect = Rectangle.Empty;

            if (!HasVisibleContainer(manager.Panels[DockingType.Left]))
            {
                if (managerLocalDropPoint.X < managerClientBounds.Left + dockingBarSize)
                {
                    dropRect = managerClientBounds;
                    dropRect.Width = dockingBarSize;
                }
            }

            if (!HasVisibleContainer(manager.Panels[DockingType.Right]))
            {
                if (managerLocalDropPoint.X > managerClientBounds.Right - dockingBarSize)
                {
                    dropRect = managerClientBounds;
                    dropRect.X = dropRect.Right - dockingBarSize;
                    dropRect.Width = dockingBarSize;
                }
            }

            if (!HasVisibleContainer(manager.Panels[DockingType.Top]))
            {
                if (managerLocalDropPoint.Y < managerClientBounds.Top + dockingBarSize)
                {
                    dropRect = managerClientBounds;
                    dropRect.Height = dockingBarSize;
                }
            }

            if (!HasVisibleContainer(manager.Panels[DockingType.Bottom]))
            {
                if (managerLocalDropPoint.Y > managerClientBounds.Bottom - dockingBarSize)
                {
                    dropRect = managerClientBounds;
                    dropRect.Y = dropRect.Bottom - dockingBarSize;
                    dropRect.Height = dockingBarSize;
                }
            }

            return dropRect;
        }

        static private bool HasVisibleContainer(IDockingPanelCollection panels)
        {
            foreach (DockControlContainer control in panels)
            {
                if (control.Visible)
                {
                    return true;
                }
            }

            return false;
        }

        static public DockControlContainer CreateNewContainerIfNecessary(DockingManagerControl manager, DockControlContainer container, Point screenDropPoint, int dockingBarSize)
        {
            DockControlContainer result = container;
                
            if (container.Visible)
            {
                Point managerLocalDropPoint = manager.PointToClient(screenDropPoint);
                DockControlContainerCollection containerCollection = GetDockControlContainerCollection(manager, container);
                int index = containerCollection.IndexOf(container);

                switch (container.Dock)
                {
                    case DockStyle.Top:
                        if (managerLocalDropPoint.Y < container.Top + dockingBarSize)
                        {
                            result = containerCollection.InsertPanel(index + 1) as DockControlContainer;
                        }
                        else if (managerLocalDropPoint.Y > container.Bottom - dockingBarSize)
                        {
                            result = containerCollection.InsertPanel(index) as DockControlContainer;
                        }

                        break;

                    case DockStyle.Bottom:

                        if (managerLocalDropPoint.Y < container.Top + dockingBarSize)
                        {
                            result = containerCollection.InsertPanel(index) as DockControlContainer;
                        }
                        else if (managerLocalDropPoint.Y > container.Bottom - dockingBarSize)
                        {
                            result = containerCollection.InsertPanel(index + 1) as DockControlContainer;
                        }

                        break;

                    case DockStyle.Left:

                        if (managerLocalDropPoint.X < container.Left + dockingBarSize)
                        {
                            result = containerCollection.InsertPanel(index + 1) as DockControlContainer;
                        }
                        else if (managerLocalDropPoint.X > container.Right - dockingBarSize)
                        {
                            result = containerCollection.InsertPanel(index) as DockControlContainer;
                        }

                        break;

                    case DockStyle.Right:

                        if (managerLocalDropPoint.X < container.Left + dockingBarSize)
                        {
                            result = containerCollection.InsertPanel(index) as DockControlContainer;
                        }
                        else if (managerLocalDropPoint.X > container.Right - dockingBarSize)
                        {
                            result = containerCollection.InsertPanel(index + 1) as DockControlContainer;
                        }

                        break;

                    default:
                        throw new InvalidOperationException("Invalid dock style");
                }
            }            

            return result;
        }

        static public int GetDockedDimension(Control control, DockStyle style)
        {
            switch (style)
            {
                case DockStyle.Top:
                case DockStyle.Bottom:
                    return control.Height;
                case DockStyle.Left:
                case DockStyle.Right:
                    return control.Width;
                default:
                    break;
            }

            return 0;
        }

        static public void SetDockedDimension(Control control, int dimension)
        {
            switch (control.Dock)
            {
                case DockStyle.Top:
                case DockStyle.Bottom:
                    control.Height = dimension;
                    break;
                case DockStyle.Left:
                case DockStyle.Right:
                    control.Width = dimension;
                    break;
                default:
                    break;
            }
        }

        static private DockControlContainerCollection GetDockControlContainerCollection(DockingManagerControl manager, DockControlContainer container)
        {
            return DockControlContainerCollectionHelpers.GetCollection(manager, container.Dock);            
        }        
    }
}
