using AutoHDR.Displays;
using AutoHDR.Profiles.Actions;
using CodectoryCore.UI.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutoHDR.Profiles
{
    public enum ProfileActionListType
    {
        None,
        Started,
        Closed,
        GotFocus,
        LostFocus
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Profile : BaseViewModel, IEquatable<Profile>
    {

        public static Profile DefaultProfile()
        {
            Profiles.Profile defaultProfile = new Profiles.Profile();
            defaultProfile.Name = "HDR";
            Profiles.Actions.DisplayAction startAction = new Profiles.Actions.DisplayAction();
            startAction.Display = Display.AllDisplays;
            startAction.SetHDR = true;
            startAction.EnableHDR = true;
            Profiles.Actions.DisplayAction endAction = new Profiles.Actions.DisplayAction();
            endAction.Display = Display.AllDisplays;
            endAction.SetHDR = true;
            endAction.EnableHDR = false;
            defaultProfile.ApplicationStarted.Add(startAction);
            defaultProfile.ApplicationClosed.Add(endAction);
            return defaultProfile;
        }


        public RelayCommand AddStartedActionCommand { get; private set; }
        public RelayCommand AddClosedActionCommand { get; private set; }
        public RelayCommand AddGotFocusActionCommand { get; private set; }
        public RelayCommand AddLostFocusActionCommand { get; private set; }

        public RelayCommand<ProfileActionBase> EditProfileActionCommand { get; private set; }
        public RelayCommand<ProfileActionBase> RemoveProfileActionCommand { get; private set; }



        public Profile()
        {
            _guid = Guid.NewGuid();
            AddStartedActionCommand = new RelayCommand(() => AddProfileAction(ProfileActionListType.Started));
            AddClosedActionCommand = new RelayCommand(() => AddProfileAction(ProfileActionListType.Closed));
            AddGotFocusActionCommand = new RelayCommand(() => AddProfileAction(ProfileActionListType.GotFocus));
            AddLostFocusActionCommand = new RelayCommand(() => AddProfileAction(ProfileActionListType.LostFocus));
            EditProfileActionCommand = new RelayCommand<ProfileActionBase>((pa) => EditProfileAction(pa));
            RemoveProfileActionCommand = new RelayCommand<ProfileActionBase>((pa) => RemoveProfileAction(pa));
        }

        private Guid _guid = Guid.Empty;

        [JsonProperty]
        public Guid GUID
        {
            get { return _guid; }
            set { if (value.Equals(Guid.Empty)) _guid = Guid.NewGuid(); _guid = value; OnPropertyChanged(); }
        }

        private string _name = "-";

        [JsonProperty]
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private ListOfProfileActions _applicationStarted = new ListOfProfileActions();

        [JsonProperty]
        public ListOfProfileActions ApplicationStarted
        {
            get { return _applicationStarted; }
            set { _applicationStarted = value; OnPropertyChanged(); }
        }

        private ListOfProfileActions _applicationClosed = new ListOfProfileActions();

        [JsonProperty]
        public ListOfProfileActions ApplicationClosed
        {
            get { return _applicationClosed; }
            set { _applicationClosed = value; OnPropertyChanged(); }
        }

        private ListOfProfileActions _applicationGotFocus = new ListOfProfileActions();

        [JsonProperty]
        public ListOfProfileActions ApplicationGotFocus
        {
            get { return _applicationGotFocus; }
            set { _applicationGotFocus = value; OnPropertyChanged(); }
        }

        private ListOfProfileActions _applicationLostFocus = new ListOfProfileActions();

        [JsonProperty]
        public ListOfProfileActions ApplicationLostFocus
        {
            get { return _applicationLostFocus; }
            set { _applicationLostFocus = value; OnPropertyChanged(); }


        }

        private bool _restartApplication = false;

        [JsonProperty]
        public bool RestartApplication { get => _restartApplication; set { _restartApplication = value; OnPropertyChanged(); } }




        public void AddProfileAction(ProfileActionListType listType)
        {
            ProfileActionAdder adder = new ProfileActionAdder();
            adder.DialogService = DialogService;
            adder.OKClicked += (o, e) =>
            {
                switch (listType)
                {
                    case ProfileActionListType.Started:
                        ApplicationStarted.Add(adder.ProfileAction);
                        break;
                    case ProfileActionListType.Closed:
                        ApplicationClosed.Add(adder.ProfileAction);
                        break;
                    case ProfileActionListType.GotFocus:
                        ApplicationGotFocus.Add(adder.ProfileAction);
                        break;
                    case ProfileActionListType.LostFocus:
                        ApplicationLostFocus.Add(adder.ProfileAction);
                        break;

                }
            };
            if (DialogService != null)
                DialogService.ShowDialogModal(adder, new System.Drawing.Size(800, 600));
        }

        public void EditProfileAction(ProfileActionBase profileAction)
        {
            ProfileActionListType listType = GetProfileActionListType(profileAction);
            ProfileActionAdder adder = new ProfileActionAdder(profileAction);

            adder.DialogService = DialogService;
            adder.OKClicked += (o, e) =>
            {
                RemoveProfileAction(listType, profileAction);
                switch (listType)
                {
                    case ProfileActionListType.Started:
                        ApplicationStarted.Add(adder.ProfileAction);
                        break;
                    case ProfileActionListType.Closed:
                        ApplicationClosed.Add(adder.ProfileAction);
                        break;
                    case ProfileActionListType.GotFocus:
                        ApplicationGotFocus.Add(adder.ProfileAction);
                        break;
                    case ProfileActionListType.LostFocus:
                        ApplicationLostFocus.Add(adder.ProfileAction);
                        break;

                }
            };
            if (DialogService != null)
                DialogService.ShowDialogModal(adder, new System.Drawing.Size(800, 600));
        }

        private ProfileActionListType GetProfileActionListType(ProfileActionBase profileAction)
        {
            if (ApplicationStarted.Contains(profileAction))
                return ProfileActionListType.Started;
            if (ApplicationClosed.Contains(profileAction))
                return ProfileActionListType.Closed;
            if (ApplicationGotFocus.Contains(profileAction))
                return ProfileActionListType.GotFocus;
            if (ApplicationLostFocus.Contains(profileAction))
                return ProfileActionListType.LostFocus;
            return ProfileActionListType.None;
        }

        public void RemoveProfileAction(ProfileActionBase profileAction)
        {
            if (ApplicationStarted.Contains(profileAction))
                ApplicationStarted.Remove(profileAction);
            if (ApplicationClosed.Contains(profileAction))
                ApplicationClosed.Remove(profileAction);
            if (ApplicationGotFocus.Contains(profileAction))
                ApplicationGotFocus.Remove(profileAction);
            if (ApplicationLostFocus.Contains(profileAction))
                ApplicationLostFocus.Remove(profileAction);
        }


        public void RemoveProfileAction(ProfileActionListType listType, ProfileActionBase profileAction)
        {
            switch (listType)
            {
                case ProfileActionListType.Started:
                    ApplicationStarted.Remove(profileAction);
                    break;
                case ProfileActionListType.Closed:
                    ApplicationClosed.Remove(profileAction);
                    break;
                case ProfileActionListType.GotFocus:
                    ApplicationGotFocus.Remove(profileAction);
                    break;
                case ProfileActionListType.LostFocus:
                    ApplicationLostFocus.Remove(profileAction);
                    break;

            }

        }

        public override string ToString()
        {
            return $"{Name} {GetHashCode()}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Profile);
        }

        public bool Equals(Profile other)
        {
            return other != null &&
                   GUID == other._guid &&
                   EqualityComparer<ListOfProfileActions>.Default.Equals(ApplicationStarted, other.ApplicationStarted) &&
                   EqualityComparer<ListOfProfileActions>.Default.Equals(ApplicationClosed, other.ApplicationClosed) &&
                   EqualityComparer<ListOfProfileActions>.Default.Equals(ApplicationGotFocus, other.ApplicationGotFocus) &&
                   EqualityComparer<ListOfProfileActions>.Default.Equals(ApplicationLostFocus, other.ApplicationLostFocus);
        }

        public override int GetHashCode()
        {
            int hashCode = 210938521;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GUID.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<ListOfProfileActions>.Default.GetHashCode(ApplicationStarted);
            hashCode = hashCode * -1521134295 + EqualityComparer<ListOfProfileActions>.Default.GetHashCode(ApplicationClosed);
            hashCode = hashCode * -1521134295 + EqualityComparer<ListOfProfileActions>.Default.GetHashCode(ApplicationGotFocus);
            hashCode = hashCode * -1521134295 + EqualityComparer<ListOfProfileActions>.Default.GetHashCode(ApplicationLostFocus);
            return hashCode;
        }
    }
}
