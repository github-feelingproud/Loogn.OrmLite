﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace Loogn.OrmLite
{
    public static partial class OrmLiteWriteApi
    {
        public static DbCommand Proc(this DbTransaction dbTrans, string name, object inParams = null, bool excludeDefaults = false)
        {
            var cmd = dbTrans.Connection.CreateCommand();
            cmd.Transaction = dbTrans;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = name;
            if (inParams != null)
            {
                var ps = ORM.AnonTypeToParams(dbTrans.GetProviderType(), inParams);
                cmd.Parameters.AddRange(ps);
            }
            if (excludeDefaults)
            {
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            return cmd;
        }

        public static int ExecuteNonQuery(this DbTransaction dbTrans, CommandType commandType, string commandText, params DbParameter[] ps)
        {
            OrmLite.SetSqlStringBuilderCapacity(commandText);
            return SqlHelper.ExecuteNonQuery(dbTrans, commandType, commandText, ps);
        }
        public static int ExecuteNonQuery(this DbTransaction dbTrans, CommandType commandType, string commandText, Dictionary<string, object> parameters)
        {
            OrmLite.SetSqlStringBuilderCapacity(commandText);
            return SqlHelper.ExecuteNonQuery(dbTrans, commandType, commandText, ORM.DictionaryToParams(dbTrans.GetProviderType(), parameters));
        }

        public static object ExecuteScalar(this DbTransaction dbTrans, CommandType commandType, string commandText, params DbParameter[] ps)
        {
            OrmLite.SetSqlStringBuilderCapacity(commandText);
            return SqlHelper.ExecuteScalar(dbTrans, commandType, commandText, ps);
        }
        public static object ExecuteScalar(this DbTransaction dbTrans, CommandType commandType, string commandText, Dictionary<string, object> parameters)
        {
            OrmLite.SetSqlStringBuilderCapacity(commandText);
            return SqlHelper.ExecuteNonQuery(dbTrans, commandType, commandText, ORM.DictionaryToParams(dbTrans.GetProviderType(), parameters));
        }

        public static int Insert<T>(this DbTransaction dbTrans, T obj, bool selectIdentity = false)
        {
            var tuple = SqlCmd.Insert<T>(dbTrans.GetProviderType(), obj, selectIdentity);
            if (selectIdentity)
            {
                var identity = ExecuteScalar(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
                if (identity == null || identity is DBNull)
                {
                    return 0;
                }
                return Convert.ToInt32(identity);
            }
            else
            {
                var raw = ExecuteNonQuery(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
                return raw;
            }
        }

        public static int Insert(this DbTransaction dbTrans, string table, Dictionary<string, object> fields, bool selectIdentity = false)
        {
            var tuple = SqlCmd.Insert(dbTrans.GetProviderType(), table, fields, selectIdentity);
            if (selectIdentity)
            {
                var identity = ExecuteScalar(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
                if (identity == null || identity is DBNull)
                {
                    return 0;
                }
                return Convert.ToInt32(identity);
            }
            else
            {
                var raw = ExecuteNonQuery(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
                return raw;
            }
        }

        public static int Insert(this DbTransaction dbTrans, string table, object anonType, bool selectIdentity = false)
        {
            var tuple = SqlCmd.Insert(dbTrans.GetProviderType(), table, anonType, selectIdentity);
            if (selectIdentity)
            {
                var identity = ExecuteScalar(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
                if (identity == null || identity is DBNull)
                {
                    return 0;
                }
                return Convert.ToInt32(identity);
            }
            else
            {
                var raw = ExecuteNonQuery(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
                return raw;
            }
        }

        private static int InsertTrans<T>( this DbTransaction dbTrans, OrmLiteProviderType type, T obj)
        {
            var objtype = typeof(T);
            var table = objtype.GetCachedTableName();
            var propertys = objtype.GetCachedProperties();

            StringBuilder sbsql = new StringBuilder(OrmLite.SqlStringBuilderCapacity);
            sbsql.AppendFormat("insert into {0} (", table);
            StringBuilder sbParams = new StringBuilder(") values (", OrmLite.SqlStringBuilderCapacity);
            var ps = new List<DbParameter>();
            var provider = OrmLite.GetProvider(type);
            foreach (var property in propertys)
            {
                var fieldAttr = (OrmLiteFieldAttribute)property.GetCachedCustomAttributes(typeof(OrmLiteFieldAttribute)).FirstOrDefault();
                if (property.Name.Equals(OrmLite.DefaultKeyName, StringComparison.OrdinalIgnoreCase))
                {
                    if (fieldAttr == null || (!fieldAttr.InsertRequire))
                    {
                        continue;
                    }
                }
                if (fieldAttr == null || (!fieldAttr.InsertIgnore && !fieldAttr.Ignore))
                {
                    var fieldName = property.Name;

                    if (fieldAttr != null && fieldAttr.Name != null && fieldAttr.Name.Length > 0)
                    {
                        fieldName = fieldAttr.Name;
                    }
                    var val = property.GetValue(obj, null);
                    if (val == null)
                    {
                        if (property.PropertyType == typeof(string))
                        {
                            val = string.Empty;
                        }
                    }
                    else
                    {
                        if (property.PropertyType == typeof(DateTime) && (DateTime)val == DateTime.MinValue)
                        {
                            val = DateTime.Now;
                        }
                    }
                    sbsql.AppendFormat("{0},", fieldName);
                    sbParams.AppendFormat("@{0},", fieldName);
                    ps.Add(provider.CreateParameter("@" + fieldName, val ?? DBNull.Value));
                }
            }
            if (ps.Count == 0)
            {
                throw new ArgumentException("model里没有字段，无法插入");
            }
            sbsql.Remove(sbsql.Length - 1, 1);
            sbParams.Remove(sbParams.Length - 1, 1);
            sbsql.Append(sbParams.ToString());
            sbsql.Append(")");
            var raw = ExecuteNonQuery(dbTrans, CommandType.Text, sbsql.ToString(), ps.ToArray());
            return raw;
        }

        private static int InsertTrans(this DbTransaction dbTrans, OrmLiteProviderType type, string table, object anonType)
        {
            var objtype = anonType.GetType();
            var propertys = objtype.GetCachedProperties();
            StringBuilder sbsql = new StringBuilder(OrmLite.SqlStringBuilderCapacity);
            sbsql.AppendFormat("insert into {0} (", table);
            StringBuilder sbParams = new StringBuilder(") values (", OrmLite.SqlStringBuilderCapacity);
            var ps = new List<DbParameter>();
            var provider = OrmLite.GetProvider(type);
            foreach (var property in propertys)
            {
                var fieldName = property.Name;
                var val = property.GetValue(anonType, null);
                sbsql.AppendFormat("{0},", fieldName);
                sbParams.AppendFormat("@{0},", fieldName);
                ps.Add(provider.CreateParameter("@" + fieldName, val ?? DBNull.Value));
            }
            if (ps.Count == 0)
            {
                throw new ArgumentException("model里没有字段，无法插入");
            }
            sbsql.Remove(sbsql.Length - 1, 1);
            sbParams.Remove(sbParams.Length - 1, 1);
            sbsql.Append(sbParams.ToString());
            sbsql.Append(")");
            var raw = ExecuteNonQuery(dbTrans, CommandType.Text, sbsql.ToString(), ps.ToArray());
            return raw;
        }

        public static void Insert(this DbTransaction dbTrans, string table, params object[] objs)
        {
            InsertAll(dbTrans, table, objs);
        }
        public static bool InsertAll(this DbTransaction dbTrans, string table, IEnumerable objs)
        {
            if (objs != null)
            {
                var providerType = dbTrans.GetProviderType();
                foreach (var obj in objs)
                {
                    var rowCount = InsertTrans(dbTrans, providerType, table, obj);
                    if (rowCount == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool Insert<T>(this DbTransaction dbTrans, params T[] objs)
        {
            return InsertAll<T>(dbTrans, objs);
        }

        public static bool InsertAll<T>(this DbTransaction dbTrans, IEnumerable<T> objs)
        {
            if (objs != null)
            {
                var providerType = dbTrans.GetProviderType();
                foreach (var obj in objs)
                {
                    var rowCount = InsertTrans<T>(dbTrans, providerType, obj);
                    if (rowCount == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static int Update<T>(this DbTransaction dbTrans, T obj, params string[] updateFields)
        {
            var tuple = SqlCmd.Update<T>(dbTrans.GetProviderType(), obj, updateFields);
            int c = ExecuteNonQuery(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
            return c;
        }

        public static int UpdateAnonymous(this DbTransaction dbTrans, string tableName, object anonymous)
        {
            var tuple = SqlCmd.Update(dbTrans.GetProviderType(), tableName, anonymous);
            int c = ExecuteNonQuery(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
            return c;
        }

        public static int UpdateAnonymous<T>(this DbTransaction dbTrans, object anonymous)
        {
            var tuple = SqlCmd.Update(dbTrans.GetProviderType(), typeof(T).GetCachedTableName(), anonymous);
            int c = ExecuteNonQuery(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
            return c;
        }

        public static int UpdateAnonymous(this DbTransaction dbTrans, object model, object anonymous)
        {
            var tuple = SqlCmd.Update(dbTrans.GetProviderType(), model.GetType().GetCachedTableName(), anonymous);
            int c = ExecuteNonQuery(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
            return c;
        }

        private static int UpdateTrans<T>(this DbTransaction dbTrans, OrmLiteProviderType type, T obj)
        {
            var objtype = typeof(T);
            var table = objtype.GetCachedTableName();
            var propertys = objtype.GetCachedProperties();
            StringBuilder sbsql = new StringBuilder(OrmLite.SqlStringBuilderCapacity);
            sbsql.AppendFormat("update {0} set ", table);
            string condition = null;
            var ps = new List<DbParameter>();
            var provider = OrmLite.GetProvider(type);
            foreach (var property in propertys)
            {
                var fieldAttr = (OrmLiteFieldAttribute)property.GetCachedCustomAttributes(typeof(OrmLiteFieldAttribute)).FirstOrDefault();
                if (fieldAttr == null || (!fieldAttr.UpdateIgnore && !fieldAttr.Ignore))
                {
                    var fieldName = property.Name;
                    if (fieldAttr != null && fieldAttr.Name != null && fieldAttr.Name.Length > 0)
                    {
                        fieldName = fieldAttr.Name;
                    }

                    var val = property.GetValue(obj, null);
                    if (val == null)
                    {
                        if (property.PropertyType == typeof(string))
                        {
                            continue;
                        }
                    }
                    if (property.PropertyType == typeof(DateTime) && (DateTime)val == DateTime.MinValue)
                    {
                        continue;
                    }
                    if (fieldName.Equals(OrmLite.DefaultKeyName, StringComparison.OrdinalIgnoreCase) || (fieldAttr != null && fieldAttr.IsPrimaryKey))
                    {
                        condition = string.Format("{0} = @{0}", fieldName);
                    }
                    else
                    {
                        sbsql.AppendFormat("{0} = @{0},", fieldName);
                    }
                    ps.Add(provider.CreateParameter("@" + fieldName, val ?? DBNull.Value));
                }
            }
            if (ps.Count == 0)
            {
                throw new ArgumentException("model里没有字段，无法修改");
            }
            sbsql.Remove(sbsql.Length - 1, 1);
            sbsql.AppendFormat(" where ");
            sbsql.Append(condition);
            int c = ExecuteNonQuery(dbTrans, CommandType.Text, sbsql.ToString(), ps.ToArray());
            return c;
        }

        public static int Update<T>(this DbTransaction dbTrans, params T[] objs)
        {
            return UpdateAll<T>(dbTrans, objs);
        }

        public static int UpdateAll<T>(this DbTransaction dbTrans, IEnumerable<T> objs)
        {
            int row = 0;
            var providerType = dbTrans.GetProviderType();
            foreach (var obj in objs)
            {
                var rowCount = UpdateTrans<T>(dbTrans, providerType, obj);
                row++;
            }
            return row;
        }

        public static int Update<T>(this DbTransaction dbTrans, Dictionary<string, object> updateFields, string conditions, Dictionary<string, object> parameters)
        {
            var tuple = SqlCmd.Update(dbTrans.GetProviderType(),typeof(T).GetCachedTableName(), updateFields, conditions, parameters);
            int c = ExecuteNonQuery(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
            return c;
        }

        public static int Update(this DbTransaction dbTrans, string tableName, Dictionary<string, object> updateFields, string conditions, Dictionary<string, object> parameters)
        {
            var tuple = SqlCmd.Update(dbTrans.GetProviderType(),tableName, updateFields, conditions, parameters);

            int c = ExecuteNonQuery(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
            return c;
        }

        public static int UpdateById(this DbTransaction dbTrans, string tableName, Dictionary<string, object> updateFields, object id, string idname = OrmLite.KeyName)
        {
            return Update(dbTrans, tableName, updateFields, idname + "=@id", DictBuilder.Assign("id", id));
        }

        public static int UpdateById<T>(this DbTransaction dbTrans, Dictionary<string, object> updateFields, object id, string idname = OrmLite.KeyName)
        {
            return Update<T>(dbTrans, updateFields, idname + "=@id", DictBuilder.Assign("id", id));
        }

        public static int UpdateFieldById<T>(this DbTransaction dbTrans, string fieldName, object fieldValue, object id, string idname = OrmLite.KeyName)
        {
            return Update<T>(dbTrans, DictBuilder.Assign(fieldName, fieldValue), idname + "=@id", DictBuilder.Assign("id", id));
        }

        public static int Delete(this DbTransaction dbTrans, string sql, Dictionary<string, object> parameters = null)
        {
            return ExecuteNonQuery(dbTrans, CommandType.Text, sql, ORM.DictionaryToParams(dbTrans.GetProviderType(), parameters));
        }

        public static int Delete<T>(this DbTransaction dbTrans, Dictionary<string, object> conditions)
        {
            var tuple = SqlCmd.Delete<T>(dbTrans.GetProviderType(), conditions);
            return ExecuteNonQuery(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
        }

        public static int DeleteById<T>(this DbTransaction dbTrans, object id, string idField = OrmLite.KeyName)
        {
            var tuple = SqlCmd.DeleteById<T>(dbTrans.GetProviderType(), id, idField);
            return ExecuteNonQuery(dbTrans, CommandType.Text, tuple.Item1, tuple.Item2);
        }

        public static int DeleteByIds<T>(this DbTransaction dbTrans, IEnumerable idValues, string idFields = OrmLite.KeyName)
        {
            var sql = SqlCmd.DeleteByIds<T>(idValues, idFields);
            if (sql == null || sql.Length == 0) return 0;
            return ExecuteNonQuery(dbTrans, CommandType.Text, sql);
        }

        public static int Delete<T>(this DbTransaction dbTrans)
        {
            var sql = SqlCmd.Delete<T>();
            return ExecuteNonQuery(dbTrans, CommandType.Text, sql);
        }

    }
}
