using AutoActions.Profiles;
using CodectoryCore;
using CodectoryCore.UI.Wpf;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace AutoActions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApplicationProfileAssignment : BaseViewModel
    {
        private int _position = -1;
        private ApplicationItem _application = null;


        private static SortableObservableCollection<ApplicationProfileAssignment> Assignments => Globals.Instance.Settings.ApplicationProfileAssignments;

        [JsonProperty]
        public ApplicationItem Application { get => _application; set { _application = value; OnPropertyChanged(); }
        }

        private Guid _profileGuid = Guid.Empty;

        [JsonProperty]
        public Guid ProfileGUID
        {
            get => _profileGuid;
            set
            {
                _profileGuid = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Profile));
            }
        }

        public Profile Profile {
            get
            {
                if (Globals.Instance.Settings.ApplicationProfiles.Any(p => p.GUID.Equals(_profileGuid)))
                    return Globals.Instance.Settings.ApplicationProfiles.First(p => p.GUID.Equals(_profileGuid));
                else
                    return null;
            }
            set 
            { 
                if (value == null) 
                {_profileGuid = Guid.Empty; 
                    return; 
                } 
                _profileGuid = value.GUID.Equals(Guid.Empty) ? Guid.NewGuid() : value.GUID;   OnPropertyChanged(); OnPropertyChanged(nameof(ProfileGUID)); 
            }
        }

        [JsonProperty]

        public int Position { get => _position;  set { _position = value; OnPropertyChanged(); } }


        private ApplicationProfileAssignment()
        {

        }

        private ApplicationProfileAssignment(ApplicationItem application)
        {
            Application = application;
        }


        public static ApplicationProfileAssignment NewAssigment(ApplicationItem application)
        {
            ApplicationProfileAssignment assigment = new ApplicationProfileAssignment(application);
            assigment.Position = GetNextPosition();
            Assignments.Add(assigment);
            Assignments.Sort(x => x.Position, System.ComponentModel.ListSortDirection.Ascending);
            return assigment;
        }

        private static int GetNextPosition()
        {
            int position = 0;
            while (Assignments.Any(x => x.Position == position))
            {
                position++;
            }
            return position;
        }



    }
    
}
