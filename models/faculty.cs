namespace oop_project.Models
{
    // INHERITANCE: a Faculty member is a Person with faculty rules.
    public class Faculty : Person
    {
        public override string Role
        {
            get { return "FACULTY"; }
        }
        public override int BorrowLimit
        {
            get { return 10; }
        }
        public override string GetAccessDescription()
        {
            return "FACULTY ACCESS: MAY BORROW UP TO 10 BOOKS.";
        }
    }
}
