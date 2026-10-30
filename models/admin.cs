namespace oop_project.Models
{
    // INHERITANCE: an Admin is a Person that can manage the whole library system.
    public class Admin : Person
    {
        public override string Role
        {
            get { return "ADMIN"; }
        }
        public override int BorrowLimit
        {
            get { return 10; }
        }
        public override string GetAccessDescription()
        {
            return "ADMIN ACCESS: MANAGES ACCOUNTS, BOOKS AND LOANS.";
        }
    }
}
