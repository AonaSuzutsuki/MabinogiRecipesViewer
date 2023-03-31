using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommonExtensionLib.Extensions;
using CookInformationViewer.Models.Db.Raw;
using Newtonsoft.Json.Linq;
using SavannahXmlLib.XmlWrapper;

namespace CookInformationViewer.Models.Db.Loader
{
    public class SqlLoader : IDisposable
    {
        private readonly Stream _stream;
        private readonly SavannahXmlReader _reader;
        private TemplateLoader? _templateLoader;
        private readonly Dictionary<string, SelectParameter> _parameters = new();
        private string _sqlId = string.Empty;
        
        public SqlLoader(string fileName, string @namespace = "CookInformationViewer.Resources.SQL", Assembly? assembly = null)
        {
            var (stream, reader) = Load(fileName, @namespace, assembly ?? Assembly.GetExecutingAssembly());
            _stream = stream;
            _reader = reader;
        }

        private (Stream stream, SavannahXmlReader reader) Load(string fileName, string @namespace, Assembly assembly)
        {
            var stream = assembly.GetManifestResourceStream($"{@namespace}.{fileName}");
            if (stream == null)
                throw new FileNotFoundException();

            var reader = new SavannahXmlReader(stream)
            {
                IndentSize = 4
            };

            return (stream, reader);
        }

        public void SetQuery(string id)
        {
            _sqlId = id;

           var node =  _reader.GetNode($"/queries/query[@id='{id}']/sql");
           if (node == null)
               return;

           var sql = node.InnerText;

           _templateLoader = new TemplateLoader(sql);
        }

        public void SetParameter(string name, string value)
        {
            _parameters.Put(name, new SelectParameter
            {
                ColumnName = name,
                Value = value
            });
            _templateLoader?.Assign(name, $"@{name}");
        }

        public SelectParameter[] GetParameters()
        {
            return _parameters.Values.ToArray();
        }

        public void SetStatement(string name)
        {
            var node = _reader.GetNode($"/queries/query[@id='{_sqlId}']/states/state[@id='{name}']");
            if (node == null)
                return;

            var cond = node.InnerText;

            _templateLoader?.Assign(name, cond);
        }

        public override string ToString()
        {
            return _templateLoader?.ToString() ?? string.Empty;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
