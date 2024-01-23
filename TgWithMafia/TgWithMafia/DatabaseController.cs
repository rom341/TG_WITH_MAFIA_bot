using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace TgWithMafia
{
    public class DatabaseController : IDisposable
    {
        private static DatabaseController instance;
        private readonly MySqlConnection connection;

        public DatabaseController()
        {
            try
            {
                connection = new MySqlConnection(AppConfigController.ConnectionSettings.MySqlConnection);
                connection.Open();
                ExecuteSqlQuery("USE `sql11674382`");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during database connection initialization: " + ex.Message);
            }
        }

        public static DatabaseController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DatabaseController();
                }
                return instance;
            }
        }

        public DataTable ExecuteSqlQuery(string query, params MySqlParameter[] parameters)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Добавим параметры, чтобы предотвратить SQL-инъекции
                    command.Parameters.AddRange(parameters);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        dataTable.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            return dataTable;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources (close connection)
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            // No unmanaged resources to release, but if there were, they would be released here.
        }
    }
}
