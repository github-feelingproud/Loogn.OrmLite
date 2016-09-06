﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace Loogn.OrmLite
{
    /// <summary>
    /// 便捷操作类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrmLiteOperator<T> where T : new()
    {
        Func<DbConnection> openDb;
        public OrmLiteOperator(Func<DbConnection> connFunc)
        {
            openDb = connFunc;
        }

        public DbCommand Proc(string name, object inParams = null, bool execute = false)
        {
            using (var db = openDb())
            {
                return db.Proc(name, inParams, execute);
            }
        }

        public int ExecuteNonQuery(string sql, IDictionary<string, object> ps = null)
        {
            using (var db = openDb())
            {
                return db.ExecuteNonQuery(System.Data.CommandType.Text, sql, BaseCmd.GetCmd(db.GetProviderType()).DictionaryToParams(ps));
            }
        }


        #region insert
        public int Insert(T m, bool selectIdentity = false)
        {
            using (var db = openDb())
            {
                var newid = db.Insert(m, selectIdentity);
                return newid;
            }
        }

        public int Insert(string table, IDictionary<string, object> fields, bool selectIdentity = false)
        {
            using (var db = openDb())
            {
                return db.Insert(table, fields, selectIdentity);
            }
        }
        public int Insert(string table, object anonType, bool selectIdentity = false)
        {
            using (var db = openDb())
            {
                if (anonType is IDictionary<string, object>)
                {
                    return db.Insert(table, (IDictionary<string, object>)anonType, selectIdentity);
                }
                else
                {
                    return db.Insert(table, anonType, selectIdentity);
                }

            }
        }

        public bool Insert(string table, params object[] objs)
        {
            using (var db = openDb())
            {
                return db.Insert(table, objs);
            }
        }

        public bool InsertAll(string table, IEnumerable objs)
        {
            using (var db = openDb())
            {
                return db.InsertAll(table, objs);
            }
        }

        public bool Insert(params T[] objs)
        {
            using (var db = openDb())
            {
                return db.Insert(objs);
            }
        }

        public bool InsertAll(IEnumerable<T> objs)
        {
            using (var db = openDb())
            {
                return db.InsertAll(objs);
            }
        }
        #endregion

        #region update
        public int Update(T obj, params string[] updateFields)
        {
            using (var db = openDb())
            {
                return db.Update(obj, updateFields);
            }
        }

        public int UpdateAnonymous(string tableName, object anonymous)
        {
            using (var db = openDb())
            {
                return db.UpdateAnonymous(tableName, anonymous);
            }
        }

        public int UpdateAnonymous(object anonymous)
        {
            using (var db = openDb())
            {
                return db.UpdateAnonymous<T>(anonymous);
            }
        }

        public int Update(params T[] objs)
        {
            return UpdateAll(objs);
        }

        public int UpdateAll(IEnumerable<T> objs)
        {
            using (var db = openDb())
            {
                return db.UpdateAll(objs);
            }
        }

        public int Update(IDictionary<string, object> updateFields, string conditions, IDictionary<string, object> parameters)
        {
            using (var db = openDb())
            {
                return db.Update<T>(updateFields, conditions, parameters);
            }
        }

        public int UpdateById(IDictionary<string, object> updateFields, object id, string idname = OrmLite.KeyName)
        {
            using (var db = openDb())
            {
                return db.UpdateById<T>(updateFields, id, idname);
            }
        }


        public int UpdateFieldById(string fieldName, object fieldValue, object id, string idname = OrmLite.KeyName)
        {
            using (var db = openDb())
            {
                return db.UpdateFieldById<T>(fieldName, fieldValue, id, idname);
            }
        }
        #endregion

        #region delete
        public int Delete(string sql, IDictionary<string, object> parameters = null)
        {
            using (var db = openDb())
            {
                return db.Delete(sql, parameters);
            }
        }

        public int Delete(IDictionary<string, object> conditions)
        {
            using (var db = openDb())
            {
                return db.Delete<T>(conditions);
            }
        }

        public int DeleteById(object id, string idField = OrmLite.KeyName)
        {
            using (var db = openDb())
            {
                return db.DeleteById<T>(id, idField);
            }
        }

        public int DeleteByIds(IEnumerable idValues, string idField = OrmLite.KeyName)
        {
            using (var db = openDb())
            {
                return db.DeleteByIds<T>(idValues, idField);
            }
        }

        public int Delete()
        {
            using (var db = openDb())
            {
                return db.Delete<T>();
            }
        }
        #endregion

        #region select
        public List<T> Select()
        {
            using (var db = openDb())
            {
                return db.Select<T>();
            }
        }

        public List<T> Select(string sql)
        {
            using (var db = openDb())
            {
                return db.Select<T>(sql);
            }
        }

        public List<T> Select(string sql, IDictionary<string, object> parameters)
        {
            using (var db = openDb())
            {
                return db.Select<T>(sql, parameters);
            }
        }

        public List<T> Select(string sql, object parameters)
        {
            using (var db = openDb())
            {
                return db.Select<T>(sql, parameters);
            }
        }

        public List<T> SelectWhere(string name, object value)
        {
            using (var db = openDb())
            {
                return db.SelectWhere<T>(name, value);
            }
        }

        public List<T> SelectWhere(IDictionary<string, object> conditions)
        {
            using (var db = openDb())
            {
                return db.SelectWhere<T>(conditions);
            }
        }

        public List<T> SelectWhere(object conditions)
        {
            using (var db = openDb())
            {
                return db.SelectWhere<T>(conditions);
            }
        }

        public List<T> SelectFmt(string sqlFormat, params object[] parameters)
        {
            using (var db = openDb())
            {
                return db.SelectFmt<T>(sqlFormat, parameters);
            }
        }

        public List<T> SelectByIds(IEnumerable idValues, string idField = OrmLite.KeyName, string selectFields = "*")
        {
            using (var db = openDb())
            {
                return db.SelectByIds<T>(idValues, idField, selectFields);
            }
        }

        public List<T> SelectPage(OrmLitePageFactor factor, out int totalCount)
        {
            using (var db = openDb())
            {
                return db.SelectPage<T>(factor, out totalCount);
            }
        }

        #endregion

        #region single
        public T Single(IDictionary<string, object> conditions)
        {
            using (var db = openDb())
            {
                return db.Single<T>(conditions);
            }
        }

        public T Single(object conditions)
        {
            using (var db = openDb())
            {
                return db.Single<T>(conditions);
            }
        }

        public T Single(string sql)
        {
            using (var db = openDb())
            {
                return db.Single<T>(sql);
            }
        }

        public T Single(string sql, IDictionary<string, object> parameters)
        {
            using (var db = openDb())
            {
                return db.Single<T>(sql, parameters);
            }
        }

        public T SingleFmt(string sqlFormat, params object[] parameters)
        {
            using (var db = openDb())
            {
                return db.SingleFmt<T>(sqlFormat, parameters);
            }
        }

        public T SingleById(object idValue, string idField = OrmLite.KeyName)
        {
            using (var db = openDb())
            {
                return db.SingleById<T>(idValue, idField);
            }
        }

        public T SingleWhere(string name, object value)
        {
            using (var db = openDb())
            {
                return db.SingleWhere<T>(name, value);
            }
        }

        public T SingleWhere(IDictionary<string, object> conditions)
        {
            using (var db = openDb())
            {
                return db.SingleWhere<T>(conditions);
            }
        }

        public T SingleWhere(object conditions)
        {
            using (var db = openDb())
            {
                return db.SingleWhere<T>(conditions);
            }
        }
        #endregion

        #region Scalar

        public RetType Scalar<RetType>(string sql)
        {
            using (var db = openDb())
            {
                return db.Scalar<RetType>(sql);
            }
        }

        public RetType Scalar<RetType>(string sql, IDictionary<string, object> parameters)
        {
            using (var db = openDb())
            {
                return db.Scalar<RetType>(sql, parameters);
            }
        }

        public RetType Scalar<RetType>(string sql, object parameters)
        {
            using (var db = openDb())
            {
                return db.Scalar<RetType>(sql, parameters);
            }
        }

        public RetType ScalarFmt<RetType>(string sqlFormat, params object[] parameters)
        {
            using (var db = openDb())
            {
                return db.ScalarFmt<RetType>(sqlFormat, parameters);
            }
        }

        #endregion

        #region Column

        public List<TField> Column<TField>(string sql)
        {
            using (var db = openDb())
            {
                return db.Column<TField>(sql);
            }
        }

        public List<TField> Column<TField>(string sql, IDictionary<string, object> parameters)
        {
            using (var db = openDb())
            {
                return db.Column<TField>(sql, parameters);
            }
        }

        public List<TField> Column<TField>(string sql, object parameters)
        {
            using (var db = openDb())
            {
                return db.Column<TField>(sql, parameters);
            }
        }

        public List<TField> ColumnFmt<TField>(string sqlFormat, params object[] parameters)
        {
            using (var db = openDb())
            {
                return db.ColumnFmt<TField>(sqlFormat, parameters);
            }
        }

        public HashSet<TField> ColumnDistinct<TField>(string sql)
        {
            using (var db = openDb())
            {
                return db.ColumnDistinct<TField>(sql);
            }
        }

        public HashSet<TField> ColumnDistinct<TField>(string sql, IDictionary<string, object> parameters)
        {
            using (var db = openDb())
            {
                return db.ColumnDistinct<TField>(sql, parameters);
            }
        }

        public HashSet<TField> ColumnDistinct<TField>(string sql, object parameters)
        {
            using (var db = openDb())
            {
                return db.ColumnDistinct<TField>(sql, parameters);
            }
        }

        public HashSet<TField> ColumnDistinctFmt<TField>(string sqlFormat, params object[] parameters)
        {
            using (var db = openDb())
            {
                return db.ColumnDistinctFmt<TField>(sqlFormat, parameters);
            }
        }

        #endregion

        #region Lookup Dictionary
        public Dictionary<K, List<V>> Lookup<K, V>(string sql)
        {
            using (var db = openDb())
            {
                return db.Lookup<K, V>(sql);
            }
        }

        public Dictionary<K, List<V>> Lookup<K, V>(string sql, IDictionary<string, object> parameters)
        {
            using (var db = openDb())
            {
                return db.Lookup<K, V>(sql, parameters);
            }
        }

        public Dictionary<K, List<V>> Lookup<K, V>(string sql, object parameters)
        {
            using (var db = openDb())
            {
                return db.Lookup<K, V>(sql, parameters);
            }
        }

        public Dictionary<K, List<V>> LookupFmt<K, V>(string sqlFormat, params object[] parameters)
        {
            using (var db = openDb())
            {
                return db.LookupFmt<K, V>(sqlFormat, parameters);
            }
        }

        public Dictionary<K, V> Dictionary<K, V>(string sql)
        {
            using (var db = openDb())
            {
                return db.Dictionary<K, V>(sql);
            }
        }

        public Dictionary<K, V> Dictionary<K, V>(string sql, IDictionary<string, object> parameters)
        {
            using (var db = openDb())
            {
                return db.Dictionary<K, V>(sql, parameters);
            }
        }
        public Dictionary<K, V> Dictionary<K, V>(string sql, object parameters)
        {
            using (var db = openDb())
            {
                return db.Dictionary<K, V>(sql, parameters);
            }
        }

        public Dictionary<K, V> DictionaryFmt<K, V>(string sqlFormat, params object[] parameters)
        {
            using (var db = openDb())
            {
                return db.DictionaryFmt<K, V>(sqlFormat, parameters);
            }
        }
        #endregion

        #region Count
        public int Count()
        {
            using (var db = openDb())
            {
                return db.Count<T>();
            }
        }

        public int Count(string sql)
        {
            using (var db = openDb())
            {
                return db.Count<T>(sql);
            }
        }

        public int Count(string sql, IDictionary<string, object> parameters)
        {
            using (var db = openDb())
            {
                return db.Count<T>(sql, parameters);
            }
        }

        public int Count(string sql, object parameters)
        {
            using (var db = openDb())
            {
                return db.Count<T>(sql, parameters);
            }
        }

        public int CountWhere(string name, object value)
        {
            using (var db = openDb())
            {
                return db.CountWhere<T>(name, value);
            }
        }

        public int CountWhere(IDictionary<string, object> conditions)
        {
            using (var db = openDb())
            {
                return db.CountWhere<T>(conditions);
            }
        }

        public int CountWhere(object conditions)
        {
            using (var db = openDb())
            {
                return db.CountWhere<T>(conditions);
            }
        }

        public int CountFmt(string sqlFormat, params object[] parameters)
        {
            using (var db = openDb())
            {
                return db.CountFmt(sqlFormat, parameters);
            }
        }

        public TField MaxId<TField>(string field = "ID")
        {
            using (var db = openDb())
            {
                var tableName = ReflectionHelper.GetInfo<T>().TableName;
                return db.MaxID<TField>(tableName, field);
            }
        }
        #endregion

    }
}
