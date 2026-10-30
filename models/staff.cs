namespace oop_project.Models
{
    // INHERITANCE: a Staff member is a Person with library staff rules.
    public class Staff : Person
    {
        public override string Role
        {
            get { return "STAFF"; }
        }
        public override int BorrowLimit
        {
            get { return 5; }
        }
        public override string GetAccessDescription()
        {
            return "STAFF ACCESS: MANAGES DAILY CIRCULATION AND ACCOUNTS.";
        }
    }
}
