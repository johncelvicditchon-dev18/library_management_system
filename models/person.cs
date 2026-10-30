using System;
namespace oop_project.Models
{
    // ABSTRACTION + ENCAPSULATION: defines what a library user is and hides its data.
    public abstract class Person
    {
        private string _username = string.Empty;
        private string _fullName = string.Empty;
        private string _program = string.Empty;
        private string _institute = string.Empty;
        public string Username
        {
            get { return _username; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("USERNAME IS REQUIRED."); }
                _username = value.Trim();
            }
        }
        // ENCAPSULATION: falls back to the username when no full name was saved.
        public string FullName
        {
            get { return string.IsNullOrWhiteSpace(_fullName) ? _username : _fullName; }
            set { _fullName = (value ?? string.Empty).Trim(); }
        }
        // POLYMORPHISM: every subclass supplies its own role rules.
        public abstract string Role { get; }
        public abstract int BorrowLimit { get; }
        public abstract string GetAccessDescription();

        // ENCAPSULATION: the program and the institute / office every role may have.
        // They stay empty for roles that do not ask for them.
        public string Program
        {
            get { return _program; }
            set { _program = (value ?? string.Empty).Trim(); }
        }
        public string Institute
        {
            get { return _institute; }
            set { _institute = (value ?? string.Empty).Trim(); }
        }
        public virtual string GetWelcomeMessage()
        {
            return "WELCOME, " + FullName;
        }
        public override string ToString()
        {
            return FullName + " (" + Username + ") - " + Role;
        }
    }
}
