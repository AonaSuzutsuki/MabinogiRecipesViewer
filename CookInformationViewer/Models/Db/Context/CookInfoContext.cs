using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CommonExtensionLib.Extensions;
using CookInformationViewer.Models.Db.Raw;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CookInformationViewer.Models.Db.Context
{
    public interface IDbSet
    {
        void Remove(IDbElement target);
    }

    public class DbSetWrapper<T> : IDbSet, IEnumerable<T> where T : class, IDbElement
    {
        private readonly DbSet<T> _dbSet;

        public DbSetWrapper(DbSet<T> dbSet)
        {
            _dbSet = dbSet;
        }

        public void Remove(IDbElement target)
        {
            var elem = target as T;
            if (elem == null)
                return;
            _dbSet.Remove(elem);
        }

        public IEnumerable<T> FromSqlRaw(string sql)
        {
            return _dbSet.FromSqlRaw(sql);
        }

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerable<T> enumerable = _dbSet;
            return enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CookInfoContext : DbContext
    {
#if DEBUG && LOG
        private readonly FileStream _logStream;
#endif

        private readonly Dictionary<Type, IDbSet> _dbMaps;
        
        public DbSet<DbCookMaterials> CookMaterials { get; set; }
        public DbSet<DbCookCategories> CookCategories { get; set; }
        public DbSet<DbCookLocations> CookLocations { get; set; }
        public DbSet<DbCookSellers> CookSellers { get; set; }
        public DbSet<DbCookRecipes> CookRecipes { get; set; }
        public DbSet<DbCookMaterialSellers> CookMaterialSellers { get; set; }
        public DbSet<DbCookMaterialDrops> CookMaterialDrops { get; set; }
        public DbSet<DbCookEffects> CookEffects { get; set; }
        public DbSet<DbCookAdditionals> Additionals { get; set; }
        public DbSet<DbDownloadHistory> Histories { get; set; }
        public DbSet<DbCookFavorites> Favorites { get; set; }

        public CookInfoContext()
        {
#if DEBUG && LOG
            _logStream = new FileStream("context.log", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
#endif

            _dbMaps = new Dictionary<Type, IDbSet>
            {
                { typeof(DbCookMaterials), new DbSetWrapper<DbCookMaterials>(CookMaterials) },
                { typeof(DbCookCategories), new DbSetWrapper<DbCookCategories>(CookCategories) },
                { typeof(DbCookLocations), new DbSetWrapper<DbCookLocations>(CookLocations) },
                { typeof(DbCookSellers), new DbSetWrapper<DbCookSellers>(CookSellers) },
                { typeof(DbCookRecipes), new DbSetWrapper<DbCookRecipes>(CookRecipes) },
                { typeof(DbCookMaterialSellers), new DbSetWrapper<DbCookMaterialSellers>(CookMaterialSellers) },
                { typeof(DbCookMaterialDrops), new DbSetWrapper<DbCookMaterialDrops>(CookMaterialDrops) },
                { typeof(DbCookEffects), new DbSetWrapper<DbCookEffects>(CookEffects) },
                { typeof(DbDownloadHistory), new DbSetWrapper<DbDownloadHistory>(Histories) },
                { typeof(DbCookAdditionals), new DbSetWrapper<DbCookAdditionals>(Additionals) },
                { typeof(DbCookFavorites), new DbSetWrapper<DbCookFavorites>(Favorites) }
            };
        }


        public IEnumerable<T> FromSqlRaw<T>(SqlCreator creator) where T : class, IDbElement
        {
            var sql = creator.ToString(false);
            var set = GetDbSet<T>();
            if (set != null)
                return set.FromSqlRaw(sql);
            return new List<T>();
        }

        public void Remove(Type type, IDbElement target)
        {
            var set = _dbMaps.Get(type);
            if (set == null)
                return;

            set.Remove(target);
        }

        public DbSetWrapper<T> GetDbSet<T>() where T : class, IDbElement
        {
            if (_dbMaps.ContainsKey(typeof(T)))
            {
                if (_dbMaps[typeof(T)] is DbSetWrapper<T> set)
                    return set;
            }
            return null;
        }

        public IEnumerable<IDbElement> GetDbEnumerable(Type type)
        {
            var set = _dbMaps.Get(type);
            if (!(set is IEnumerable<IDbElement> enumerable))
                return null;
            return enumerable;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={Constants.DatabaseFileName}");
#if DEBUG
            optionsBuilder.LogTo(x =>
            {
#if LOG
                var data = new UTF8Encoding(false).GetBytes(x);
                _logStream.Write(data);
                _logStream.Flush();
#endif
                Debug.WriteLine(x);
            });
#endif
        }

        public static Type ConvertType(ManageType manageType)
        {
            var typeMap = new Dictionary<ManageType, Type>
            {
                { ManageType.CookMaterials, typeof(DbCookMaterials) },
                { ManageType.CookCategories, typeof(DbCookCategories) },
                { ManageType.CookLocations, typeof(DbCookLocations) },
                { ManageType.CookSellers, typeof(DbCookSellers) },
                { ManageType.CookRecipes, typeof(DbCookRecipes) },
                { ManageType.CookMaterialSellers, typeof(DbCookMaterialSellers) },
                { ManageType.CookMaterialDrops, typeof(DbCookMaterialDrops) },
                { ManageType.CookEffects, typeof(DbCookEffects) },
                { ManageType.History, typeof(DbDownloadHistory) },
                { ManageType.CookAdditionals, typeof(DbCookAdditionals) },
                { ManageType.CookFavorites, typeof(DbCookFavorites) }
            };

            return typeMap.Get(manageType);
        }

        public override void Dispose()
        {
            base.Dispose();

#if DEBUG && LOG
            _logStream.Dispose();
#endif
        }
    }
}
