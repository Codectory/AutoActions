using AutoActions.Profiles.Actions;
using CodectoryCore;
using CodectoryCore.UI.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ProfileActionShortcut : BaseViewModel
    {
        private IProfileAction _action;

        [JsonProperty]
        public IProfileAction Action
        { 
            get => _action; 
            set { _action = value; OnPropertyChanged(); }
        }

        private string _shortcutName;

        [JsonProperty]
        public string ShortcutName
        {
            get { return _shortcutName; }
            set { _shortcutName = value;  OnPropertyChanged(); }
        }

        public RelayCommand RunActionCommand { get; private set; }

        private ProfileActionShortcut()
        {
            RunActionCommand = new RelayCommand(RunAction);

        }
        public ProfileActionShortcut(IProfileAction action, string shortcutName) : this()
        {
            Action = action;
            ShortcutName = shortcutName;
        }

        public void RunAction()
        {
            Action.RunAction(ApplicationChangedType.None);
        }
    }

    public class ActionShortcutManager : BaseViewModel
    {

        public RelayCommand AddActionShortcutCommand { get; private set; }
        public RelayCommand<ProfileActionShortcut> EditActionShortcutCommand { get; private set; }
        public RelayCommand<ProfileActionShortcut> RemoveActionShortcutCommand { get; private set; }

        public DispatchingObservableCollection<ProfileActionShortcut> ActionShortcuts => ProjectData.Instance.Settings.ActionShortcuts;

        public ActionShortcutManager()
        {
            AddActionShortcutCommand = new RelayCommand(AddActionShortcut);
            EditActionShortcutCommand = new RelayCommand<ProfileActionShortcut>(EditActionShortcut);
            RemoveActionShortcutCommand = new RelayCommand<ProfileActionShortcut>(RemoveActionShortcut);

        }

     
        private void AddActionShortcut()
        {
            ProfileActionAdder adder = new ProfileActionAdder();
            adder.DialogService = DialogService;
            adder.OKClicked += (o, e) =>
            {
                ProfileActionShortcut shortcut = new ProfileActionShortcut(adder.ProfileAction, adder.ProfileAction.ActionDescription);
                ProjectData.Instance.Settings.ActionShortcuts.Add(shortcut);
            };
            if (DialogService != null)
                DialogService.ShowDialogModal(adder, new System.Drawing.Size(800, 600));
        }

        private void RemoveActionShortcut(ProfileActionShortcut obj)
        {
            ProjectData.Instance.Settings.ActionShortcuts.Remove(obj);
        }

        private void EditActionShortcut(ProfileActionShortcut obj)
        {
            ProfileActionAdder adder = new ProfileActionAdder(obj.Action);

            adder.DialogService = DialogService;
            if (DialogService != null)
                DialogService.ShowDialogModal(adder, new System.Drawing.Size(800, 600));
        }
    }
}
