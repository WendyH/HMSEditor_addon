using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Darwen.Windows.Forms.Controls.Docking.Serialization
{
    public class PersistableRectangle
    {
        private Rectangle _rectangle = Rectangle.Empty;
        
        public PersistableRectangle()
        {
        }

        public PersistableRectangle(Rectangle rectangle)
        {
            _rectangle = rectangle;
        }

        [XmlElement]
        public int X
        {
            get
            {
                return _rectangle.X;
            }

            set
            {
                _rectangle.X = value;
            }
        }

        [XmlElement]
        public int Y
        {
            get
            {
                return _rectangle.Y;
            }

            set
            {
                _rectangle.Y = value;
            }
        }

        [XmlElement]
        public int Width
        {
            get
            {
                return _rectangle.Width;
            }

            set
            {
                _rectangle.Width = value;
            }
        }

        [XmlElement]
        public int Height
        {
            get
            {
                return _rectangle.Height;
            }

            set
            {
                _rectangle.Height = value;
            }
        }

        static public implicit operator Rectangle(PersistableRectangle rectangle)
        {
            return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }
    }

    public class DockingControlData
    {
        private DockingType _type = DockingType.Floating;
        private string _title;
        private bool _cancelled;
        private PersistableRectangle _floatingBounds = new PersistableRectangle(Rectangle.Empty);
        private bool _autoHide;
        private int _dockingIndex;
        private int _dockedDimension;

        public DockingControlData()
        {
        }

        public DockingControlData(IDockingControl control)
        {
            DockingControl dockingControl = control as DockingControl;

            if (dockingControl == null)
            {
                throw new ArgumentException("Argument is not a DockingControl");
            }
            else
            {
                _type = dockingControl.DockingType;
                _title = dockingControl.Title;
                _autoHide = dockingControl.AutoHide;
                
                if (_type == DockingType.Floating)
                {
                    _floatingBounds = new PersistableRectangle(dockingControl.FloatingBounds);
                }
                else
                {
                    _dockingIndex = control.DockIndex;
                    _panelIndex = control.PanelIndex;
                    _dockedDimension = control.DockedDimension;
                }

                _cancelled = dockingControl.Cancelled;
            }

            if (dockingControl.AutoHide)
            {
                AutoResizeControl autoResizeControl = dockingControl.Parent as AutoResizeControl;
                _dockedDimension = autoResizeControl.TargetSize;                                        
            }
        }

        [XmlElement]
        public int DockedDimension
        {
            get
            {
                return _dockedDimension;
            }

            set
            {
                _dockedDimension = value;
            }
        }


        [XmlElement]
        public bool Cancelled
        {
            get
            {
                return _cancelled;
            }

            set
            {
                _cancelled = value;
            }
        }

        [XmlElement]
        public DockingType DockingType
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }

        [XmlElement]
        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                _title = value;
            }
        }

        [XmlElement]
        public PersistableRectangle FloatingBounds
        {
            get
            {
                return _floatingBounds;
            }

            set
            {
                _floatingBounds = value;
            }
        }

        [XmlElement]
        public bool AutoHide
        {
            get
            {
                return _autoHide;
            }

            set
            {
                _autoHide = value;
            }
        }

        [XmlElement]
        public int DockingIndex
        {
            get
            {
                return _dockingIndex;
            }

            set
            {
                _dockingIndex = value;
            }
        }

        private int _panelIndex;

        [XmlElement]
        public int PanelIndex
        {
            get
            {
                return _panelIndex;
            }

            set
            {
                _panelIndex = value;
            }
        }
    }

    public class DockingContainerControlData
    {
        private bool _tabbed;
        private int _dimension;

        public DockingContainerControlData()
        {
        }

        public DockingContainerControlData(IDockingPanel panel)
        {
            _tabbed = panel.Tabbed;
            _dimension = panel.Dimension;
        }

        public bool Tabbed
        {
            get
            {
                return _tabbed;
            }

            set
            {
                _tabbed = value;
            }
        }

        public int Dimension
        {
            get
            {
                return _dimension;
            }

            set
            {
                _dimension = value;
            }
        }

        public void Deserialize(IDockingPanel dockingPanel)
        {
            dockingPanel.Tabbed = _tabbed;
            dockingPanel.Dimension = _dimension;
        }
    }

    public class DockingManagerControlData
    {
        private DockingControlData[] _dockingControlsData;
        
        public DockingManagerControlData()
        {
        }

        public DockingManagerControlData(DockingManagerControl manager)
        {
            List<DockingControlData> dataList = new List<DockingControlData>();

            foreach (DockingControl control in manager.DockingControls)
            {
                dataList.Add(new DockingControlData(control));
            }

            _dockingControlsData = dataList.ToArray();

            _leftDockingContainerControlData = CreateDockingControlData(manager.Panels[DockingType.Left]);
            _rightDockingContainerControlData = CreateDockingControlData(manager.Panels[DockingType.Right]);
            _bottomDockingContainerControlData = CreateDockingControlData(manager.Panels[DockingType.Bottom]);
            _topDockingContainerControlData = CreateDockingControlData(manager.Panels[DockingType.Top]);
        }

        private DockingContainerControlData[] CreateDockingControlData(IDockingPanelCollection panelCollection)
        {
            DockingContainerControlData[] data = new DockingContainerControlData[panelCollection.Count];

            int index = 0;

            foreach (IDockingPanel panel in panelCollection)
            {
                data[index++] = new DockingContainerControlData(panel);
            }

            return data;
        }

        [XmlArray]
        public DockingControlData[] DockingControls
        {
            get
            {
                return _dockingControlsData;
            }

            set
            {
                _dockingControlsData = value;
            }
        }

        private DockingContainerControlData [] _leftDockingContainerControlData;

        public DockingContainerControlData [] LeftDockingContainerControlData
        {
            get
            {
                return _leftDockingContainerControlData;
            }

            set
            {
                _leftDockingContainerControlData = value;
            }
        }

        private DockingContainerControlData [] _rightDockingContainerControlData;

        public DockingContainerControlData [] RightDockingContainerControlData
        {
            get
            {
                return _rightDockingContainerControlData;
            }

            set
            {
                _rightDockingContainerControlData = value;
            }
        }

        private DockingContainerControlData [] _topDockingContainerControlData;

        public DockingContainerControlData [] TopDockingContainerControlData
        {
            get
            {
                return _topDockingContainerControlData;
            }

            set
            {
                _topDockingContainerControlData = value;
            }
        }

        private DockingContainerControlData [] _bottomDockingContainerControlData;

        public DockingContainerControlData [] BottomDockingContainerControlData
        {
            get
            {
                return _bottomDockingContainerControlData;
            }

            set
            {
                _bottomDockingContainerControlData = value;
            }
        }
    }

}
