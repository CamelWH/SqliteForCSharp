using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SqliteForCSharp
{
    public class SQLiteHelper
    {
        private static SqliteConnection connect;
        private static SqliteCommand command;
        private static SqliteDataReader dataReader;
        /// <summary>
        /// 连接或者创建数据库
        /// </summary>
        /// <param name="path">数据库的路径</param>
        public static void ConnectDataBase(string path)
        {
            try
            {
                if (connect != null)
                    connect.Close();
                connect = new SqliteConnection("data source=" + path);
                connect.Open();
                command = connect.CreateCommand();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 关闭数据库
        /// </summary>
        public static void CloseConnect()
        {
            //销毁Command
            if (command != null)
            {
                command.Cancel();
            }
            command = null;

            //销毁Reader
            if (dataReader != null)
            {
                dataReader.Close();
            }
            dataReader = null;

            //销毁Connection
            if (connect != null)
            {
                connect.Close();
            }
            connect = null;
        }
        /// <summary>
        /// 向数据库添加一个表,列类型为string
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="colNames">列名称</param>
        public static void CreateTable(string tableName, string[] colNames)
        {
            string[] colTypes = new string[colNames.Length];
            string queryString = "CREATE TABLE " + tableName + "( " + colNames[0] + " " + colTypes[0];
            for (int i = 1; i < colNames.Length; i++)
            {
                queryString += ", " + colNames[i] + " " + colTypes[i];
            }
            queryString += "  ) ";
            ExecuteQuery(queryString);
        }
        /// <summary>
        /// 根据自制数据表向数据库添加一个表
        /// </summary>
        /// <param name="table"></param>
        public static void CreateTable(Table table)
        {
            CreateTable(table.tableName, table.colName.ToArray());
            foreach (var item in table.List)
            {
                AddData(table.tableName, item.ToArray());
            }
        }
        /// <summary>
        /// 删除一个表
        /// </summary>
        /// <param name="tableName"></param>
        public static void DeleteTable(string tableName)
        {
            string queryString = "DROP TABLE " + tableName;
            ExecuteQuery(queryString);
        }
        /// <summary>
        /// 向一个表中添加数据
        /// </summary>
        public static void AddData(string tableName, string[] data)
        {
            string queryString = "INSERT INTO " + tableName + " VALUES (" + data[0];
            for (int i = 1; i < data.Length; i++)
            {
                queryString += ", " + "'" + data[i] + "'";
            }
            queryString += " )";
            ExecuteQuery(queryString);
        }
        /// <summary>
        /// 更新表中的某个数据
        /// </summary>
        public static void UpdateData(string tableName, string[] colNames, string[] colValues, string key, string operation, string value)
        {
            string queryString = "UPDATE " + tableName + " SET " + colNames[0] + "=" + colValues[0];
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += ", " + colNames[i] + "=" + colValues[i];
            }
            queryString += " WHERE " + key + operation + value;
            ExecuteQuery(queryString);
        }
        /// <summary>
        /// 删除表中某个数据
        /// </summary>
        public static void DeleteData(string tableName, string[] colNames, string[] operations, string[] colValues)
        {
            string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + colValues[0];
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += "OR " + colNames[i] + operations[0] + colValues[i];
            }
            ExecuteQuery(queryString);
        }
        /// <summary>
        /// 读取数据
        /// </summary>
        public static SqliteDataReader ReadData(string tableName, string[] items, string[] colNames, string[] operations, string[] colValues)
        {
            string queryString = "SELECT " + items[0];
            for (int i = 1; i < items.Length; i++)
            {
                queryString += ", " + items[i];
            }
            queryString += " FROM " + tableName + " WHERE " + colNames[0] + " " + operations[0] + " " + colValues[0];
            for (int i = 0; i < colNames.Length; i++)
            {
                queryString += " AND " + colNames[i] + " " + operations[i] + " " + colValues[0] + " ";
            }
            ExecuteQuery(queryString);
            return dataReader;
        }
        /// <summary>
        /// 读取整张表
        /// </summary>
        public static Table ReadTable(string tableName)
        {
            string queryString = "SELECT * FROM " + tableName;
            ExecuteQuery(queryString);
            Table table = new Table(tableName);
            for (int i = 0; i <  dataReader.FieldCount; i++)
            {
                table.colName.Add(dataReader.GetName(i));
            }
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            while (dataReader.Read())
            {
                Dictionary<string, string> temp = new Dictionary<string, string>();
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    temp.Add(dataReader.GetName(i), dataReader[dataReader.GetName(i)] as string);
                    Console.WriteLine(temp[dataReader.GetName(i)]);
                }
                list.Add(temp);
            }
            table.content = list;
            return table;
        }
        /// <summary>
        /// 执行指令
        /// </summary>
        private static void ExecuteQuery(string queryString)
        {
            command = connect.CreateCommand();
            command.CommandText = queryString;
            dataReader = command.ExecuteReader();
        }
    }
    /// <summary>
    /// 自制的数据表
    /// </summary>
    public class Table
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string tableName;
        /// <summary>
        /// 列名
        /// </summary>
        public List<string> colName;
        public int count{get{return content.Count;}}//数据条数
        public List<Dictionary<string, string>> content;//数据库的内容
        private List<List<string>> list = new List<List<string>>();
        public List<List<string>> List
        {
            get
            {
                foreach (var item in content)
                {
                    List<string> temp = new List<string>();
                    foreach (var v in item)
                    {
                        temp.Add(v.Value);
                    }
                    list.Add(temp);
                }
                return list;
            }
        }
        public Table(string tableName)
        {
            this.tableName = tableName;
            this.colName = new List<string>();
            this.content = new List<Dictionary<string, string>>();
        }
    }
}