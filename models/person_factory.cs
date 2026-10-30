using System;
namespace oop_project.Models
{
    // FACTORY + POLYMORPHISM: returns the right Person subclass for a role string,
    // so the forms can work with "Person" without knowing the concrete class.
    public static class PersonFactory
    {
        // The signup dropdown lists the roles in this order.
        public static string[] Roles
        {
            get { return new[] { "LIBRARIAN", "FACULTY", "STAFF", "STUDENT" }; }
        }
        public static Person Create(string role)
        {
            string key = (role ?? string.Empty).Trim().ToUpperInvariant();
            switch (key)
            {
                case "STUDENT": return new Student();
                case "FACULTY": return new Faculty();
                case "STAFF": return new Staff();
                case "LIBRARIAN": return new Librarian();
                // Kept so accounts saved with the old role names can still sign in.
                case "OFFICER": return new Officer();
                case "ADMIN": return new Admin();
                default: throw new ArgumentException("UNKNOWN ROLE '" + role + "'.");
            }
        }
    }
}
