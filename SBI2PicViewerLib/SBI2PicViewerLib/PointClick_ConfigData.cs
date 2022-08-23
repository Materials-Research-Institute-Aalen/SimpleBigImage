using System;
using System.Xml;
using System.Windows.Forms;
using Scripting;


namespace ZiAPointClickNavigation
{
    class PointClick_ConfigData
    {
        //Für Test am Laptop
        //public static string strConfigPath = @"C:\Programmentwicklung\C#\ZiAGeoImaging_Repository\GeoImaging_Configuration\";
        public static string strConfigPath = @"C:\PointClickNavigation\PointClickNavigation_Configuration\";

        //Documents
        public static string strPathImages;

        //Navigation
        public static int intStepWidthZoom;
        public static int intStepWidthMovement;
        public static bool blnNavigationControl = true;

        //Colors
        public static System.Drawing.Color colorFiducial = System.Drawing.Color.Red;
        public static System.Drawing.Color colorObject = System.Drawing.Color.Blue;
        
        //Default values
        public static readonly int intDefaultStepWidthZoom = 20;
        public static readonly int intDefaultStepWidthMovement = 10;

        public static System.Drawing.Color stringToColor(String strInput)
        {
            string[] astrColors = strInput.Split('-');
            int r, g, b;
            if (astrColors.Length > 2)
            {
                r = Convert.ToInt16(astrColors[0]);
                g = Convert.ToInt16(astrColors[1]);
                b = Convert.ToInt16(astrColors[2]);
                System.Drawing.Color c = System.Drawing.Color.FromArgb(r, g, b);
                return c;
            }

            return System.Drawing.Color.Green;
        }

        public static string colorToString(System.Drawing.Color c)
        {
            return c.R.ToString() + "-" + c.G.ToString() + "-" + c.B.ToString();
        }

        public static void readInConfigFile()
        {
            string strLastNode = string.Empty;

            //create ConfigPath
            if (System.IO.Directory.Exists(PointClick_ConfigData.strConfigPath) == false)
            {
                System.IO.Directory.CreateDirectory(PointClick_ConfigData.strConfigPath);
            }

            string strFileConfig = PointClick_ConfigData.strConfigPath + @"PointClickNavigation_Configuration.xml";
            FileSystemObject fileSysObj = new FileSystemObject();

            //check if configfile exists
            if (fileSysObj.FileExists(strFileConfig))
            {

                XmlReader reader = XmlReader.Create(strFileConfig);

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Text:

                            switch (strLastNode)
                            {
                                case "Path_ImageOpen":
                                    {
                                        PointClick_ConfigData.strPathImages = reader.Value;
                                        strLastNode = "";
                                        break;
                                    }
                                case "Color_FiducialPoints":
                                    {
                                        colorFiducial = stringToColor(reader.Value);
                                        
                                        //PointClick_ConfigData.strPathFiducialPoints = reader.Value;
                                        strLastNode = "";
                                        break;
                                    }
                                case "Color_ObjectList":
                                    {
                                        colorObject = stringToColor(reader.Value);

                                        //PointClick_ConfigData.strPathObjectList = reader.Value;
                                        strLastNode = "";
                                        break;
                                    }
                                case "Navigation_Step_Zoom":
                                    {
                                        PointClick_ConfigData.intStepWidthZoom = Convert.ToInt32(reader.Value);
                                        strLastNode = "";
                                        break;
                                    }

                                case "Navigation_Step_Movement":
                                    {
                                        PointClick_ConfigData.intStepWidthMovement = Convert.ToInt32(reader.Value);
                                        strLastNode = "";
                                        break;
                                    }

                                case "Navigation_UseControl":
                                    {
                                        PointClick_ConfigData.blnNavigationControl = Convert.ToBoolean(reader.Value);
                                        strLastNode = "";
                                        break;
                                    }

                            }
                            break;

                        case XmlNodeType.Element:
                            if (!reader.IsEmptyElement)
                            {
                                switch (reader.Name)
                                {
                                    case "Path_ImageOpen":
                                        {
                                            strLastNode = "Path_ImageOpen";
                                            break;
                                        }
                                    case "Color_FiducialPoints":
                                        {
                                            strLastNode = "Color_FiducialPoints";
                                            break;
                                        }
                                    case "Color_ObjectList":
                                        {
                                            strLastNode = "Color_ObjectList";
                                            break;
                                        }

                                    case "Navigation_Step_Zoom":
                                        {
                                            strLastNode = "Navigation_Step_Zoom";
                                            break;
                                        }
                                    case "Navigation_Step_Movement":
                                        {
                                            strLastNode = "Navigation_Step_Movement";
                                            break;
                                        }
                                    case "Navigation_UseControl":
                                        {
                                            strLastNode = "Navigation_UseControl";
                                            break;
                                        }

                                }
                            }
                            break;
                    }

                }

                reader.Close();
            }
            else
            {
                //Hier auch: Wenn keine Konfig vorhanden ist, ist das kein Fehler.
                //Zumindest sollte er nicht erwähnt werden.
                //MessageBox.Show("Konfigurationsdatei nicht vorhanden!");
                return;
            }
        }

        public static void writeConfigFile()
        {

            string strFileConfig = PointClick_ConfigData.strConfigPath + @"PointClickNavigation_Configuration.xml";
            FileSystemObject fileSysObj = new FileSystemObject();

            //check if configfile exists
            if (fileSysObj.FileExists(strFileConfig))
            {
                //save backup
                string strFileConfig_backup = PointClick_ConfigData.strConfigPath + @"PointClickNavigation_Configuration_backup.xml";
                if (fileSysObj.FileExists(strFileConfig_backup) == true)
                    fileSysObj.DeleteFile(strFileConfig_backup);

                try
                {
                    fileSysObj.CopyFile(strFileConfig, strFileConfig_backup);
                }

                catch
                {
                    MessageBox.Show("Creating backup file of configuration wasn't successful!");
                    return;
                }

            }

            //write configfile
            //if already exists -> delete
            if (fileSysObj.FileExists(strFileConfig))
            {
                try
                {
                    fileSysObj.DeleteFile(strFileConfig);
                }
                catch
                {
                    //MessageBox.Show("Datei kann nicht gelöscht werden!");
                    MessageBox.Show("File can't be deleted!");
                }
            }

            //create
            XmlWriter writer = XmlWriter.Create(strFileConfig);

            writer.WriteStartDocument();
            writer.WriteStartElement("Configuration");
            writer.WriteStartElement("Images");
            writer.WriteElementString("Path_ImageOpen", PointClick_ConfigData.strPathImages);
            writer.WriteElementString("Color_FiducialPoints", PointClick_ConfigData.colorToString(PointClick_ConfigData.colorFiducial));
            writer.WriteElementString("Color_ObjectList", PointClick_ConfigData.colorToString(PointClick_ConfigData.colorObject));
            writer.WriteEndElement();
            writer.WriteStartElement("Navigation");
            writer.WriteElementString("Navigation_Step_Zoom", PointClick_ConfigData.intStepWidthZoom.ToString());
            writer.WriteElementString("Navigation_Step_Movement", PointClick_ConfigData.intStepWidthMovement.ToString());
            //"boolescher String" darf nur kleingeschrieben abgespeichert werden, sonst wird er vom Reader nicht als boolean erkannt!
            writer.WriteElementString("Navigation_UseControl", PointClick_ConfigData.blnNavigationControl.ToString().ToLower());
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
        }

    }
}
