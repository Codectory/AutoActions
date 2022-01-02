using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using AutoHDR.UWP;
using AutoHDR.ProjectResources;

namespace AutoHDR.Profiles.Actions
{
    public class ProfileActionAdder : DialogViewModelBase
    {


        private bool _editMode = false;
        private bool _canCreate = false;

        public bool CanCreate { get => _canCreate; set { _canCreate = value; OnPropertyChanged(); } }
        public bool EditMode { get => _editMode; set { _editMode = value; OnPropertyChanged(); } }


        private ActionTypeDescription _actionType = null;

        public ActionTypeDescription ActionType

        {
            get { return _actionType; }
            set 
            {
                _actionType = value;
                UpdateCanCreate();
                ContentControlViewModel = (BaseViewModel)Activator.CreateInstance(_actionType.ActionType);
                ContentControlViewModel.PropertyChanged += ContentControlViewModel_PropertyChanged;
                OnPropertyChanged(); 
            }
        }

        private void ContentControlViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateCanCreate();
        }

        private BaseViewModel _contentControlViewModel;
        public BaseViewModel ContentControlViewModel
        {
            get { return _contentControlViewModel; }
            set
            {
                _contentControlViewModel = value;
                OnPropertyChanged();
                ProfileAction = (IProfileAction)ContentControlViewModel;
                UpdateCanCreate();

            }
        }
    

        private IProfileAction _profileAction = null;

        public IProfileAction ProfileAction { get => _profileAction; set { _profileAction = value; OnPropertyChanged(); } }

        public List<ActionTypeDescription> ProfileActions
        {
            get
            {
                return new List<ActionTypeDescription>() { 
                    new ActionTypeDescription(typeof(DisplayAction), ProjectLocales.DisplayAction), 
                    new ActionTypeDescription(typeof(RunProgramAction), ProjectLocales.RunProgramAction),
                    new ActionTypeDescription(typeof(CloseProgramAction), ProjectLocales.CloseProgramAction),
                    new ActionTypeDescription(typeof(ReferenceProfileAction), ProjectLocales.ReferenceProfileAction),
                    new ActionTypeDescription(typeof(AudioDeviceAction), ProjectLocales.AudioAction) };
            }
        }


        public RelayCommand<object> OKClickCommand { get; private set; }

        public event EventHandler OKClicked;

        public ProfileActionAdder()
        {
            EditMode = false;
            Title = ProjectLocales.Add;
            CreateRelayCommands();
        }

        public ProfileActionAdder(IProfileAction action)
        {
            EditMode = true;
            ActionType = ProfileActions.First(d => d.ActionType.Equals(action.GetType()));
            ContentControlViewModel = (BaseViewModel)action;

            Title = ProjectLocales.Edit;

            CreateRelayCommands();
        }

        private void CreateRelayCommands()
        {
            OKClickCommand = new RelayCommand<object>(CreateBaseProfileAction);
        }



        private void UpdateCanCreate()
        {
            CanCreate = ActionType != null && ProfileAction!= null && ProfileAction.CanSave;
        }




        public void CreateBaseProfileAction(object parameter)
        {
            OKClicked?.Invoke(this, EventArgs.Empty);
            CloseDialog(parameter as Window);
        }
     

    }
}
