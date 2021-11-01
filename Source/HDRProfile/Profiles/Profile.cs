using AutoHDR.Profiles.Actions;
using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutoHDR.Profiles
{
    public class Profile : BaseViewModel
    {

        public enum ProfileActionListType
        {
            Started,
            Closed,
            GotFocus,
            LostFocus
        }

        [XmlIgnore]
        public RelayCommand AddStartedActionCommand { get; private set; }
        [XmlIgnore]
        public RelayCommand AddClosedActionCommand { get; private set; }
        [XmlIgnore]
        public RelayCommand AddGotFocusActionCommand { get; private set; }
        [XmlIgnore]
        public RelayCommand AddLostFocusActionCommand { get; private set; }
        [XmlIgnore]
        public RelayCommand<IProfileAction> RemoveProfileActionCommand { get; private set; }
        [XmlIgnore]
        public RelayCommand<IProfileAction> RemoveStartedActionCommand { get; private set; }
        [XmlIgnore]
        public RelayCommand<IProfileAction> RemoveClosedActionCommand { get; private set; }
        [XmlIgnore]
        public RelayCommand<IProfileAction> RemoveGotFocusActionCommand { get; private set; }
        [XmlIgnore]
        public RelayCommand<IProfileAction> RemoveLostFocusActionCommand { get; private set; }


        public Profile()
        {
            AddStartedActionCommand = new RelayCommand(() => AddProfileAction(ProfileActionListType.Started));
            AddClosedActionCommand = new RelayCommand(() => AddProfileAction(ProfileActionListType.Closed));
            AddGotFocusActionCommand = new RelayCommand(() => AddProfileAction(ProfileActionListType.GotFocus));
            AddLostFocusActionCommand = new RelayCommand(() => AddProfileAction(ProfileActionListType.LostFocus));
            RemoveProfileActionCommand = new RelayCommand<IProfileAction>((pa) => RemoveProfileAction(pa));

            RemoveStartedActionCommand = new RelayCommand<IProfileAction>((pa) => RemoveProfileAction(ProfileActionListType.Started, pa));
            RemoveClosedActionCommand = new RelayCommand<IProfileAction>((pa) => RemoveProfileAction(ProfileActionListType.Closed, pa));
            RemoveGotFocusActionCommand = new RelayCommand<IProfileAction>((pa) => RemoveProfileAction(ProfileActionListType.GotFocus, pa));
            RemoveLostFocusActionCommand = new RelayCommand<IProfileAction>((pa) => RemoveProfileAction(ProfileActionListType.LostFocus, pa));
        }

        private string _name = "-";
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private ProfileMode _mode = ProfileMode.OnRunning;

        public ProfileMode Mode
        {
            get { return _mode; }
            set { _mode = value; OnPropertyChanged(); }
        }

        private ListOfProfileActions _applicationStarted = new ListOfProfileActions();
        public ListOfProfileActions ApplicationStarted
        {
            get { return _applicationStarted; }
            set { _applicationStarted = value; OnPropertyChanged(); }
        }

        private ListOfProfileActions _applicationClosed = new ListOfProfileActions();
        public ListOfProfileActions ApplicationClosed
        {
            get { return _applicationClosed; }
            set { _applicationClosed = value; OnPropertyChanged(); }
        }

        private ListOfProfileActions _applicationGotFocus = new ListOfProfileActions();

        public ListOfProfileActions ApplicationGotFocus
        {
            get { return _applicationGotFocus; }
            set { _applicationGotFocus = value; OnPropertyChanged(); }
        }

        private ListOfProfileActions _applicationLostFocus = new ListOfProfileActions();

        public ListOfProfileActions ApplicationLostFocus
        {
            get { return _applicationLostFocus; }
            set { _applicationLostFocus = value; OnPropertyChanged(); }
        }

        public void AddProfileAction(ProfileActionListType listType)
        {

            ProfileActionAdder adder = new ProfileActionAdder();
            adder.DialogService = DialogService;
            adder.OKClicked += (o, e) =>
            {
                switch (listType)
                {
                    case ProfileActionListType.Started:
                        if (!ApplicationStarted.Any(a => a.GetType().Equals(adder.ActionType)))
                            ApplicationStarted.Add(adder.ProfileAction);
                        break;
                    case ProfileActionListType.Closed:
                        if (!ApplicationClosed.Any(a => a.GetType().Equals(adder.ActionType)))
                            ApplicationClosed.Add(adder.ProfileAction);
                        break;
                    case ProfileActionListType.GotFocus:
                        if (!ApplicationGotFocus.Any(a => a.GetType().Equals(adder.ActionType)))
                            ApplicationGotFocus.Add(adder.ProfileAction);
                        break;
                    case ProfileActionListType.LostFocus:
                        if (!ApplicationLostFocus.Any(a => a.GetType().Equals(adder.ActionType)))
                            ApplicationLostFocus.Add(adder.ProfileAction);
                        break;

                }
            };
            if (DialogService != null)
                DialogService.ShowDialogModal(adder);
        }

        public void RemoveProfileAction( IProfileAction profileAction)
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


        public void RemoveProfileAction(ProfileActionListType listType, IProfileAction profileAction)
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
    }
}
