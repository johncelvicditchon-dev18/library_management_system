using MySql.Data.MySqlClient;
namespace oop_project.Database
{
    // ABSTRACTION: the forms depend on this contract instead of a concrete class.
    public interface IDatabaseConnection
    {
        MySqlConnection CreateConnection();
    }
}
