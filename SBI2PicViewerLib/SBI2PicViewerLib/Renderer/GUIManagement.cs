using SBI2PicViewerLib.Geom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBI2PicViewerLib.Renderer
{
    class GUIManagement
    {
        List<GUIElementInterface> _renderList;

        public GUIManagement(List<GUIElementInterface> elements)
        {
            _renderList = elements;
        }

        public void setGUI(List<GUIElementInterface> elements)
        {
            _renderList = elements;
        }

        public void render(Camera cam)
        {
            if (_renderList != null)
            {
                foreach (GUIElementInterface elm in _renderList)
                {
                    elm.render(cam);
                }
            }
        }
    }
}
