namespace oop_project.Models
{
    // INHERITANCE: an Officer is a Person with full library officer rights.
    public class Officer : Person
    {
        public override string Role
        {
            get { return "OFFICER"; }
        }
        public override int BorrowLimit
        {
            get { return 15; }
        }
        public override string GetAccessDescription()
        {
            return "OFFICER ACCESS: FULL CIRCULATION AND ACCOUNT APPROVAL.";
        }
    }
}
