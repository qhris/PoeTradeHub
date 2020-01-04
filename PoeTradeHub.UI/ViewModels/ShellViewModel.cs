using Caliburn.Micro;
using PoeTradeHub.UI.Models;

namespace PoeTradeHub.UI.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        private string _firstName = "qhris";
        private string _lastName;
        private BindableCollection<PersonModel> _people = new BindableCollection<PersonModel>();
        private PersonModel _selectedPerson;

        public ShellViewModel()
        {
            People.Add(new PersonModel { FirstName = "Tim", LastName = "Corey" });
            People.Add(new PersonModel { FirstName = "Bill", LastName = "Jones" });
            People.Add(new PersonModel { FirstName = "Christoffer", LastName = "Hjalmarsson" });
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                NotifyOfPropertyChange(() => FirstName);
                NotifyOfPropertyChange(() => FullName);
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                NotifyOfPropertyChange(() => LastName);
                NotifyOfPropertyChange(() => FullName);
            }
        }

        public string FullName =>
            $"{FirstName} {LastName}";

        public BindableCollection<PersonModel> People
        {
            get => _people;
            set { _people = value; }
        }

        public PersonModel SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                _selectedPerson = value;
                NotifyOfPropertyChange(() => SelectedPerson);
            }
        }

        public bool CanClearText(string firstName, string lastName) =>
            !string.IsNullOrWhiteSpace(firstName) ||
            !string.IsNullOrWhiteSpace(lastName);

        public void ClearText(string firstName, string lastName)
        {
            FirstName = "";
            LastName = "";
        }

        public void LoadPageOne()
        {
            ActivateItem(new PageOneViewModel());
        }

        public void LoadPageTwo()
        {
            ActivateItem(new PageTwoViewModel());
        }
    }
}
