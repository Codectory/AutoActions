//using AutoHDR.ProjectResources;
//using CodectoryCore.UI.Wpf;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Text;
//using System.Threading.Tasks;

//namespace AutoHDR.Profiles.Actions
//{
//    [JsonObject(MemberSerialization.OptIn)]
//    public class ApplicationAction : ProfileActionBase
//    {
//        public override string ActionTypeName => ProjectResources.ProjectLocales.ApplicationAction;


//        private bool _restartApplication = false;

//        [JsonProperty]
//        public bool RestartApplication { get => _restartApplication; set { _restartApplication = value; OnPropertyChanged(); } }


//        public override string ActionDescription => $"{ProjectLocales.RestartProccessOnFirstOccurence}: {(RestartApplication ? ProjectLocales.Yes : ProjectLocales.No)}";

//        public ApplicationAction()
//        {
//        }

//        public override ActionEndResult RunAction(params object[] parameter)
//        {
//            try
//            {
//                if (RestartApplication)
//                    ((ApplicationItem)parameter[0]).Restart();
//                return new ActionEndResult(true);
//            }
//            catch (Exception ex)
//            {
//                return new ActionEndResult(false, ex.Message, ex);
//            }
//        }

//        public override string ToString()
//        {
//            return ProjectLocales.ApplicationAction;
//        }
//    }
//}
