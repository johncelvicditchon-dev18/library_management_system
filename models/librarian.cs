namespace oop_project.Models
{
    // INHERITANCE: a Librarian is a Person who runs the circulation desk.
    public class Librarian : Person
    {
        public override string Role
        {
            get { return "LIBRARIAN"; }
        }
        public override int BorrowLimit
        {
            get { return 10; }
        }
        public override string GetAccessDescription()
        {
            return "LIBRARIAN ACCESS: MANAGES BOOKS, LOANS AND ACCOUNTS.";
        }
    }
}
