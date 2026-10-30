namespace oop_project.Models
{
    // INHERITANCE: a Student is a Person with student rules and a student profile.
    public class Student : Person
    {
        private string _lastName = string.Empty;
        private string _firstName = string.Empty;
        private string _middleInitial = string.Empty;
        private string _schoolId = string.Empty;
        private string _yearLevel = string.Empty;
        private string _contactNumber = string.Empty;
        private string _address = string.Empty;

        // ENCAPSULATION: the student profile lives only on a Student.
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = (value ?? string.Empty).Trim(); }
        }
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = (value ?? string.Empty).Trim(); }
        }
        public string MiddleInitial
        {
            get { return _middleInitial; }
            set { _middleInitial = (value ?? string.Empty).Trim().TrimEnd('.'); }
        }
        public string SchoolId
        {
            get { return _schoolId; }
            set { _schoolId = (value ?? string.Empty).Trim(); }
        }
        public string YearLevel
        {
            get { return _yearLevel; }
            set { _yearLevel = (value ?? string.Empty).Trim(); }
        }
        public string ContactNumber
        {
            get { return _contactNumber; }
            set { _contactNumber = (value ?? string.Empty).Trim(); }
        }
        public string Address
        {
            get { return _address; }
            set { _address = (value ?? string.Empty).Trim(); }
        }

        // "DITCHON, JOHNCEL VIC D." - the name shown in the dashboard and the reports.
        public string ComposeDisplayName()
        {
            string middle = string.IsNullOrEmpty(_middleInitial) ? string.Empty : " " + _middleInitial + ".";
            return (_lastName + ", " + _firstName + middle).Trim();
        }

        public override string Role
        {
            get { return "STUDENT"; }
        }
        public override int BorrowLimit
        {
            get { return 3; }
        }
        public override string GetAccessDescription()
        {
            return "STUDENT ACCESS: MAY BORROW UP TO 3 BOOKS.";
        }
    }
}
