using System;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using System.Windows.Forms;
using Darwen.Windows.Forms.General;

namespace Darwen.Windows.Forms.Controls.Docking.Serialization
{
    public static class DockingControlsPersister
    {
        private const string Filename = "DockingControls.xml";

        static public string GetXml(DockingManagerControl manager) {
            using (var stream = new MemoryStream()) {
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8)) {
                    DockingControlsPersister.Serialize(manager, streamWriter);
                    return Encoding.UTF8.GetString(stream.GetBuffer());
                }
            }
        }

        /// <summary>
        /// Saves to local directory of the exe
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="filename">The name of the file (not the path of the file)</param>
        static public void Serialize(DockingManagerControl manager, string filename)
        {
            string filePath = GetPathName(filename);

            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                DockingControlsPersister.Serialize(manager, writer);
            }
        }

        static public void Serialize(DockingManagerControl manager)
        {            
            Serialize(manager, GetFilePath());
        }

        static public void Serialize(DockingManagerControl manager, StreamWriter stream)
        {
            DockingManagerControlData data = new DockingManagerControlData(manager);

            XmlSerializer serializer = new XmlSerializer(typeof(DockingManagerControlData));
            serializer.Serialize(stream, data);            
        }

        static public void Deserialize(DockingManagerControl manager)
        {
            string filepath = GetFilePath();

            if (File.Exists(filepath))
            {
                Deserialize(manager, filepath);
            }
        }

        static public void Deserialize(DockingManagerControl manager, string filename)
        {
            if (!File.Exists(filename)) return;
            string filePath = GetPathName(filename);

            using (StreamReader reader = new StreamReader(filename))
            {
                DockingControlsPersister.Deserialize(manager, reader);
            }
        }

        static public void Deserialize(DockingManagerControl manager, StreamReader reader)
        {
            manager.SuspendLayout();

            XmlSerializer serializer = new XmlSerializer(typeof(DockingManagerControlData));

            DockingManagerControlData dockingManagerControlData =
                serializer.Deserialize(reader) as DockingManagerControlData;

            CreatePanels(manager, dockingManagerControlData);

            IEnumerator dataEnumerator = dockingManagerControlData.DockingControls.GetEnumerator();
            IEnumerator<IDockingControl> controlEnumerator = manager.DockingControls.GetEnumerator();

            DeserializeControls(dataEnumerator, controlEnumerator);

            DeseriailizeDockingControlData(manager.Panels[DockingType.Left], dockingManagerControlData.LeftDockingContainerControlData);
            DeseriailizeDockingControlData(manager.Panels[DockingType.Right], dockingManagerControlData.RightDockingContainerControlData);
            DeseriailizeDockingControlData(manager.Panels[DockingType.Top], dockingManagerControlData.TopDockingContainerControlData);
            DeseriailizeDockingControlData(manager.Panels[DockingType.Bottom], dockingManagerControlData.BottomDockingContainerControlData);

            ClearEmptyPanels(manager.Panels[DockingType.Left]);
            ClearEmptyPanels(manager.Panels[DockingType.Right]);
            ClearEmptyPanels(manager.Panels[DockingType.Top]);
            ClearEmptyPanels(manager.Panels[DockingType.Bottom]);

            SetDockedDimensions(dataEnumerator, controlEnumerator);

            manager.ResumeLayout(true);
        }

        private static void DeserializeControls(IEnumerator dataEnumerator, IEnumerator<IDockingControl> controlEnumerator)
        {
            while (dataEnumerator.MoveNext() && controlEnumerator.MoveNext())
            {
                DockingControl control = controlEnumerator.Current as DockingControl;
                DockingControlData data = dataEnumerator.Current as DockingControlData;

                control.Cancelled = data.Cancelled;

                switch (data.DockingType)
                {
                    case DockingType.Floating:
                        control.FloatControl(data.FloatingBounds);
                        break;
                    case DockingType.Left:
                    case DockingType.Right:
                    case DockingType.Top:
                    case DockingType.Bottom:
                        control.DockControl(data.PanelIndex, data.DockingIndex, data.DockingType);
                        control.AutoHide = data.AutoHide;
                        break;
                    default:
                        break;
                }                                
            }
        }

        private static void SetDockedDimensions(IEnumerator dataEnumerator, IEnumerator<IDockingControl> controlEnumerator)
        {
            dataEnumerator.Reset();
            controlEnumerator.Reset();

            while (dataEnumerator.MoveNext() && controlEnumerator.MoveNext())
            {
                DockingControl control = controlEnumerator.Current as DockingControl;
                DockingControlData data = dataEnumerator.Current as DockingControlData;

                switch (data.DockingType)
                {
                    case DockingType.Left:
                    case DockingType.Right:
                    case DockingType.Top:
                    case DockingType.Bottom:
                        control.DockedDimension = data.DockedDimension;
                        break;

                    default:
                        break;
                }
            }
        }

        static private void ClearEmptyPanels(IDockingPanelCollection collection)
        {
            DockControlContainerCollection panels = collection as DockControlContainerCollection;

            for (int index = 0; panels.Count > 1 && index < panels.Count; index += 1)
            {
                DockControlContainer container = panels[index] as DockControlContainer;

                if (container.DockedControlList.Count == 0)
                {
                    panels.Remove(container);
                    index -= 1;
                }
                else if (container.DockedControlList.GetVisibleDockedControlCount() == 0)
                {
                    container.Visible = true;
                    container.Visible = false;
                }
            }
        }

        static private void CreatePanels(DockingManagerControl manager, DockingManagerControlData dockingManagerControlData)
        {
            Dictionary<DockingType, int> mapDockingTypeToCount = new Dictionary<DockingType, int>();

            foreach (DockingControlData data in dockingManagerControlData.DockingControls)
            {
                if (data.DockingType != DockingType.Floating)
                {
                    if (mapDockingTypeToCount.ContainsKey(data.DockingType))
                    {
                        mapDockingTypeToCount[data.DockingType] =
                            Math.Max(mapDockingTypeToCount[data.DockingType], data.PanelIndex);
                    }
                    else
                    {
                        mapDockingTypeToCount.Add(data.DockingType, data.PanelIndex);
                    }
                }
            }

            foreach (DockingType dockingType in mapDockingTypeToCount.Keys)
            {
                int panels = mapDockingTypeToCount[dockingType];

                for (int index = 0; index <= panels; index += 1)
                {
                    IDockingPanelCollection panel = manager.Panels[dockingType];

                    if (panel.Count < (index + 1))
                    {
                        panel.InsertPanel(index);
                    }                     
                }
            }
        }

        static private void DeseriailizeDockingControlData(IDockingPanelCollection panels, DockingContainerControlData[] data)
        {
            int index = 0;

            while (panels.Count < data.Length)
            {
                panels.InsertPanel(panels.Count);
            }

            for (; index < data.Length; index += 1)
            {
                data[index].Deserialize(panels[index]);
            }
        }

        static private string GetPathName(string filename)
        {
            string directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string filenamePath = Path.GetDirectoryName(filename);
            string filePath = string.Empty;

            if (filenamePath == string.Empty)
            {
                filePath = Path.Combine(directory, filename);
            }
            else
            {
                filePath = filename;
            }

            return filePath;
        }

        static private string GetFilePath()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Filename);
        }
    }
}
