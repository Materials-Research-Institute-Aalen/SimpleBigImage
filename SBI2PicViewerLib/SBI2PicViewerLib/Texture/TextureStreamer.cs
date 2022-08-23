using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;


namespace SBI2PicViewerLib
{
    class TextureStreamer
    {
        TexturePiece[, ,] textures;
        int _iXSize;
        int _iYSize;
        int _iZSize;

        private int _iXStart;
        private int _iYStart;
        private int _iZStart;
        private int _iXEnd;
        private int _iYEnd;
        private int _iZEnd;
         
        private void writeXML(string strXMLFile, int x, int y, int z)
        {
            XmlWriter xmlWriter = XmlWriter.Create(strXMLFile);

            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("size");
            xmlWriter.WriteAttributeString("x", x.ToString());
            xmlWriter.WriteAttributeString("y", y.ToString());
            xmlWriter.WriteAttributeString("z", z.ToString());

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        private void readXML(string strXMLFile)
        {
            XmlTextReader reader = new XmlTextReader(strXMLFile);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        while (reader.MoveToNextAttribute())
                        { // Read the attributes.
                            switch (reader.Name)
                            {
                                case "x":
                                    _iXSize = Convert.ToInt32(reader.Value);
                                    break;
                                case "y":
                                    _iYSize = Convert.ToInt32(reader.Value);
                                    break;
                                case "z":
                                    _iZSize = Convert.ToInt32(reader.Value);
                                    break;
                            }
                        }
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        break;
                }
            }

        }

        public TextureStreamer(string strXMLFile)
        {
            readXML(strXMLFile);
            OpenCVVariables.strFolder = System.IO.Path.GetDirectoryName(strXMLFile) + @"/";


            if (_iXSize < 1)
            {
                _iXSize = 1;
            }
            if (_iYSize < 1)
            {
                _iYSize = 1;
            }
            if (_iZSize < 1)
            {
                _iZSize = 1;
            }
            textures = new TexturePiece[_iXSize, _iYSize, _iZSize];

            for (int x = 0; x < _iXSize; x++)
            {
                for (int y = 0; y < _iYSize; y++)
                {
                    for (int z = 0; z < _iZSize; z++)
                    {
                        if (TexturePiece.checkPieceExists(x, y, z))
                        {
                            textures[x, y, z] = new TexturePiece(x, y, z);
                        }
                        else
                        {
                            textures[x, y, z] = null;
                        }
                    }
                }
            }
        }

        public TextureStreamer(int xSize, int ySize, int depth)
        {
            writeXML(OpenCVVariables.strFolder + @"/data.xml", xSize, ySize, depth);
            textures = new TexturePiece[xSize, ySize, depth];
            _iXSize = xSize;
            _iYSize = ySize;
            _iZSize = depth;
        }

        public void addTexture(TexturePiece p)
        {
            textures[p.x, p.y, p.z] = p;
        }

        public void addTexture(TexturePiece p, int xPos, int yPos, int zPos)
        {
            textures[xPos, yPos, zPos] = p;
        }

        public void fillEmpty(int iDepth)
        {
            for (int x = 0; x < _iXSize; x++)
            {
                for (int y = 0; y < _iYSize; y++)
                {
                    if (textures[x, y, iDepth] == null)
                    {
                        textures[x, y, iDepth] = new TexturePiece(x, y, iDepth);
                    }
                }
            }
        }

        public TexturePiece[,,] getList()
        {
            return textures;
        }

        
        private void updateUsed()
        {
            foreach (TexturePiece p in textures)
            {
                if (p != null)
                {
                    p.blnActive = false;
                }
            }

            for (int x = _iXStart; x < _iXEnd; x++)
            {
                for (int y = _iYStart; y < _iYEnd; y++)
                {
                    for (int z = _iZStart; z < _iZEnd; z++)
                    {
                        TexturePiece p = textures[x, y, z];
                        if (p != null)
                        {
                            p.blnActive = true;
                        }
                    }
                }
            }
            
        }

        public void checkLoaded()
        {
            updateUsed();

            foreach (TexturePiece p in textures)
            {
                if (p != null)
                {
                    p.checkLoaded();
                }
            }
        }

        public void usedRectangle(float startX, float endX, float startY, float endY, float zoom)
        {
            int iStartX =   (int)(startX / OpenCVVariables.Schrittweite);
            int iEndX =     (int)(endX / OpenCVVariables.Schrittweite);
            int iStartY = (int)(startY / OpenCVVariables.Schrittweite);
            int iEndY = (int)(endY / OpenCVVariables.Schrittweite);
            int iCurrentDepth = (int)(1.0f / zoom / 2);
            usedRectangle(iStartX, iEndX, iStartY, iEndY, iCurrentDepth);
        }

        public void usedRectangle(int iStartX, int iEndX, int iStartY, int iEndY, int iCurrentDepth)
        {
            int iWidth = iEndX - iStartX;
            int iHeight = iEndY - iStartY;

            _iXStart = iStartX - iWidth / 3;
            _iXStart = (_iXStart < 1 ? 0 : _iXStart);
            _iXEnd = iEndX + iWidth / 3;
            _iXEnd = (_iXEnd > _iXSize ? _iXSize : _iXEnd);

            _iYStart = iStartY - iWidth / 3;
            _iYStart = (_iYStart < 1 ? 0 : _iYStart);
            _iYEnd = iEndY + iWidth / 3;
            _iYEnd = (_iYEnd > _iYSize ? _iYSize : _iYEnd);

            _iZStart = iCurrentDepth - 0;
            _iZStart = (_iZStart < 1 ? 0 : _iZStart);
            _iZEnd = iCurrentDepth + 1;
            _iZEnd = (_iZEnd > _iZSize ? _iZSize : _iZEnd);
        }
    }
}
