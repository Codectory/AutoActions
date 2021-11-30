﻿//using AutoHDR.ProjectResources;
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
//        public override string ActionTypeName => ProjectResources.Locale_Texts.ApplicationAction;


//        private bool _restartApplication = false;

//        [JsonProperty]
//        public bool RestartApplication { get => _restartApplication; set { _restartApplication = value; OnPropertyChanged(); } }


//        public override string ActionDescription => $"{Locale_Texts.RestartProccessOnFirstOccurence}: {(RestartApplication ? Locale_Texts.Yes : Locale_Texts.No)}";

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
//            return Locale_Texts.ApplicationAction;
//        }
//    }
//}