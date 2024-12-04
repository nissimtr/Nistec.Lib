using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
#pragma warning disable  CS1591

namespace Nistec.Web
{
    /// <summary>
    /// ExportUtil
    /// </summary>
    public static class ExportUtil
    {
        public static void ExportCSV(DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            //headers    
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }
        public static Dictionary<string, string> ParseCommaString(string text, char outSplitter = '|', char inSplitter = ',')
        {
            var dic = new Dictionary<string, string>();

            var args = text.Split(outSplitter);
            foreach (var arg in args)
            {
                var a = arg.Split(inSplitter);
                dic[a[0]] = a[1];
            }
            return dic;
        }
        public static string ToCSV(DataTable table, bool addApos = true, bool addColumnsHeader = true, bool removeComma = true)
        {
            var result = new StringBuilder();
            int colCount = table.Columns.Count;

            string q = addApos ? "\"" : "";

            StringBuilder sb = new StringBuilder();

            if (addColumnsHeader)
            {
                IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().
                                                  Select(column => string.Concat(q, column.ColumnName, q));

                //sb.AppendLine(string.Join(",", columnNames));
                sb.Append(string.Join(",", columnNames));
                sb.Append("\n");
            }

            foreach (DataRow row in table.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => removeComma ? string.Concat(q, field.ToString().Replace(",", " ").Replace('"', '\"'), q) : string.Concat(q, field.ToString().Replace("\",\"", "\" , \"").Replace('"', '\"'), q));
                //sb.AppendLine(string.Join(",", fields));
                sb.Append(string.Join(",", fields));
                sb.Append("\n");
            }

            //foreach (DataRow row in table.Rows)
            //{
            //    IEnumerable<string> fields = row.ItemArray.Select(field =>
            //      string.Concat(q, field.ToString().Replace("\"", "\"\""), q));
            //    sb.AppendLine(string.Join(",", fields));
            //}

            return sb.ToString();

        }

    }
}
