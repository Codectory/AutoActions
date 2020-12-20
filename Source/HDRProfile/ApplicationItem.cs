using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace HDRProfile
{
    public class ApplicationItem : BaseViewModel
    {
        private string displayName;
        private string _applicationFilePath;
        private string _applicationName;
        private System.Drawing.Bitmap icon = null;

        public string DisplayName { get => displayName; set { displayName = value; OnPropertyChanged(); } }
        public string ApplicationName { get => _applicationName; set { _applicationName = value; OnPropertyChanged(); } }
        public string ApplicationFilePath { get => _applicationFilePath; set { _applicationFilePath = value;  try { Icon = System.Drawing.Icon.ExtractAssociatedIcon(value).ToBitmap(); } catch { } OnPropertyChanged(); } }
        
        [XmlIgnore]
        public Bitmap Icon { get => icon; set { icon = value; OnPropertyChanged(); } }
        private ApplicationItem()
        {

        }
        public ApplicationItem(string displayName, string applicationFilePath)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            ApplicationFilePath = applicationFilePath ?? throw new ArgumentNullException(nameof(applicationFilePath));
            ApplicationName = new FileInfo(ApplicationFilePath).Name.Replace(".exe", "");
        }

    }
}
