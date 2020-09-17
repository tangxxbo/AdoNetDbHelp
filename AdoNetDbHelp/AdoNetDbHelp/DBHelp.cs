using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace AdoNetDbHelp
{
    public class DBHelp
    {
        private static string connSql = ConfigurationManager.ConnectionStrings["connSql"].ConnectionString;
        SqlConnection conn = null;

        // <summary>
        // 封装了数据库连接
        // </summary>
        // <returns></returns>
        //private SqlConnection GetConn() {
        //    conn = new SqlConnection(connSql);
        //    return conn;
        //}

        //Command的三种执行方法
        //ExecuteNonQuery() 执行T-SQL，返回受影响的行数
        //ExecuteSclar() 执行查询，返回第一行第一列的值，忽略其它行列的值
        //ExecuteReader() 执行查询，生成SqlDataRead对象，一行行的读取使用while(xxx.Read()){}
        /// <summary>
        /// 封装了查询数据库获取返回数据行数方法
        /// </summary>
        /// <param name="sql">传入的查询语句</param>
        /// <param name="cmdType">定义传入的查询语句类型是什么</param>
        /// <param name="paras">定义传入参数查询条件</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql,int cmdType, params SqlParameter[] paras) {
            int count = 0;
            using (SqlConnection conn = new SqlConnection(connSql)) {
                //SqlCommand cmd = new SqlCommand(sql,conn);
                //if (cmdType == 2)
                //    cmd.CommandType = CommandType.StoredProcedure;
                //if (paras!=null && paras.Length>0)
                //{
                //    cmd.Parameters.AddRange(paras);
                //}
                //conn.Open();
                SqlCommand cmd = BuildCommand(sql, conn, cmdType, null, paras);
                count = cmd.ExecuteNonQuery();
                conn.Close();
            } ;
            return count;
        }

        /// <summary>
        /// 封装查询数据库ExecuteSclar返回一行一列的对象
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="cmdType"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static object ExecuteSclar(string sql, int cmdType, params SqlParameter[] paras)
        {
            object obj = null;
            using (SqlConnection conn = new SqlConnection(connSql))
            {
                /*SqlCommand cmd = new SqlCommand(sql, conn);
                if (cmdType == 2)
                    cmd.CommandType = CommandType.StoredProcedure;
                if (paras != null && paras.Length > 0)
                {
                    cmd.Parameters.AddRange(paras);
                }
                conn.Open();*/
                SqlCommand cmd = BuildCommand(sql, conn, cmdType, null, paras);
                obj = cmd.ExecuteScalar();
                conn.Close();
            };
            return obj;
        }
        /// <summary>
        /// 少量数据查询，获取对象通过ExecuteReader方式，返回的是SqlDataReader对象,然后对象返回后使用while(XXX.READ)读取存储数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="cmdType"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(string sql, int cmdType, params SqlParameter[] paras)
        {
            SqlDataReader sdr = null;
            SqlConnection conn = new SqlConnection(connSql);
            //SqlCommand cmd = new SqlCommand(sql, conn);

            //if (cmdType == 2)
            //    cmd.CommandType = CommandType.StoredProcedure;
            //if (paras != null && paras.Length > 0)
            //{
            //    cmd.Parameters.AddRange(paras);
            //}
            try
            {
                //conn.Open();
                SqlCommand cmd = BuildCommand(sql, conn, cmdType, null, paras);
                sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (SqlException ex)
            {
                conn.Close();
                throw new Exception("执行查询异常",ex);
            }
            return sdr;
        }


        public static DataSet GetDataSet(string sql, int cmdType, params SqlParameter[] paras) {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connSql)) {
                //必须先创建SqlCommand对象识别是否存储对象
                //SqlCommand cmd = new SqlCommand(sql,conn);
                //if (cmdType == 2)
                //    cmd.CommandType = CommandType.StoredProcedure;
                //if (paras!=null && paras.Length>0)
                //{
                //    cmd.Parameters.AddRange(paras);
                //}
                SqlCommand cmd = BuildCommand(sql, conn, cmdType, null, paras);
                SqlDataAdapter sd = new SqlDataAdapter(cmd);
                conn.Open();
                sd.Fill(ds);
                conn.Close();
            };

            return ds;
        }

        /// <summary>
        /// 填充1个结果集
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="cmdType"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(string sql, int cmdType, params SqlParameter[] paras)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connSql))
            {
                //必须先创建SqlCommand对象识别是否存储对象
                //SqlCommand cmd = new SqlCommand(sql,conn);
                //if (cmdType == 2)
                //    cmd.CommandType = CommandType.StoredProcedure;
                //if (paras != null && paras.Length > 0)
                //{
                //    cmd.Parameters.AddRange(paras);
                //}
                SqlCommand cmd = BuildCommand(sql, conn, cmdType, null, paras);
                SqlDataAdapter sd = new SqlDataAdapter(cmd);
                conn.Open();
                sd.Fill(dt);
                conn.Close();
            };

            return dt;
        }
        /// <summary>
        /// 不带参数的事务执行
        /// </summary>
        /// <param name="listSql"></param>
        /// <returns></returns>
        public static bool ExecuteTrans(List<string> listSql) {

            using (SqlConnection conn = new SqlConnection(connSql))
            {
                conn.Open();
                SqlTransaction sqlTransaction = conn.BeginTransaction();
                SqlCommand sqlCommand = conn.CreateCommand();
                sqlCommand.Transaction = sqlTransaction;
                try
                {
                    for (int i = 0; i < listSql.Count; i++)
                    {
                        sqlCommand.CommandText = listSql[i].ToString();
                        sqlCommand.ExecuteNonQuery();
                    }
                    sqlTransaction.Commit();
                    return true;
                }
                catch (SqlException ex)
                {
                    sqlTransaction.Rollback();
                    throw new Exception("语句执行异常",ex);
                }
                finally
                {
                    sqlTransaction.Dispose();
                    sqlCommand.Dispose();
                    conn.Close();
                }

            }
            
        }
        /// <summary>
        /// 带参数的事务执行，将操作封装CmdInfo中
        /// </summary>
        /// <param name="listCmd"></param>
        /// <returns></returns>
        public static bool ExecuteTrans(List<CmdInfo> listCmd)
        {

            using (SqlConnection conn = new SqlConnection(connSql))
            {
                conn.Open();
                SqlTransaction sqlTransaction = conn.BeginTransaction();
                SqlCommand sqlCommand = conn.CreateCommand();
                sqlCommand.Transaction = sqlTransaction;
                try
                {
                    for (int i = 0; i < listCmd.Count; i++)
                    {
                        sqlCommand.CommandText = listCmd[i].CommandText;
                        if (listCmd[i].CmdType == 2)
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.Clear();
                        if (listCmd[i].Parameters!=null && listCmd[i].Parameters.Length>0)
                            sqlCommand.Parameters.AddRange(listCmd[i].Parameters);

                        sqlCommand.ExecuteNonQuery();
                        sqlCommand.Parameters.Clear();
                    }
                    sqlTransaction.Commit();
                    return true;
                }
                catch (SqlException ex)
                {
                    sqlTransaction.Rollback();
                    throw new Exception("语句执行异常", ex);
                }
                finally {
                    sqlTransaction.Dispose();
                    sqlCommand.Dispose();
                    conn.Close();
                }
            }
        }
        private static SqlCommand BuildCommand(string sql,SqlConnection conn,int cmdType,SqlTransaction trans,params SqlParameter[] parameters) {

            SqlCommand cmd = new SqlCommand();
            if (cmdType == 2)
                cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            if (trans != null)
                cmd.Transaction = trans;

            return cmd;
        }
    }
}
