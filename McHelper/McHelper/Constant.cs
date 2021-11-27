using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McHelper
{
    class Constant
    {
        public const string SQLITE_FILE_NAME = "McHelperDatabase.sqlite";
        public const string SQLITE_TABLE_LINK_NAME = "tbl_links";
        public const string SQLITE_TABLE_LINK_CREATE_SQL = "CREATE TABLE IF NOT EXISTS tbl_links ([id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, link nvarchar(255), identifier varchar(50))";
        public const string SQLITE_TABLE_LINK_GET_ALL_SQL = "SELECT id as [ID], link as [Link], identifier as [Identifier] from tbl_links";
    }
}
