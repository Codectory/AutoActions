using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace AutoHDR.Theming
{
    public class ThemeResourceDirectory : ResourceDictionary
    {
        private Uri _darkSource;
        private Uri _lightSource;

        public Uri LightResource
        {
            get { return _lightSource; }
            set
            {
                _lightSource = value;
                UpdateSource();
            }
        }
        public Uri DarkResource
        {
            get { return _darkSource; }
            set
            {
                _darkSource = value;
                UpdateSource();
            }
        }

        private void UpdateSource()
        {
            var val = App.Theme == Theme.Light ? LightResource : DarkResource;
            if (val != null && base.Source != val)
                base.Source = val;
        }
    }
}
