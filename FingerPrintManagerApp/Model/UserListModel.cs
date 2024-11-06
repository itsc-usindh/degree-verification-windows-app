using System.Windows.Input;
using System.Windows;

namespace FingerPrintManagerApp.Model
{
    public class UserListModel
    {
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string CNIC { get; set; }
        public string ChallanNo { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }

        public ICommand ViewCommand { get; set; }

        public UserListModel()
        {
            // Initialize the ViewCommand with an action to execute when triggered
            ViewCommand = new RelayCommand(ExecuteViewCommand);
        }

        private void ExecuteViewCommand(object parameter)
        {
            // Action to perform when the command is executed
            MessageBox.Show($"Viewing record for {Name}");
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged;
    }

}
