using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Data;
using System.Windows.Forms;

namespace McHelper.Helper
{
    class SQLiteHelper
    {
        SQLiteConnection _con = new SQLiteConnection();
        string _sqlFilename;

        public SQLiteHelper(string sqliteFilename)
        {
            _sqlFilename = sqliteFilename;
        }

        public void InitialDatabase()
        {
            if (String.IsNullOrWhiteSpace(_sqlFilename))
            {
                throw new SQLiteException("SQLite file name is required");
            }

            if (File.Exists(_sqlFilename))
            {
                CreateFileSQLite();
            }
        }

        public void InitialTable(string tableName, string createTableSql)
        {
            CreateConnection();
            if(TableAlreadyExists(tableName))
            {
                return;
            }

            CreateTable(createTableSql);

            CloseConnection();
        }

        public bool TableAlreadyExists(string tableName)
        {
            var sql =
            "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tableName + "';";
            if (_con.State == ConnectionState.Open)
            {
                SQLiteCommand command = new SQLiteCommand(sql, _con);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
                return false;
            }
            else
            {
                throw new ArgumentException("Connection state must be open");
            }
        }

        public void CreateTable(string createTableSql)
        {
            if (string.IsNullOrWhiteSpace(createTableSql))
            {
                throw new ArgumentException("Create table string must be defined");
            }

            if (_con.State == ConnectionState.Open)
            {
                SQLiteCommand command = new SQLiteCommand(createTableSql, _con);
                command.ExecuteNonQuery();
            }
            else
            {
                throw new ArgumentException("Connection state must be open");
            }
        }

        public void CreateConnection()
        {

            string _strConnect = string.Format("Data Source={0};Version=3;", _sqlFilename);
            _con.ConnectionString = _strConnect;
            _con.Open();
        }

        public void CloseConnection()
        {
            _con.Close();
        }

        public void CreateFileSQLite()
        {
            SQLiteConnection.CreateFile(_sqlFilename);
        }

        public DataSet LoadData(string loadDataSql)
        {
            DataSet dataSet = new DataSet();
            CreateConnection();
            SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(loadDataSql, _con);

            dataAdapter.Fill(dataSet);
            CloseConnection();
            return dataSet;
        }
    }
}
