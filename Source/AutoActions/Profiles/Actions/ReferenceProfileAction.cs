using AutoActions.ProjectResources;
using CodectoryCore;
using CodectoryCore.UI.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions.Profiles.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ReferenceProfileAction : ActionBase
    {

        public override bool CanSave => ReferenceProfile != null && IsLoopFree();

        public override string CannotSaveMessage => CanSave ? string.Empty : ProjectLocales.MessageReferenceLoop;
        public DispatchingObservableCollection<Profile> AllProfiles => Globals.Instance.Settings.ApplicationProfiles;
        public override string ActionTypeName => ProjectResources.ProjectLocales.ReferenceProfileAction;


        private Guid _referenceGuid = Guid.Empty;

        [JsonProperty]
        public Guid ReferenceGuid { get => _referenceGuid; set { _referenceGuid = value; OnPropertyChanged(); OnPropertyChanged(nameof(ReferenceProfile)); OnPropertyChanged(nameof(CanSave)); } }


        public Profile ReferenceProfile { get => AllProfiles.FirstOrDefault(p => p.GUID.Equals(ReferenceGuid)); set { ReferenceGuid = value.GUID;  } }


        public override string ActionDescription => $"{ReferenceProfile.Name}";



        public ReferenceProfileAction() : base()
        {

        }

        public override ActionEndResult RunAction(ApplicationChangedType changedType)
        {
            try
            {
               if (ReferenceProfile == null)
                {
                    CallNewLog(new CodectoryCore.Logging.LogEntry(string.Format(ProjectLocales.MessageReferenceProfileNotFound, ReferenceGuid), CodectoryCore.Logging.LogEntryType.Error));
                    return new ActionEndResult(false);
                }
               switch (changedType)
                {
                    case ApplicationChangedType.Started:
                        ReferenceProfile.GetProfileActions(ProfileActionListType.Started).ToList().ForEach(a => a.RunAction(changedType));
                        break;
                    case ApplicationChangedType.Closed:
                        ReferenceProfile.GetProfileActions(ProfileActionListType.Closed).ToList().ForEach(a => a.RunAction(changedType));
                        break;

                    case ApplicationChangedType.GotFocus:
                        ReferenceProfile.GetProfileActions(ProfileActionListType.GotFocus).ToList().ForEach(a => a.RunAction(changedType));
                        break;

                    case ApplicationChangedType.LostFocus:
                        ReferenceProfile.GetProfileActions(ProfileActionListType.LostFocus).ToList().ForEach(a => a.RunAction(changedType));
                        break;

                }
                return new ActionEndResult(true);
            }
            catch (Exception ex)
            {
                CallNewLog(new CodectoryCore.Logging.LogEntry($"{ ex.Message }\r\n{ ex.StackTrace}", CodectoryCore.Logging.LogEntryType.Error));
                return new ActionEndResult(false, ex.Message, ex);
            }
        }

        public bool IsLoopFree()
        {
            List<ProfileChainInfo> profileChains = new List<ProfileChainInfo>();
            foreach (Profile profile in AllProfiles)
            {
                ProfileChainInfo chainInfo = new ProfileChainInfo(profile);
                ListOfProfileActions allActions = new ListOfProfileActions();
                foreach (var a in profile.ApplicationStarted.Where(a => a.GetType().Equals(typeof(ReferenceProfileAction))))
                    chainInfo.ApplicationStartedChilds.AddRange(GetChildActions((ReferenceProfileAction)a, ProfileActionListType.Started));
                foreach (var a in profile.ApplicationClosed.Where(a => a.GetType().Equals(typeof(ReferenceProfileAction))))
                    chainInfo.ApplicationClosedChilds.AddRange(GetChildActions((ReferenceProfileAction)a, ProfileActionListType.Closed));
                foreach (var a in profile.ApplicationGotFocus.Where(a => a.GetType().Equals(typeof(ReferenceProfileAction))))
                    chainInfo.ApplicationGotFocusChilds.AddRange(GetChildActions((ReferenceProfileAction)a, ProfileActionListType.GotFocus));
                foreach (var a in profile.ApplicationLostFocus.Where(a => a.GetType().Equals(typeof(ReferenceProfileAction))))
                    chainInfo.ApplicationLostFocusChilds.AddRange(GetChildActions((ReferenceProfileAction)a, ProfileActionListType.LostFocus));
                profileChains.Add(chainInfo);
            }
            foreach (ProfileChainInfo chainInfo in profileChains)
            {
                bool hasParent = false;
                foreach (ProfileChainInfo otherChainInfo in profileChains)
                {
                    if (otherChainInfo.AllActions.Any(a => a.ReferenceProfile.GUID.Equals(chainInfo.Profile.GUID)))
                    {
                        hasParent = true;
                        break;
                    }    
                }
                chainInfo.HasParent = hasParent;
            }

            foreach (ProfileChainInfo chainInfo in profileChains.Where(c => c.HasParent == false))
            {
                if (chainInfo.HasLoop)
                    return false;

            }
            return true;
        }

        public List<ReferenceProfileAction> GetChildActions(ReferenceProfileAction action, ProfileActionListType listType)
        {
            List<ReferenceProfileAction> actions = new List<ReferenceProfileAction>();

            foreach (var a in action.ReferenceProfile.GetProfileActions(listType).Where(a => a.GetType().Equals(typeof(ReferenceProfileAction))))
            {
                actions.AddRange(GetChildActions((ReferenceProfileAction)a, listType));
            }
            return actions;
        }
    }

    public class ProfileChainInfo
    {



        public Profile Profile { get;  }
        public bool HasParent { get; set; }
        public List<ReferenceProfileAction> ApplicationStartedChilds = new List<ReferenceProfileAction>();
        public List<ReferenceProfileAction> ApplicationClosedChilds = new List<ReferenceProfileAction>();
        public List<ReferenceProfileAction> ApplicationGotFocusChilds = new List<ReferenceProfileAction>();
        public List<ReferenceProfileAction> ApplicationLostFocusChilds = new List<ReferenceProfileAction>();

        public List<ReferenceProfileAction> AllActions
        {
            get
            {
                List<ReferenceProfileAction> allActions = new List<ReferenceProfileAction>();
                allActions.AddRange(ApplicationStartedChilds);
                allActions.AddRange(ApplicationClosedChilds);
                allActions.AddRange(ApplicationGotFocusChilds);
                allActions.AddRange(ApplicationLostFocusChilds);
                return allActions;
            }
        }
        public bool ApplicationStartedChildsHaveLoop
        {
            get
            {
                int count;
                count = ApplicationStartedChilds.GroupBy(x => x.ReferenceGuid)
                                        .Where(g => g.Count() > 1)
                                        .Count();
                if (count > 1)
                    return true;
                else
                    return false;
            }
        }

        public bool ApplicationClosedChildsHaveLoop
        {
            get
            {
                int count;
                count = ApplicationClosedChilds.GroupBy(x => x.ReferenceGuid)
                                        .Where(g => g.Count() > 1)
                                        .Count();
                if (count > 1)
                    return true;
                else
                    return false;
            }
        }
        public bool ApplicationGotFocusChildsHaveLoop
        {
            get
            {
                int count;
                count = ApplicationGotFocusChilds.GroupBy(x => x.ReferenceGuid)
                                        .Where(g => g.Count() > 1)
                                        .Count();
                if (count > 1)
                    return true;
                else
                    return false;
            }
        }

        public bool ApplicationLostFocusChildsHaveLoop
        {
            get
            {
                int count;
                count = ApplicationLostFocusChilds.GroupBy(x => x.ReferenceGuid)
                                        .Where(g => g.Count() > 1)
                                        .Count();
                if (count > 1)
                    return true;
                else
                    return false;
            }
        }



        public bool HasLoop => ApplicationStartedChildsHaveLoop && ApplicationClosedChildsHaveLoop && ApplicationGotFocusChildsHaveLoop && ApplicationLostFocusChildsHaveLoop;

        public ProfileChainInfo(Profile profile)
        {
            Profile = profile;
        }
    }
}
