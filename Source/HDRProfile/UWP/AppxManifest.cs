using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HDRProfile.UWP
{

    // HINWEIS: Für den generierten Code ist möglicherweise mindestens .NET Framework 4.5 oder .NET Core/Standard 2.0 erforderlich.
    /// <remarks/>

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10")]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "Package", Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10", IsNullable = false)]
    public partial class AppxManifest
    {

        private PackageIdentity identityField;

        private PackageProperties propertiesField;

        private PackageResources resourcesField;

        private PackageDependencies dependenciesField;

        private PackageCapabilities capabilitiesField;

        private PackageApplications applicationsField;

        /// <remarks/>
        public PackageIdentity Identity
        {
            get
            {
                return this.identityField;
            }
            set
            {
                this.identityField = value;
            }
        }

        /// <remarks/>
        public PackageProperties Properties
        {
            get
            {
                return this.propertiesField;
            }
            set
            {
                this.propertiesField = value;
            }
        }

        /// <remarks/>
        public PackageResources Resources
        {
            get
            {
                return this.resourcesField;
            }
            set
            {
                this.resourcesField = value;
            }
        }

        /// <remarks/>
        public PackageDependencies Dependencies
        {
            get
            {
                return this.dependenciesField;
            }
            set
            {
                this.dependenciesField = value;
            }
        }

        /// <remarks/>
        public PackageCapabilities Capabilities
        {
            get
            {
                return this.capabilitiesField;
            }
            set
            {
                this.capabilitiesField = value;
            }
        }

        /// <remarks/>
        public PackageApplications Applications
        {
            get
            {
                return this.applicationsField;
            }
            set
            {
                this.applicationsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10")]
    public partial class PackageIdentity
    {

        private string nameField;

        private string versionField;

        private string publisherField;

        private string processorArchitectureField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Publisher
        {
            get
            {
                return this.publisherField;
            }
            set
            {
                this.publisherField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ProcessorArchitecture
        {
            get
            {
                return this.processorArchitectureField;
            }
            set
            {
                this.processorArchitectureField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10")]
    public partial class PackageProperties
    {

        private object displayNameField;

        private object publisherDisplayNameField;

        private object descriptionField;

        private object logoField;

        /// <remarks/>
        public object DisplayName
        {
            get
            {
                return this.displayNameField;
            }
            set
            {
                this.displayNameField = value;
            }
        }

        /// <remarks/>
        public object PublisherDisplayName
        {
            get
            {
                return this.publisherDisplayNameField;
            }
            set
            {
                this.publisherDisplayNameField = value;
            }
        }

        /// <remarks/>
        public object Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        public object Logo
        {
            get
            {
                return this.logoField;
            }
            set
            {
                this.logoField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10")]
    public partial class PackageResources
    {

        private PackageResourcesResource resourceField;

        /// <remarks/>
        public PackageResourcesResource Resource
        {
            get
            {
                return this.resourceField;
            }
            set
            {
                this.resourceField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10")]
    public partial class PackageResourcesResource
    {

        private string languageField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Language
        {
            get
            {
                return this.languageField;
            }
            set
            {
                this.languageField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10")]
    public partial class PackageDependencies
    {

        private PackageDependenciesTargetDeviceFamily targetDeviceFamilyField;

        /// <remarks/>
        public PackageDependenciesTargetDeviceFamily TargetDeviceFamily
        {
            get
            {
                return this.targetDeviceFamilyField;
            }
            set
            {
                this.targetDeviceFamilyField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10")]
    public partial class PackageDependenciesTargetDeviceFamily
    {

        private string nameField;

        private string minVersionField;

        private string maxVersionTestedField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MinVersion
        {
            get
            {
                return this.minVersionField;
            }
            set
            {
                this.minVersionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MaxVersionTested
        {
            get
            {
                return this.maxVersionTestedField;
            }
            set
            {
                this.maxVersionTestedField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10")]
    public partial class PackageCapabilities
    {

        private Capability capabilityField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabil" +
            "ities")]
        public Capability Capability
        {
            get
            {
                return this.capabilityField;
            }
            set
            {
                this.capabilityField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabil" +
        "ities")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabil" +
        "ities", IsNullable = false)]
    public partial class Capability
    {

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10")]
    public partial class PackageApplications
    {

        private PackageApplicationsApplication applicationField;

        /// <remarks/>
        public PackageApplicationsApplication Application
        {
            get
            {
                return this.applicationField;
            }
            set
            {
                this.applicationField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10")]
    public partial class PackageApplicationsApplication
    {

        private VisualElements visualElementsField;

        private string idField;

        private string executableField;

        private string entryPointField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/appx/manifest/uap/windows10")]
        public VisualElements VisualElements
        {
            get
            {
                return this.visualElementsField;
            }
            set
            {
                this.visualElementsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Executable
        {
            get
            {
                return this.executableField;
            }
            set
            {
                this.executableField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EntryPoint
        {
            get
            {
                return this.entryPointField;
            }
            set
            {
                this.entryPointField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/appx/manifest/uap/windows10")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/appx/manifest/uap/windows10", IsNullable = false)]
    public partial class VisualElements
    {

        private string displayNameField;

        private string descriptionField;

        private string square150x150LogoField;

        private string square44x44LogoField;

        private string backgroundColorField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DisplayName
        {
            get
            {
                return this.displayNameField;
            }
            set
            {
                this.displayNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Square150x150Logo
        {
            get
            {
                return this.square150x150LogoField;
            }
            set
            {
                this.square150x150LogoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Square44x44Logo
        {
            get
            {
                return this.square44x44LogoField;
            }
            set
            {
                this.square44x44LogoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string BackgroundColor
        {
            get
            {
                return this.backgroundColorField;
            }
            set
            {
                this.backgroundColorField = value;
            }
        }
    }


}
