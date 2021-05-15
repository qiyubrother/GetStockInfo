using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Data;
using System.Windows.Forms;
using AhDung.WinForm;

namespace Database
{
    public class SqlServer
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);
        private static SqlServer __Instance;
        private static string __ConnectionString = string.Empty;
        public static SqlServer Instance
        {
            get {
                if (__Instance == null)
                {
                    if (string.IsNullOrEmpty(__ConnectionString))
                    {
                        throw new Exception("Invalid connectionString.");
                    }
                    __Instance = new SqlServer(ConnectionString);
                }
                return __Instance; 
            }
            set { __Instance = value; }
        }

        public static string ConnectionString
        {
            get => __ConnectionString;
            set { __ConnectionString = value; }
        }

        public SqlServer(string connectionString)
        {
            __ConnectionString = connectionString;
        }

        public DataTable GetDataTable(string sql)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var ada = new SqlDataAdapter(sql, conn))
                {
                    var dt = new DataTable();
                    ada.Fill(dt);
                    return dt;
                }
            }
        }
        public int ExcuteSQL(string sql)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public bool ExcuteSQL(IEnumerable<string> sqlCollection)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                foreach(var sql in sqlCollection)
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch(Exception ex)
                        {
                            OutputDebugString($"Exception:{ex.Message}");
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public bool ExcuteSQL(IEnumerable<SqlCommand> sqlcmdCollection)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                foreach (var sqlcmd in sqlcmdCollection)
                {
                    sqlcmd.Connection = conn;
                    try
                    {
                        sqlcmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        OutputDebugString($"Exception:{ex.Message}");
                        return false;
                    }
                    sqlcmd.Dispose();
                }
                return true;
            }
        }

        public static void Test(string strConnectionString)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(strConnectionString);
                conn.Open();
                MsgBox.ShowInfo(
                    message: "Connect successfull.", // 消息摘要
                    attach: "详细信息",  // 详细信息
                    caption: "消息", // 窗口标题
                    expand: false, // 是否展开详细信息
                    buttonText: "OK" // 确定按钮文字
                );
            }
            catch (Exception ex)
            {
                MsgBox.ShowInfo(
                    message: "Connect falied.", // 消息摘要
                    attach: $"{ex.Message}",  // 详细信息
                    caption: "消息", // 窗口标题
                    expand: false, // 是否展开详细信息
                    buttonText: "OK" // 确定按钮文字
                );
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }
    }
}
