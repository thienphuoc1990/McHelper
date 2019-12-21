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
            dv.Sort = "id";
            dv.Delete(0);
            dv.RowFilter = "";
            save();
        }

        /// <summary>
        /// Selects a single record from the Category table.
        /// </summary>
        public static DataRow Select(string id)
        {
            dv.RowFilter = "id='" + id + "'";
            dv.Sort = "id";
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

        /// <summary>
        /// Updates a record in the Category table.
        /// </summary>
        public static void UpdateAllFields(Character character)
        {
            DataRow dr = Select(character.ID);
            dr[1] = character.Link;
            dr[2] = character.Date;
            dr[3] = character.VipLevel;
            dr[4] = character.IncreaseFPS;
            dr[5] = character.VipPromotion;
            dr[6] = character.DoiNangNo;
            dr[7] = character.DoiNangNoNL4;
            dr[8] = character.DoiNangNoLoai;
            dr[9] = character.TrongNL;
            dr[10] = character.TrongNLLoai;
            dr[11] = character.TriAn;
            dr[12] = character.LatTheBai;
            dr[13] = character.RutBo;
            dr[14] = character.DoiKGDK;
            dr[15] = character.TuHanh;
            dr[16] = character.TruMa;
            dr[17] = character.AoMaThap;
            dr[18] = character.TrongCay;
            dr[19] = character.CheMatBao;
            dr[20] = character.CheMatBaoLoai;
            dr[21] = character.CheMatBaoCap;
            dr[22] = character.AutoPhuBan;
            dr[23] = character.AutoPhuBanDanhSach;
            dr[24] = character.UocNguyen;
            dr[25] = character.DauPet;
            dr[26] = character.NhanThuongHLVT;
            dr[27] = character.BugOnline;
            dr[28] = character.MeTran;
            dr[29] = character.HaiThuoc;
            dr[30] = character.CauCa;
            dr[31] = character.ViTriNhanVat;
            dr[32] = character.Running;
            save();
        }
    }
}
