using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using AutoVPT.Objects;
using System.IO;
using System.Reflection;

namespace AutoVPT.DML
{
    public sealed class XMLCharacter
    {
        private XMLCharacter() { }
        static DataSet ds = new DataSet();
        static DataView dv = new DataView();
        /// <summary>
        /// Inserts a record into the Character table.
        /// </summary>
        /// 
        public static void save()
        {
            ds.WriteXml(Application.StartupPath + "\\database\\data.xml", XmlWriteMode.WriteSchema);
        }
        public static void Insert(string id, string link)
        {
            DataRow dr = dv.Table.NewRow();
            dr[0] = id;
            dr[1] = link;
            dv.Table.Rows.Add(dr);
            save();
        }

        /// <summary>
        /// Updates a record in the Category table.
        /// </summary>
        public static void Update(string id, string link)
        {
            DataRow dr = Select(id);
            dr[1] = link;
            save();
        }

        /// <summary>
        /// Deletes a record from the Category table by a composite primary key.
        /// </summary>
        public static void Delete(string id)
        {
            dv.RowFilter = "id='" + id + "'";
            //dv.Sort = "id";
            dv.Delete(0);
            dv.RowFilter = "";
            save();
        }

        /// <summary>
        /// Selects a single record from the Category table.
        /// </summary>
        public static DataRow SelectByRowIndex(int index)
        {
            DataRow dr = null;
            if (dv.Count > 0)
            {
                dr = dv[index].Row;
            }
            return dr;
        }

        /// <summary>
        /// Selects a single record from the Category table.
        /// </summary>
        public static DataRow Select(string id)
        {
            dv.RowFilter = "id='" + id + "'";
            //dv.Sort = "id";
            DataRow dr = null;
            if (dv.Count > 0)
            {
                dr = dv[0].Row;
            }
            dv.RowFilter = "";
            return dr;
        }

        /// <summary>
        /// Selects all records from the Category table.
        /// </summary>
        public static DataView SelectAll()
        {
            ds.Clear();
            ds.ReadXml(Application.StartupPath + "\\database\\data.xml", XmlReadMode.ReadSchema);
            dv = ds.Tables[0].DefaultView;
            return dv;
        }
    }
}
