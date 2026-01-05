using acquleo.Protocol.Serializer;
using easyLNC.Abstract.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace easyLNC.ServerTransport.TcpIp
{
    internal class XmlMessageSerializerInfo : IXmlMessageSerializerInfo
    {
        Dictionary<string, XmlSerializer> _typeMapping = new Dictionary<string, XmlSerializer>();

        public XmlMessageSerializerInfo()
        {
            _typeMapping.Add(typeof(MouseEnterScreen).Name, new XmlSerializer(typeof(MouseEnterScreen)));
            _typeMapping.Add(typeof(MouseLeaveScreen).Name, new XmlSerializer(typeof(MouseLeaveScreen)));
            _typeMapping.Add(typeof(SessionEndReq).Name, new XmlSerializer(typeof(SessionEndReq)));
            _typeMapping.Add(typeof(SessionEndRes).Name, new XmlSerializer(typeof(SessionEndRes)));
            _typeMapping.Add(typeof(SessionKeepAlive).Name, new XmlSerializer(typeof(SessionKeepAlive)));
            _typeMapping.Add(typeof(SessionStartReq).Name, new XmlSerializer(typeof(SessionStartReq)));
            _typeMapping.Add(typeof(SessionStartRes).Name, new XmlSerializer(typeof(SessionStartRes)));
            _typeMapping.Add(typeof(MouseMove).Name, new XmlSerializer(typeof(MouseMove)));
            _typeMapping.Add(typeof(MouseWheel).Name, new XmlSerializer(typeof(MouseWheel)));
            _typeMapping.Add(typeof(MouseButtonAction).Name, new XmlSerializer(typeof(MouseButtonAction)));
            _typeMapping.Add(typeof(VirtualKeyDown).Name, new XmlSerializer(typeof(VirtualKeyDown)));
            _typeMapping.Add(typeof(VirtualKeyUp).Name, new XmlSerializer(typeof(VirtualKeyUp)));

        }
        public string GetContentType(Type type)
        {
            return type.Name;
        }

        public XmlSerializer GetXmlSerializer(string contentType)
        {
            if (_typeMapping.ContainsKey(contentType))
            {
                return _typeMapping[contentType];
            }
            throw new KeyNotFoundException($"XmlSerializer for content type '{contentType}' not found.");
        }

        public bool IsXmlSerializerPresent(Type type)
        {
            return _typeMapping.ContainsKey(type.Name);
        }

        public bool IsXmlSerializerPresent(string contentType)
        {
            return _typeMapping.ContainsKey(contentType);
        }
    }
}
